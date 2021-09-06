using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using YesSql;

namespace OrchardCore.Queries.Sql
{
    public class SqlParser
    {
        private StringBuilder _builder;
        private IDictionary<string, object> _parameters;
        private ISqlDialect _dialect;
        private string _tablePrefix;
        private HashSet<string> _aliases;
        private ParseTree _tree;
        private static LanguageData language = new LanguageData(new SqlGrammar());
        private Stack<FormattingModes> _modes;

        private string _limit;
        private string _offset;
        private string _select;
        private string _from;
        private string _where;
        private string _having;
        private string _groupBy;
        private string _orderBy;

        private SqlParser(ParseTree tree, ISqlDialect dialect, string tablePrefix, IDictionary<string, object> parameters)
        {
            _tree = tree;
            _dialect = dialect;
            _tablePrefix = tablePrefix;
            _parameters = parameters;
            _builder = new StringBuilder(tree.SourceText.Length);
            _modes = new Stack<FormattingModes>();
        }

        public static bool TryParse(string sql, ISqlDialect dialect, string tablePrefix, IDictionary<string, object> parameters, out string query, out IEnumerable<string> messages)
        {
            try
            {
                var tree = new Parser(language).Parse(sql);

                if (tree.HasErrors())
                {
                    query = null;

                    messages = tree
                        .ParserMessages
                        .Select(x => $"{x.Message} at line:{x.Location.Line}, col:{x.Location.Column}")
                        .ToArray();

                    return false;
                }

                var sqlParser = new SqlParser(tree, dialect, tablePrefix, parameters);
                query = sqlParser.Evaluate();

                messages = Array.Empty<string>();

                return true;
            }
            catch (SqlParserException se)
            {
                query = null;
                messages = new string[] { se.Message };
            }
            catch (Exception e)
            {
                query = null;
                messages = new string[] { "Unexpected error: " + e.Message };
            }

            return false;
        }

        private string Evaluate()
        {
            PopulateAliases(_tree);
            var statementList = _tree.Root;

            var statementsBuilder = new StringBuilder();

            foreach (var unionStatementList in statementList.ChildNodes)
            {
                foreach (var unionStatement in unionStatementList.ChildNodes)
                {
                    var selectStatement = unionStatement.ChildNodes[0];
                    var unionClauseOpt = unionStatement.ChildNodes[1];
                    statementsBuilder.Append(EvaluateSelectStatement(selectStatement));

                    for (var i = 0; i < unionClauseOpt.ChildNodes.Count; i++)
                    {
                        if (i == 0)
                        {
                            statementsBuilder.Append(" ");
                        }
                        var term = unionClauseOpt.ChildNodes[i].Term;
                        statementsBuilder.Append(term).Append(" ");
                    }
                }
                statementsBuilder.Append(';');
            }

            return statementsBuilder.ToString();
        }

        private void PopulateAliases(ParseTree tree)
        {
            // In order to determine if an Id is a table name or an alias, we
            // analyze every Alias and store the value.

            _aliases = new HashSet<string>();

            for (var i = 0; i < tree.Tokens.Count; i++)
            {
                if (tree.Tokens[i].Terminal.Name == "AS")
                {
                    _aliases.Add(tree.Tokens[i + 1].ValueString);
                }
            }
        }

