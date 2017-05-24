using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace Orchard.Queries.Sql
{
    public class SqlParser
    {
        private StringBuilder _builder;
        private Dictionary<string, object> _parameters;
        private string _tablePrefix;
        private HashSet<string> _aliases;
        private ParseTree _tree;
        private static LanguageData language = new LanguageData(new SqlGrammar());

        private SqlParser(ParseTree tree, string tablePrefix)
        {
            _tree = tree;
            _tablePrefix = tablePrefix;
            _parameters = new Dictionary<string, object>();
            _builder = new StringBuilder(tree.SourceText.Length);
        }

        public static bool TryParse(string sql, string tablePrefix, out string query, out Dictionary<string, object> parameters, out IEnumerable<string> messages)
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
                        .Select(x => $"{x.Message} ({x.Location.Line}:{x.Location.Column})")
                        .ToArray();

                    return false;
                }

                var sqlParser = new SqlParser(tree, tablePrefix);
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
            _builder.Append("SELECT");
            EvaluateSelectRestriction(selectStatement.ChildNodes[1]);
            EvaluateSelectorList(selectStatement.ChildNodes[2]);
            EvaluateFromClause(selectStatement.ChildNodes[3]);
            EvaluateWhereClause(selectStatement.ChildNodes[4]);
            EvaluateGroupClause(selectStatement.ChildNodes[5]);
            EvaluateHavingClause(selectStatement.ChildNodes[6]);
            EvaluateOrderClause(selectStatement.ChildNodes[7]);
            _builder.Append(";");
        }

        private void EvaluateOrderClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.AppendLine().Append("ORDER BY");

            var idList = parseTreeNode.ChildNodes[2];

            for (var i = 0; i < idList.ChildNodes.Count; i++)
            {
                var id = idList.ChildNodes[i].ChildNodes[0];

                if (i > 0)
                {
                    _builder.Append(",");
                }

                EvaluateId(id, true);

                if (idList.ChildNodes[i].ChildNodes[1].ChildNodes.Count > 0)
                {
                    _builder.Append(" ").Append(idList.ChildNodes[i].ChildNodes[1].ChildNodes[0].Term.Name);
                }
            }
        }

        private void EvaluateHavingClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.AppendLine().Append("HAVING");
            EvaluateExpression(parseTreeNode.ChildNodes[1]);
        }

        private void EvaluateGroupClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                return;
            }

            _builder.AppendLine().Append("GROUP BY");

            var idList = parseTreeNode.ChildNodes[2];

            for (var i = 0; i < idList.ChildNodes.Count; i++)
            {
                var id = idList.ChildNodes[i];

                if (i > 0)
                {
                    _builder.Append(",");
                }

                EvaluateId(id, true);
            }
        }

        private void EvaluateWhereClause(ParseTreeNode parseTreeNode)
        {
            if (parseTreeNode.ChildNodes.Count == 0)
            {
                // EMPTY
                return;
            }

            _builder.AppendLine().Append("WHERE");

            EvaluateExpression(parseTreeNode.ChildNodes[1]);
        }

        private void EvaluateExpression(ParseTreeNode parseTreeNode)
        {
            switch (parseTreeNode.Term.Name)
            {
                case "unExpr":
                    _builder.Append(" ").Append(parseTreeNode.ChildNodes[0].Term.Name);
                    EvaluateTerm(parseTreeNode.ChildNodes[1]);
                    break;
                case "binExpr":
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    _builder.Append(" ").Append(parseTreeNode.ChildNodes[1].ChildNodes[0].Term.Name);
                    EvaluateExpression(parseTreeNode.ChildNodes[2]);
                    break;
                case "betweenExpr":
                    EvaluateExpression(parseTreeNode.ChildNodes[0]);
                    if (parseTreeNode.ChildNodes[1].ChildNodes.Count > 0)
                    {
                        _builder.Append(" NOT");
                    }
                    _builder.Append(" BETWEEN");
                    EvaluateExpression(parseTreeNode.ChildNodes[3]);
                    _builder.Append(" AND");
                    EvaluateExpression(parseTreeNode.ChildNodes[5]);
                    break;
                default:
                    EvaluateTerm(parseTreeNode);
                    break;
            }

            void EvaluateTerm(ParseTreeNode term)
            {
                switch (term.Term.Name)
                {
                    case "Id":
                        EvaluateTermId(term);
                        break;
                    case "boolean":
                        _builder.Append(" ").Append(AddParameter(term.ChildNodes[0].Term.Name == "TRUE"));
                        break;
                    case "string":
                        _builder.Append(" ").Append(AddParameter(term.Token.ValueString));
                        break;
                    case "number":
                        _builder.Append(" ").Append(AddParameter(term.Token.Value));
                        break;
                    case "funCall":
                        EvaluateId(term.ChildNodes[0], false);
                        _builder.Append("(");
                        if (term.ChildNodes[1].ChildNodes[0].Term.Name == "selectStatement")
                        {
                            // selectStatement
                            EvaluateSelectStatement(term.ChildNodes[1].ChildNodes[0]);
                        }
                        else
                        {
                            // expressionList
                            EvaluateExpressionList(term.ChildNodes[1].ChildNodes[0]);
                        }
                        _builder.Append(")");
                        break;
                    case "tuple":
                        _builder.Append(" (");
                        EvaluateExpressionList(term.ChildNodes[1]);
                        _builder.Append(")");
                        break;
                    case "parSelectStmt":
                        _builder.Append(" (");
                        EvaluateSelectStatement(term.ChildNodes[1]);
                        _builder.Append(")");
                        break;
                    case "inStatement":
                        EvaluateExpression(term.ChildNodes[0]);
                        _builder.Append(" IN (");
                        EvaluateExpressionList(term.ChildNodes[2]);
                        _builder.Append(")");
                        break;
                }

                void EvaluateExpressionList(ParseTreeNode expressionList)
                {
                    for (var i = 0; i < expressionList.ChildNodes.Count; i++)
                    {
                        if (i > 0)
                        {
                            _builder.Append(",");
                        }

                        EvaluateExpression(expressionList.ChildNodes[i]);
                    }
                }
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
                _builder.AppendLine().Append("FROM");
            }

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

                _builder.Append(" JOIN");

                EvaluateAliasList(joins.ChildNodes[2]);

                _builder.Append(" ON");

                EvaluateId(joins.ChildNodes[4], true);

                _builder.Append(" =");

                EvaluateId(joins.ChildNodes[6], true);
            }
        }

        private void EvaluateAliasList(ParseTreeNode aliasList)
        {
            for (var i = 0; i < aliasList.ChildNodes.Count; i++)
            {
                var aliasItem = aliasList.ChildNodes[i];

                if (i > 0)
                {
                    _builder.Append(",");
                }

                EvaluateId(aliasItem.ChildNodes[0], true);

                if (aliasItem.ChildNodes.Count > 1)
                {
                    EvaluateAliasOptional(aliasItem.ChildNodes[1]);
                }
            }
        }

        private void EvaluateSelectorList(ParseTreeNode parseTreeNode)
        {
            var selectorList = parseTreeNode.ChildNodes[0];

            _builder.Append(" ");

            if (selectorList.Term.Name == "*")
            {
                _builder.Append("*");
            }
            else
            {
                // columnItemList
                for (var i = 0; i < selectorList.ChildNodes.Count; i++)
                {
                    if (i > 0)
                    {
                        _builder.Append(",");
                    }

                    var columnItem = selectorList.ChildNodes[i];

                    // columnItem
                    var columnSource = columnItem.ChildNodes[0];
                    var aggregateOrId = columnSource.ChildNodes[0];
                    if (aggregateOrId.Term.Name == "Id")
                    {
                        EvaluateId(aggregateOrId, true);
                    }
                    else
                    {
                        _builder.Append($" {aggregateOrId.ChildNodes[0].Token.ValueString} ({aggregateOrId.ChildNodes[1].Token.ValueString})");
                    }

                    if (columnItem.ChildNodes.Count > 1)
                    {
                        // AS
                        EvaluateAliasOptional(columnItem.ChildNodes[1]);
                    }
                }
            }
        }

        private void EvaluateId(ParseTreeNode id, bool prefix)
        {
            for (var i = 0; i < id.ChildNodes.Count; i++)
            {
                if (i == 0)
                {
                    _builder.Append(" ");
                }

                if (prefix && i == 0 && !_aliases.Contains(id.ChildNodes[i].Token.ValueString))
                {
                    _builder.Append(_tablePrefix);
                }

                if (i > 0)
                {
                    _builder.Append(".");
                }

                _builder.Append(id.ChildNodes[i].Token.Value);
            }
        }

        private void EvaluateTermId(ParseTreeNode id)
        {
            // These ids represent columns but as not aliasable (WHERE clause)
            for (var i = 0; i < id.ChildNodes.Count; i++)
            {
                if (i == 0)
                {
                    _builder.Append(" ");
                }

                if (i == 0 && id.ChildNodes.Count > 1 && !_aliases.Contains(id.ChildNodes[i].Token.ValueString))
                {
                    _builder.Append(_tablePrefix);
                }

                if (i > 0)
                {
                    _builder.Append(".");
                }

                _builder.Append(id.ChildNodes[i].Token.Value);
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
                _builder.Append(" ").Append(parseTreeNode.ChildNodes[0].Term.Name);
            }
        }

        private string AddParameter(object value)
        {
            var parameterName = "@p" + _parameters.Count;
            _parameters.Add(parameterName, value);
            return parameterName;
        }
    }
}
