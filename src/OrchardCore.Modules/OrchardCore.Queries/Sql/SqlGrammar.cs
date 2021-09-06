using Irony.Parsing;

namespace OrchardCore.Queries.Sql
{
    public class SqlGrammar : Grammar
    {
        public SqlGrammar() : base(false)
        {
            var comment = new CommentTerminal("comment", "/*", "*/");
            var lineComment = new CommentTerminal("line_comment", "--", "\n", "\r\n");
            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(lineComment);
            var number = new NumberLiteral("number");
            var string_literal = new StringLiteral("string", "'", StringOptions.AllowsDoubledQuote);
            var Id_simple = TerminalFactory.CreateSqlExtIdentifier(this, "id_simple"); //covers normal identifiers (abc) and quoted id's ([abc d], "abc d")
            var comma = ToTerm(",");
            var dot = ToTerm(".");
            var CREATE = ToTerm("CREATE");
            var NULL = ToTerm("NULL");
            var NOT = ToTerm("NOT");
            var ON = ToTerm("ON");
            var SELECT = ToTerm("SELECT");
            var FROM = ToTerm("FROM");
            var AS = ToTerm("AS");
            var COUNT = ToTerm("COUNT");
            var JOIN = ToTerm("JOIN");
            var BY = ToTerm("BY");
            var TRUE = ToTerm("TRUE");
            var FALSE = ToTerm("FALSE");
            var AND = ToTerm("AND");
            var OVER = ToTerm("OVER");
            var UNION = ToTerm("UNION");
            var ALL = ToTerm("ALL");

            //Non-terminals
            var Id = new NonTerminal("Id");
            var statement = new NonTerminal("stmt");
            var selectStatement = new NonTerminal("selectStatement");
            var idlist = new NonTerminal("idlist");
            var aliaslist = new NonTerminal("aliaslist");
            var aliasItem = new NonTerminal("aliasItem");
            var orderList = new NonTerminal("orderList");
            var orderMember = new NonTerminal("orderMember");
            var orderDirOptional = new NonTerminal("orderDirOpt");
            var whereClauseOptional = new NonTerminal("whereClauseOpt");
            var expression = new NonTerminal("expression");
            var expressionList = new NonTerminal("exprList");
            var optionalSelectRestriction = new NonTerminal("optionalSelectRestriction");
            var selectorList = new NonTerminal("selList");
            var fromClauseOpt = new NonTerminal("fromClauseOpt");
            var groupClauseOpt = new NonTerminal("groupClauseOpt");
            var havingClauseOpt = new NonTerminal("havingClauseOpt");
            var orderClauseOpt = new NonTerminal("orderClauseOpt");
            var limitClauseOpt = new NonTerminal("limitClauseOpt");
            var offsetClauseOpt = new NonTerminal("offsetClauseOpt");
            var columnItemList = new NonTerminal("columnItemList");
            var columnItem = new NonTerminal("columnItem");
            var columnSource = new NonTerminal("columnSource");
            var asOpt = new NonTerminal("asOpt");
            var aliasOpt = new NonTerminal("aliasOpt");
            var tuple = new NonTerminal("tuple");
            var joinChainOpt = new NonTerminal("joinChainOpt");
            var joinStatement = new NonTerminal("joinStatement");
            var joinKindOpt = new NonTerminal("joinKindOpt");
            var joinConditions = new NonTerminal("joinConditions");
            var joinCondition = new NonTerminal("joinCondition");
            var joinConditionArgument = new NonTerminal("joinConditionArgument");
            var term = new NonTerminal("term");
            var unExpr = new NonTerminal("unExpr");
            var unOp = new NonTerminal("unOp");
            var binExpr = new NonTerminal("binExpr");
            var binOp = new NonTerminal("binOp");
            var betweenExpr = new NonTerminal("betweenExpr");
            var inExpr = new NonTerminal("inExpr");
            var parSelectStatement = new NonTerminal("parSelectStmt");
            var notOpt = new NonTerminal("notOpt");
            var funCall = new NonTerminal("funCall");
            var parameter = new NonTerminal("parameter");
            var statementLine = new NonTerminal("stmtLine");
            var optionalSemicolon = new NonTerminal("semiOpt");
            var statementList = new NonTerminal("stmtList");
            var functionArguments = new NonTerminal("funArgs");
            var boolean = new NonTerminal("boolean");
            var overClauseOpt = new NonTerminal("overClauseOpt");
            var overArgumentsOpt = new NonTerminal("overArgumentsOpt");
            var overPartitionByClauseOpt = new NonTerminal("overPartitionByClauseOpt");
            var overOrderByClauseOpt = new NonTerminal("overOrderByClauseOpt");
            var unionStatementList = new NonTerminal("unionStmtList");
            var unionStatement = new NonTerminal("unionStmt");
            var unionClauseOpt = new NonTerminal("unionClauseOpt");

            //BNF Rules
            this.Root = statementList;
            unionClauseOpt.Rule = Empty | UNION | UNION + ALL;
            unionStatement.Rule = statement + unionClauseOpt;
            unionStatementList.Rule = MakePlusRule(unionStatementList, unionStatement);

            statementLine.Rule = unionStatementList + optionalSemicolon;
            optionalSemicolon.Rule = Empty | ";";
            statementList.Rule = MakePlusRule(statementList, statementLine);

            statement.Rule = selectStatement;

            Id.Rule = MakePlusRule(Id, dot, Id_simple);

            aliasOpt.Rule = Empty | asOpt + Id;
            asOpt.Rule = Empty | AS;

            idlist.Rule = MakePlusRule(idlist, comma, columnSource);

            aliaslist.Rule = MakePlusRule(aliaslist, comma, aliasItem);
            aliasItem.Rule = Id + aliasOpt;

            //Create Index
            orderList.Rule = MakePlusRule(orderList, comma, orderMember);
            orderMember.Rule = Id + orderDirOptional;
            orderDirOptional.Rule = Empty | "ASC" | "DESC";

            //Select stmt
            selectStatement.Rule = SELECT + optionalSelectRestriction + selectorList + fromClauseOpt + whereClauseOptional +
                              groupClauseOpt + havingClauseOpt + orderClauseOpt + limitClauseOpt + offsetClauseOpt;
            optionalSelectRestriction.Rule = Empty | "ALL" | "DISTINCT";
            selectorList.Rule = columnItemList | "*";
            columnItemList.Rule = MakePlusRule(columnItemList, comma, columnItem);
            columnItem.Rule = columnSource + aliasOpt;

            columnSource.Rule = funCall + overClauseOpt | Id;
            fromClauseOpt.Rule = Empty | FROM + aliaslist + joinChainOpt;

            joinChainOpt.Rule = MakeStarRule(joinChainOpt, joinStatement);
            joinStatement.Rule = joinKindOpt + JOIN + aliaslist + ON + joinConditions;
            joinConditions.Rule = MakePlusRule(joinConditions, AND, joinCondition);
            joinCondition.Rule = joinConditionArgument + "=" + joinConditionArgument;
            joinConditionArgument.Rule = Id | boolean | string_literal | number | parameter;
            joinKindOpt.Rule = Empty | "INNER" | "LEFT" | "RIGHT";

            whereClauseOptional.Rule = Empty | "WHERE" + expression;
            groupClauseOpt.Rule = Empty | "GROUP" + BY + idlist;
            havingClauseOpt.Rule = Empty | "HAVING" + expression;
            orderClauseOpt.Rule = Empty | "ORDER" + BY + orderList;
            limitClauseOpt.Rule = Empty | "LIMIT" + expression;
            offsetClauseOpt.Rule = Empty | "OFFSET" + expression;

            overPartitionByClauseOpt.Rule = Empty | "PARTITION" + BY + columnItemList;
            overOrderByClauseOpt.Rule = Empty | "ORDER" + BY + orderList;
            overArgumentsOpt.Rule = Empty | overPartitionByClauseOpt + overOrderByClauseOpt;
            overClauseOpt.Rule = Empty | OVER + "(" + overArgumentsOpt + ")";

            //Expression
            expressionList.Rule = MakePlusRule(expressionList, comma, expression);
            expression.Rule = term | unExpr | binExpr | betweenExpr | inExpr | parameter;
            term.Rule = Id | boolean | string_literal | number | funCall | tuple | parSelectStatement;
            boolean.Rule = TRUE | FALSE;
            tuple.Rule = "(" + expressionList + ")";
            parSelectStatement.Rule = "(" + selectStatement + ")";
            unExpr.Rule = unOp + term;
            unOp.Rule = NOT | "+" | "-" | "~";
            binExpr.Rule = expression + binOp + expression;
            binOp.Rule = ToTerm("+") | "-" | "*" | "/" | "%" //arithmetic
                       | "&" | "|" | "^"                     //bit
                       | "=" | ">" | "<" | ">=" | "<=" | "<>" | "!=" | "!<" | "!>"
                       | "AND" | "OR" | "LIKE" | "NOT LIKE";
            betweenExpr.Rule = expression + notOpt + "BETWEEN" + expression + "AND" + expression;
            inExpr.Rule = expression + notOpt + "IN" + "(" + functionArguments + ")";
            notOpt.Rule = Empty | NOT;
            //funCall covers some pseudo-operators and special forms like ANY(...), SOME(...), ALL(...), EXISTS(...), IN(...)
            funCall.Rule = Id + "(" + functionArguments + ")";
            functionArguments.Rule = Empty | selectStatement | expressionList | "*";
            parameter.Rule = "@" + Id | "@" + Id + ":" + term;

            //Operators
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(9, "+", "-");
            RegisterOperators(8, "=", ">", "<", ">=", "<=", "<>", "!=", "!<", "!>", "LIKE", "IN");
            RegisterOperators(7, "^", "&", "|");
            RegisterOperators(6, NOT);
            RegisterOperators(5, "AND");
            RegisterOperators(4, "OR");

            MarkPunctuation(",", "(", ")");
            MarkPunctuation(asOpt, optionalSemicolon);
            //Note: we cannot declare binOp as transient because it includes operators "NOT LIKE", "NOT IN" consisting of two tokens.
            // Transient non-terminals cannot have more than one non-punctuation child nodes.
            // Instead, we set flag InheritPrecedence on binOp , so that it inherits precedence value from it's children, and this precedence is used
            // in conflict resolution when binOp node is sitting on the stack
            base.MarkTransient(statement, term, asOpt, aliasOpt, statementLine, expression, unOp, tuple);
            binOp.SetFlag(TermFlags.InheritPrecedence);
        }
    }
}