        private string EvaluateSelectStatement(ParseTreeNode selectStatement)
        {
            _limit = null;
            _offset = null;
            _select = null;
            _from = null;
            _where = null;
            _having = null;
            _groupBy = null;
            _orderBy = null;

            var previousContent = _builder.Length > 0 ? _builder.ToString() : null;
            _builder.Clear();

            var sqlBuilder = _dialect.CreateBuilder(_tablePrefix);

            EvaluateSelectRestriction(selectStatement.ChildNodes[1]);
            EvaluateSelectorList(selectStatement.ChildNodes[2]);

            sqlBuilder.Select();
            sqlBuilder.Selector(_select);

            EvaluateFromClause(selectStatement.ChildNodes[3]);

            if (!String.IsNullOrEmpty(_from))
            {
                sqlBuilder.From(_from);
            }

            EvaluateWhereClause(selectStatement.ChildNodes[4]);

            if (!String.IsNullOrEmpty(_where))
            {
                sqlBuilder.WhereAnd(_where);
            }

            EvaluateGroupClause(selectStatement.ChildNodes[5]);

            if (!String.IsNullOrEmpty(_groupBy))
            {
                sqlBuilder.GroupBy(_groupBy);
            }

            EvaluateHavingClause(selectStatement.ChildNodes[6]);

            if (!String.IsNullOrEmpty(_having))
            {
                sqlBuilder.Having(_having);
            }

            EvaluateOrderClause(selectStatement.ChildNodes[7]);

            if (!String.IsNullOrEmpty(_orderBy))
            {
                sqlBuilder.OrderBy(_orderBy);
            }

            EvaluateLimitClause(selectStatement.ChildNodes[8]);

            if (!String.IsNullOrEmpty(_limit))
            {
                sqlBuilder.Take(_limit);
            }

            EvaluateOffsetClause(selectStatement.ChildNodes[9]);

            if (!String.IsNullOrEmpty(_offset))
            {
                sqlBuilder.Skip(_offset);
            }

            if (previousContent != null)
            {
                _builder.Clear();
                _builder.Append(new StringBuilder(previousContent));
            }

            return sqlBuilder.ToSqlString();
        }

        private void EvaluateLimitClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.Clear();

            // Evaluating so that the value can be transformed as a parameter
            EvaluateExpression(parseTreeNode.ChildNodes[1]);

            _limit = _builder.ToString();
        }

        private void EvaluateOffsetClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.Clear();

            // Evaluating so that the value can be transformed as a parameter
            EvaluateExpression(parseTreeNode.ChildNodes[1]);

