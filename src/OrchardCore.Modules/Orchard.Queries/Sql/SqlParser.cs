using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using YesSql;

namespace Orchard.Queries.Sql
{
    public class SqlParser
    {
        private StringBuilder _builder;
        private Dictionary<string, object> _parameters;
        private ISqlDialect _dialect;
        private string _tablePrefix;
        private HashSet<string> _aliases;
        private ParseTree _tree;
        private static LanguageData language = new LanguageData(new SqlGrammar());
        private Stack<FormattingModes> _modes;

        private SqlParser(ParseTree tree, ISqlDialect dialect, string tablePrefix)
        {
            _tree = tree;
            _dialect = dialect;
            _tablePrefix = tablePrefix;
            _parameters = new Dictionary<string, object>();
            _builder = new StringBuilder(tree.SourceText.Length);
            _modes = new Stack<FormattingModes>();
        }

        public static bool TryParse(string sql, ISqlDialect dialect, string tablePrefix, out string query, out Dictionary<string, object> parameters, out IEnumerable<string> messages)
        {
            try
            {
                var tree = new Irony.Parsing.Parser(language).Parse(sql);

                if (tree.HasErrors())
                {
                    query = null;
                    parameters = null;

                    messages = tree
                        .ParserMessages
                        .Select(x => $"{x.Message} at line:{x.Location.Line}, col:{x.Location.Column}")
                        .ToArray();

                    return false;
                }

                var sqlParser = new SqlParser(tree, dialect, tablePrefix);
                sqlParser.Evaluate();

                query = sqlParser._builder.ToString();
                parameters = sqlParser._parameters;
                messages = Array.Empty<string>();

                return true;
            }
            catch (Exception e)
            {
                query = null;
                parameters = null;
                messages = new string[] { "Unexpected error: " + e.Message };
            }

            return false;
        }

        private void Evaluate()
        {
            PopulateAliases(_tree);
            var statementList = _tree.Root;

            foreach (var selectStatement in statementList.ChildNodes)
            {
                EvaluateSelectStatement(selectStatement);
                _builder.Append(";");
            }
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

        private void EvaluateSelectStatement(ParseTreeNode selectStatement)
        {
            _builder.Append("SELECT ");
            EvaluateSelectRestriction(selectStatement.ChildNodes[1]);
            EvaluateSelectorList(selectStatement.ChildNodes[2]);
            EvaluateFromClause(selectStatement.ChildNodes[3]);
            EvaluateWhereClause(selectStatement.ChildNodes[4]);
            EvaluateGroupClause(selectStatement.ChildNodes[5]);
            EvaluateHavingClause(selectStatement.ChildNodes[6]);
            EvaluateOrderClause(selectStatement.ChildNodes[7]);
        }

        private void EvaluateOrderClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.AppendLine().Append("ORDER BY ");

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
            _modes.Pop();
        }

        private void EvaluateHavingClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.AppendLine().Append("HAVING ");

            _modes.Push(FormattingModes.SelectClause);
            EvaluateExpression(parseTreeNode.ChildNodes[1]);
            _modes.Pop();
        }

        private void EvaluateGroupClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.AppendLine().Append("GROUP BY ");

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
            _modes.Pop();
        }

        private void EvaluateWhereClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                // EMPTY
                return;
            }

            _builder.AppendLine().Append("WHERE ");

            _modes.Push(FormattingModes.SelectClause);
            EvaluateExpression(parseTreeNode.ChildNodes[1]);
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
                // Term and Tuple are transient, to they appear directly
                case "Id":
                    EvaluateId(parseTreeNode);
                    break;
                case "boolean":
                    _builder.Append(AddParameter(parseTreeNode.ChildNodes[0].Term.Name == "TRUE"));
                    break;
                case "string":
                    _builder.Append(AddParameter(parseTreeNode.Token.ValueString));
                    break;
                case "number":
                    _builder.Append(AddParameter(parseTreeNode.Token.Value));
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
                    EvaluateSelectStatement(parseTreeNode.ChildNodes[0]);
                    _builder.Append(")");
                    break;
                case "inStmt":
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    _builder.Append(" IN (");
                    EvaluateExpressionList(parseTreeNode.ChildNodes[2]);
                    _builder.Append(")");
                    break;
                case "*":
                    _builder.Append("*");
                    break;
            }
        }

        private void EvaluateFunCall(ParseTreeNode funCall)
        {
            _builder.Append(funCall.ChildNodes[0].ChildNodes[0].Token.ValueString);
            _builder.Append("(");
            if (funCall.ChildNodes[1].ChildNodes[0].Term.Name == "selectStatement")
            {
                // selectStatement
                EvaluateSelectStatement(funCall.ChildNodes[1].ChildNodes[0]);
            }
            else if (funCall.ChildNodes[1].ChildNodes[0].Term.Name == "*")
            {
                _builder.Append("*");
            }
            else
            {
                // expressionList
                EvaluateExpressionList(funCall.ChildNodes[1].ChildNodes[0]);
            }
            _builder.Append(")");
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

            var aliasList = parseTreeNode.ChildNodes[1];

            if (aliasList.ChildNodes.Count > 0)
            {
                _builder.AppendLine().Append("FROM ");
            }

            _modes.Push(FormattingModes.FromClause);
            EvaluateAliasList(aliasList);

            var joins = parseTreeNode.ChildNodes[2];

            if (joins.ChildNodes.Count != 0)
            {
                _builder.AppendLine();

                var jointKindOpt = joins.ChildNodes[0];

                if (jointKindOpt.ChildNodes.Count > 0)
                {
                    _builder.Append(jointKindOpt.ChildNodes[0].Term.Name);
                }

                _builder.Append(" JOIN ");

                EvaluateAliasList(joins.ChildNodes[2]);

                _builder.Append(" ON ");
                _modes.Push(FormattingModes.SelectClause);

                EvaluateId(joins.ChildNodes[4]);

                _builder.Append(" = ");

                EvaluateId(joins.ChildNodes[6]);
            }
            _modes.Pop();
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
                    }

                    if (columnItem.ChildNodes.Count > 1)
                    {
                        // AS
                        EvaluateAliasOptional(columnItem.ChildNodes[1]);
                    }
                }

                _modes.Pop();
            }
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
            if (parseTreeNode.ChildNodes.Count > 0)
            {
                _builder.Append(parseTreeNode.ChildNodes[0].Term.Name).Append(" ");
            }
        }

        private string AddParameter(object value)
        {
            var parameterName = "@p" + _parameters.Count;
            _parameters.Add(parameterName, value);
            return parameterName;
        }
        private enum FormattingModes
        {
            SelectClause,
            FromClause
        }
    }
}
