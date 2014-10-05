using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Cakemanny.FIQL
{
    public class FIQLParser
    {
    /*
grammar:

condition = boolean_term *("," boolean_term)

boolean_term = boolean_factor *(";" boolean_factor)

boolean_factor = predicate
        | "(" condition ")"

predicate = ident (comparison_predicate | like_predicate)

comparison_predicate = comp_op value

like_predicate = "==" wildstring

comp_op = "==" | "!=" | "=lt=" | "=le=" | "=gt=" | "=ge="

value = date | number | bool | string | wildstring

date = \d{4}-\d\d-\d\d
number = [0-9]+
bool = true|false
string = [^=\*\(\)!,;]+
wildstring = [^=\(\)!,;]+

AST:

                                condition(,)
                     ... _______ ___|___ _______ _...
                        |       |       |       |
                boolean_term b_term   b_term  b_term(;)
            ... _____ __|____ ...
               |     |       |
              ==    !=      =ge=
             __|__   |       |
            |     |
          ident  897

Example:

harry==true;(data=le=1990-08-09,boy==6);(cat!=freedom;(cat==h*at,(cat==hat)))
becomes
(harry = true AND (data <= '1990-08-09' OR boy = 6) AND (cat <> 'freedom' AND (cat like 'h%at' OR (cat = 'hat'))))

     */

        private readonly List<String> fields;

        public FIQLParser(List<String> fields)
        {
            this.fields = fields;
        }

        public String parseQuery(String fiqlQuery)
        {
            var lexer = new Lexer(fiqlQuery);
            //lexer.trace += Console.WriteLine;
            var tokens = lexer.lex();

            List<String> illegalIdentifiers = tokens
                .Where(token => token.symbol == Symbol.ident)
                .Select(token => token.data)
                .Where(identifier => !fields.Contains(identifier))
                .Distinct()
                .ToList();

            if (illegalIdentifiers.Count() > 0) {
                throw new ParseException("These identifiers are not valid fields: " +
                    String.Join(", ", illegalIdentifiers));
            }

            var parser = new Parser2(tokens);
            //parser.trace += Console.WriteLine;
            Ast.Disjunction tree = parser.parse();
            var sqlGenerator = new SQLGenererator();
            return sqlGenerator.Visit(tree);
        }

        private String astToSQL(Node node)
        {
            switch (node.token.symbol)
            {
                case Symbol.ident:
                case Symbol.boolean:
                case Symbol.number:
                    return node.token.data;
                case Symbol.date:
                case Symbol.stringtype:
                    return "'" + node.token.data + "'";
                case Symbol.wildstring:
                    return "'" + node.token.data.Replace('*', '%') + "'";
                case Symbol.greaterequal:
                    return toSQLChildPair(" >= ", node);
                case Symbol.greaterthan:
                    return toSQLChildPair(" > ", node);
                case Symbol.lessequal:
                    return toSQLChildPair(" <= ", node);
                case Symbol.lessthan:
                    return toSQLChildPair(" < ", node);
                case Symbol.equal:
                    if (node.firstChild.nextSibling.token.symbol == Symbol.wildstring) {
                        return toSQLChildPair(" like ", node);
                    } else {
                        return toSQLChildPair(" = ", node);
                    }
                case Symbol.notequal:
                    if (node.firstChild.nextSibling.token.symbol == Symbol.wildstring) {
                        return toSQLChildPair(" not like ", node);
                    } else {
                        return toSQLChildPair(" <> ", node);
                    }
                case Symbol.comma:
                    return "(" + joinChildren(" OR ", node) + ")";
                case Symbol.semicolon:
                    return joinChildren(" AND ", node);
                default:
                    return " ";
            }
        }

        private string toSQLChildPair(string delim, Node node)
        {
            return astToSQL(node.firstChild) + delim + astToSQL(node.firstChild.nextSibling);
        }
        private string joinChildren(String delim, Node node)
        {
            Node child = node.firstChild;
            var sb = new StringBuilder(astToSQL(child));
            while (child.nextSibling != null) {
                child = child.nextSibling;
                sb.Append(delim).Append(astToSQL(child));
            }
            return sb.ToString();
        }

        class Node
        {
            internal Node firstChild;
            internal Node nextSibling;
            public readonly Token token;

            public Node(Token token)
            {
                this.token = token;
            }

            override public string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("{ ").Append(token).Append(" ");
                if (firstChild != null)
                    sb.Append(firstChild);
                sb.Append("}");
                if (nextSibling != null)
                    sb.Append(nextSibling);
                return sb.ToString();
            }
        }

        class Parser
        {
            private Symbol sym;
            private string data;

            // Keep track of symbols we were expecting, for error messages
            private readonly Stack<Symbol> rejected = new Stack<Symbol>();
            private readonly Stack<Token> accepted = new Stack<Token>();

            private readonly Func<Token> nextToken;

            public Parser(Func<Token> nextToken)
            {
                this.nextToken = nextToken;
            }

            public Node parse()
            {
                getsym();
                Node tree = condition();
                expect(Symbol.none); // End
                return tree;
            }

            public event Action<string> trace = msg => { };

            private Token token(Symbol sym, String data)
            {
                return new Token(sym, data);
            }
            private Token token()
            {
                return token(sym, data);
            }
            private Node node(Token token)
            {
                return new Node(token);
            }
            private Node node()
            {
                return node(accepted.Pop());
            }

            private void getsym()
            {
                var token = nextToken();
                sym = token.symbol;
                data = token.data;
            }

            ParseException error(String errorMessage)
            {
                return new ParseException(errorMessage);
            }

            private string expectedMessage()
            {
                if (rejected.Count > 0)
                    return "expected one of: " +
                        string.Join(", ", rejected.Select(x => x.ToString()));
                else
                    return "expected nothing";
            }

            bool accept(Symbol s)
            {
                trace("FIQLParser.Parser.accept(" + s + ")");
                trace("token=" + token());
                if (sym == s) {
                    rejected.Clear();
                    accepted.Push(token());
                    trace("accepted");
                    getsym();
                    return true;
                }
                rejected.Push(s);
                return false;
            }

            bool expect(Symbol s)
            {
                trace("FIQLParser.Parser.expect(" + s + ")");
                if (accept(s))
                    return true;
                throw error("expect: unexpected symbol " +
                        new Token(sym, data) + ", " + expectedMessage());
            }

            Node value()
            {
                trace("FIQLParser.Parser.value()");
                if (accept(Symbol.date)
                || accept(Symbol.number)
                || accept(Symbol.stringtype)) {
                    return node();
                } else {
                    throw error("value: syntax error: " + expectedMessage());
                }
            }

            Node equatable()
            {
                trace("FIQLParser.Parser.equatable()");
                if (accept(Symbol.date)
                || accept(Symbol.number)
                || accept(Symbol.wildstring)
                || accept(Symbol.stringtype)
                || accept(Symbol.boolean)
                ) {
                    return node();
                } else {
                    throw error("predicate: syntax error, " + expectedMessage());
                }
            }

            Node predicate()
            {
                trace("FIQLParser.Parser.predicate()");
                if (accept(Symbol.ident)) {
                    Node op = null, right = null;
                    Node left = node();
                    if (accept(Symbol.lessthan)
                        || accept(Symbol.lessequal)
                        || accept(Symbol.greaterthan)
                        || accept(Symbol.greaterequal)
                    ) {
                        op = node();
                        right = value();
                    } else if (accept(Symbol.equal) || accept(Symbol.notequal)) {
                        op = node();
                        right = equatable();
                    } else {
                        throw error("predicate: " + expectedMessage());
                    }
                    op.firstChild = left;
                    left.nextSibling = right;
                    return op;
                }
                throw error("predicate: must start with identifier");
            }

            Node booleanFactor()
            {
                trace("FIQLParser.Parser.booleanFactor()");
                if (accept(Symbol.lparen)) {
                    Node result = condition();
                    expect(Symbol.rparen);
                    return result;
                } else {
                    return predicate();
                }
            }

            Node booleanTerm()
            {
                trace("FIQLParser.Parser.booleanTerm()");
                Node termNode = node(token(Symbol.semicolon, ";"));
                Node currentChild = termNode.firstChild = booleanFactor();
                while (accept(Symbol.semicolon)) {
                    currentChild.nextSibling = booleanFactor();
                    currentChild = currentChild.nextSibling;
                }
                return termNode;
            }

            Node condition()
            {
                trace("FIQLParser.Parser.condition()");
                Node conditionNode = node(token(Symbol.comma, ","));
                Node currentChild = conditionNode.firstChild = booleanTerm();
                while (accept(Symbol.comma)) {
                    currentChild.nextSibling = booleanTerm();
                    currentChild = currentChild.nextSibling;
                }
                return conditionNode;
            }
        }

    }
}