            _offset = _builder.ToString();
        }

        private void EvaluateOrderClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.Clear();

            var idList = parseTreeNode.ChildNodes[2];

            _modes.Push(FormattingModes.SelectClause);
            for (var i = 0; i < idList.ChildNodes.Count; i++)
            {
                var id = idList.ChildNodes[i].ChildNodes[0];

                if (i > 0)
                {
                    _builder.Append(", ");
                }

                EvaluateId(id);

                if (idList.ChildNodes[i].ChildNodes[1].ChildNodes.Count > 0)
                {
                    _builder.Append(" ").Append(idList.ChildNodes[i].ChildNodes[1].ChildNodes[0].Term.Name);
                }
            }

            _orderBy = _builder.ToString();

            _modes.Pop();
        }

        private void EvaluateHavingClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.Clear();

            _modes.Push(FormattingModes.SelectClause);
            EvaluateExpression(parseTreeNode.ChildNodes[1]);

            _having = _builder.ToString();

            _modes.Pop();
        }

        private void EvaluateGroupClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.Clear();

            var idList = parseTreeNode.ChildNodes[2];

            _modes.Push(FormattingModes.SelectClause);
            for (var i = 0; i < idList.ChildNodes.Count; i++)
            {
                var columnSource = idList.ChildNodes[i];

                if (i > 0)
                {
                    _builder.Append(", ");
                }

                if (columnSource.ChildNodes[0].Term.Name == "Id")
                {
                    EvaluateId(columnSource.ChildNodes[0]);
                }
                else
                {
                    EvaluateFunCall(columnSource.ChildNodes[0]);
                }
            }

            _groupBy = _builder.ToString();

            _modes.Pop();
        }

        private void EvaluateWhereClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                // EMPTY
                return;
            }

            _builder.Clear();

            _modes.Push(FormattingModes.SelectClause);
            EvaluateExpression(parseTreeNode.ChildNodes[1]);

            _where = _builder.ToString();

            _modes.Pop();
        }

        private void EvaluateExpression(ParseTreeNode parseTreeNode)
        {
            switch (parseTreeNode.Term.Name)
            {
                case "unExpr":
                    _builder.Append(parseTreeNode.ChildNodes[0].Term.Name);
                    EvaluateExpression(parseTreeNode.ChildNodes[1]);
                    break;
                case "binExpr":
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    _builder.Append(" ");
                    _builder.Append(parseTreeNode.ChildNodes[1].ChildNodes[0].Term.Name).Append(" ");
                    EvaluateExpression(parseTreeNode.ChildNodes[2]);
                    break;
                case "betweenExpr":
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    _builder.Append(" ");
                    if (parseTreeNode.ChildNodes[1].ChildNodes.Count > 0)
                    {
                        _builder.Append("NOT ");
                    }
                    _builder.Append("BETWEEN ");
                    EvaluateExpression(parseTreeNode.ChildNodes[3]);
                    _builder.Append(" ");
                    _builder.Append("AND ");
                    EvaluateExpression(parseTreeNode.ChildNodes[5]);
                    break;
                case "inExpr":
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    _builder.Append(" ");
                    if (parseTreeNode.ChildNodes[1].ChildNodes.Count > 0)
                    {
                        _builder.Append("NOT ");
                    }
                    _builder.Append("IN (");
                    EvaluateInArgs(parseTreeNode.ChildNodes[3]);
                    _builder.Append(")");
                    break;
                // Term and Tuple are transient, to they appear directly
                case "Id":
                    EvaluateId(parseTreeNode);
                    break;
                case "boolean":
                    _builder.Append(_dialect.GetSqlValue(parseTreeNode.ChildNodes[0].Term.Name == "TRUE"));
                    break;
                case "string":
                    _builder.Append(_dialect.GetSqlValue(parseTreeNode.Token.ValueString));
                    break;
                case "number":
                    _builder.Append(_dialect.GetSqlValue(parseTreeNode.Token.Value));
                    break;
                case "funCall":
                    EvaluateFunCall(parseTreeNode);
                    break;
                case "exprList":
                    _builder.Append("(");
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    _builder.Append(")");
                    break;
                case "parSelectStmt":
                    _builder.Append("(");
                    _builder.Append(EvaluateSelectStatement(parseTreeNode.ChildNodes[0]));
                    _builder.Append(")");
                    break;
                case "parameter":
                    var name = parseTreeNode.ChildNodes[1].ChildNodes[0].Token.ValueString;

                    _builder.Append("@" + name);

                    if (_parameters != null && !_parameters.ContainsKey(name))
                    {
                        // If a parameter is not set and there is no default value, report it
                        if (parseTreeNode.ChildNodes.Count < 3)
                        {
                            throw new SqlParserException("Missing parameters: " + name);
                        }
                        else
                        {
                            if (parseTreeNode.ChildNodes[3].Token != null)
                            {
                                _parameters[name] = parseTreeNode.ChildNodes[3].Token.Value;
                            }
                            else
                            {
                                // example: true
                                if (parseTreeNode.ChildNodes[3].ChildNodes[0].Token != null)
                                {
                                    _parameters[name] = parseTreeNode.ChildNodes[3].ChildNodes[0].Token.Value;
                                }
                                else
                                {
                                    throw new SqlParserException("Unsupported syntax for parameter: " + name);
                                }
                            }
                        }
                    }

                    break;
                case "*":
                    _builder.Append("*");
                    break;
            }
        }

        private void EvaluateInArgs(ParseTreeNode inArgs)
        {
            if (inArgs.ChildNodes[0].Term.Name == "selectStatement")
            {
                // selectStatement
                _builder.Append(EvaluateSelectStatement(inArgs.ChildNodes[0]));
            }
            else
            {
                // expressionList
                EvaluateExpressionList(inArgs.ChildNodes[0]);
            }
        }

        private void EvaluateFunCall(ParseTreeNode funCall)
        {
            var funcName = funCall.ChildNodes[0].ChildNodes[0].Token.ValueString;
            IList<string> arguments;
            var tempBuilder = _builder;

            if (funCall.ChildNodes[1].ChildNodes.Count == 0) 
            {
                arguments = Array.Empty<string>();
            }
            else if (funCall.ChildNodes[1].ChildNodes[0].Term.Name == "selectStatement")
            {
                // selectStatement
                _builder = new StringBuilder();
                _builder.Append(EvaluateSelectStatement(funCall.ChildNodes[1].ChildNodes[0]));
                arguments = new string[] { _builder.ToString() };
                _builder = tempBuilder;
            }
            else if (funCall.ChildNodes[1].ChildNodes[0].Term.Name == "*")
            {
                arguments = new string[] { "*" };
            }
            else
            {
                // expressionList
                arguments = new List<string>();
                for (var i = 0; i < funCall.ChildNodes[1].ChildNodes[0].ChildNodes.Count; i++)
                {
                    _builder = new StringBuilder();
                    EvaluateExpression(funCall.ChildNodes[1].ChildNodes[0].ChildNodes[i]);
                    arguments.Add(_builder.ToString());
                    _builder = tempBuilder;
                }
            }

            _builder.Append(_dialect.RenderMethod(funcName, arguments.ToArray()));
        }

        private void EvaluateExpressionList(ParseTreeNode expressionList)
        {
            for (var i = 0; i < expressionList.ChildNodes.Count; i++)
            {
                if (i > 0)
                {
                    _builder.Append(", ");
                }

                EvaluateExpression(expressionList.ChildNodes[i]);
            }
        }

        private void EvaluateFromClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                // EMPTY
                return;
            }

            _builder.Clear();

            var aliasList = parseTreeNode.ChildNodes[1];

            _modes.Push(FormattingModes.FromClause);
            EvaluateAliasList(aliasList);
            _modes.Pop();

            var joins = parseTreeNode.ChildNodes[2];

            // process join statements
            if (joins.ChildNodes.Count != 0)
            {
                foreach (var joinStatement in joins.ChildNodes)
                {
                    _modes.Push(FormattingModes.FromClause);

                    var jointKindOpt = joinStatement.ChildNodes[0];

                    if (jointKindOpt.ChildNodes.Count > 0)
                    {
                        _builder.Append(" ").Append(jointKindOpt.ChildNodes[0].Term.Name);
                    }

                    _builder.Append(" JOIN ");

                    EvaluateAliasList(joinStatement.ChildNodes[2]);

                    _builder.Append(" ON ");

                    var joinConditions = joinStatement.ChildNodes[4].ChildNodes;

                    for (var i = 0; i < joinConditions.Count; i++)
                    {
                        if (i > 0)
                        {
                            _builder.Append(" AND ");
                        }
                        _modes.Push(FormattingModes.SelectClause);
                        var joinCondition = joinConditions[i];
                        EvaluateExpression(joinCondition.ChildNodes[0].ChildNodes[0]);
                        _builder.Append(" = ");
                        EvaluateExpression(joinCondition.ChildNodes[2].ChildNodes[0]);
                        _modes.Pop();
                    }
                }
            }

            _from = _builder.ToString();
        }

        private void EvaluateAliasList(ParseTreeNode aliasList)
        {
            for (var i = 0; i < aliasList.ChildNodes.Count; i++)
            {
                var aliasItem = aliasList.ChildNodes[i];

                if (i > 0)
                {
                    _builder.Append(", ");
                }

                EvaluateId(aliasItem.ChildNodes[0]);

                if (aliasItem.ChildNodes.Count > 1)
                {
                    EvaluateAliasOptional(aliasItem.ChildNodes[1]);
                }
            }
        }

        private void EvaluateSelectorList(ParseTreeNode parseTreeNode)
        {
            var selectorList = parseTreeNode.ChildNodes[0];

            if (selectorList.Term.Name == "*")
            {
                _builder.Append("*");
            }
            else
            {
                _modes.Push(FormattingModes.SelectClause);

                // columnItemList
                for (var i = 0; i < selectorList.ChildNodes.Count; i++)
                {
                    if (i > 0)
                    {
                        _builder.Append(", ");
                    }

                    var columnItem = selectorList.ChildNodes[i];

                    // columnItem
                    var columnSource = columnItem.ChildNodes[0];
                    var funCallOrId = columnSource.ChildNodes[0];
                    if (funCallOrId.Term.Name == "Id")
                    {
                        EvaluateId(funCallOrId);
                    }
                    else
                    {
                        EvaluateFunCall(funCallOrId);
                        var overClauseOpt = columnSource.ChildNodes[1];
                        if (overClauseOpt.ChildNodes.Count > 0) {
                            EvaluateOverClauseOptional(overClauseOpt);
                        }
                    }

                    if (columnItem.ChildNodes.Count > 1)
                    {
                        // AS
                        EvaluateAliasOptional(columnItem.ChildNodes[1]);
                    }
                }

                _modes.Pop();
            }

            _select = _builder.ToString();
        }

        private void EvaluateId(ParseTreeNode id)
        {
            switch (_modes.Peek())
            {
                case FormattingModes.SelectClause:
                    EvaluateSelectId(id);
                    break;
                case FormattingModes.FromClause:
                    EvaluateFromId(id);
                    break;
            }
        }

        private void EvaluateSelectId(ParseTreeNode id)
        {
            for (var i = 0; i < id.ChildNodes.Count; i++)
            {
                if (i == 0 && id.ChildNodes.Count > 1 && !_aliases.Contains(id.ChildNodes[i].Token.ValueString))
                {
                    _builder.Append(_dialect.QuoteForTableName(_tablePrefix + id.ChildNodes[i].Token.ValueString));
                }
                else
                {
                    if (i > 0)
                    {
                        _builder.Append(".");
                    }

                    if (_aliases.Contains(id.ChildNodes[i].Token.ValueString))
                    {
                        _builder.Append(id.ChildNodes[i].Token.ValueString);
                    }
                    else
                    {
                        _builder.Append(_dialect.QuoteForColumnName(id.ChildNodes[i].Token.ValueString));
                    }
                }
            }
        }

        private void EvaluateFromId(ParseTreeNode id)
        {
            for (var i = 0; i < id.ChildNodes.Count; i++)
            {
                if (i == 0 && !_aliases.Contains(id.ChildNodes[i].Token.ValueString))
                {
                    _builder.Append(_dialect.QuoteForTableName(_tablePrefix + id.ChildNodes[i].Token.ValueString));
                }
                else
                {
                    _builder.Append(_dialect.QuoteForColumnName(id.ChildNodes[i].Token.ValueString));
                }
            }
        }

        private void EvaluateAliasOptional(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count > 0)
            {
                _builder.Append(" AS ");
                _builder.Append(parseTreeNode.ChildNodes[0].Token.ValueString);
            }
        }

        private void EvaluateSelectRestriction(ParseTreeNode parseTreeNode)
        {
            _builder.Clear();

            if (parseTreeNode.ChildNodes.Count > 0)
            {
                _builder.Append(parseTreeNode.ChildNodes[0].Term.Name).Append(" ");
            }
        }

        private void EvaluateOverClauseOptional(ParseTreeNode overClauseOpt)
        {
            var overArgumentsOpt = overClauseOpt.ChildNodes[1];

            _builder.Append(" OVER ");
            _builder.Append("(");

            if (overArgumentsOpt.ChildNodes.Count == 0)
            {
                _builder.Append(")");
                return;
            }

            var overPartitionByClauseOpt = overArgumentsOpt.ChildNodes[0];
            var overOrderByClauseOpt = overArgumentsOpt.ChildNodes[1];

            var hasOverPartitionByClause = overPartitionByClauseOpt.ChildNodes.Count > 0;
            var hasOverOrderByClause = overOrderByClauseOpt.ChildNodes.Count > 0;

            if (hasOverPartitionByClause) {
                _builder.Append("PARTITION BY ");
                var columnItemList = overPartitionByClauseOpt.ChildNodes[2];
                for (var i = 0; i < columnItemList.ChildNodes.Count; i++) {
                    if (i > 0)
                    {
                        _builder.Append(", ");
                    }
                    var columnItem = columnItemList.ChildNodes[i];
                    var id = columnItem.ChildNodes[0].ChildNodes[0];
                    EvaluateSelectId(id);
                }
            }

            if (hasOverOrderByClause)
            {
                if (hasOverPartitionByClause) {
                    _builder.Append(" ");
                }

                _builder.Append("ORDER BY ");

                var orderList = overOrderByClauseOpt.ChildNodes[2];
                for (var i = 0; i < orderList.ChildNodes.Count; i++)
                {
                    if (i > 0)
                    {
                        _builder.Append(", ");
                    }
                    var orderMember = orderList.ChildNodes[i];
                    var id = orderMember.ChildNodes[0];
                    EvaluateSelectId(id);
                    var orderDirOpt = orderMember.ChildNodes[1];
                    if (orderDirOpt.ChildNodes.Count > 0) {
                        _builder.Append(" ").Append(orderDirOpt.ChildNodes[0].Term.Name);
                    }
                }
            }

            _builder.Append(")");
        }

        private enum FormattingModes
        {
            SelectClause,
            FromClause
        }
    }
}
