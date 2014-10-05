using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cakemanny.FIQL
{
    public class Parser2
    {
        private Symbol sym;
        private string data;

        // Keep track of symbols we were expecting, for error messages
        private readonly Stack<Symbol> rejected = new Stack<Symbol>();
        private readonly Stack<Token> accepted = new Stack<Token>();

        private readonly IEnumerator<Token> tokenEnumerator;

        public Parser2(IEnumerable<Token> tokens)
        {
            this.tokenEnumerator = tokens.GetEnumerator();
        }

        public Ast.Disjunction parse()
        {
            getsym();
            var tree = condition();
            expect(Symbol.none); // End
            return tree;
        }

        public event Action<string> trace = msg => { };

        private Token token(Symbol sym, String data)
        {
            return new Token(sym, data);
        }
        private Token lastToken()
        {
            return accepted.Pop();
        }

        private void getsym()
        {
            if (tokenEnumerator.MoveNext()) {
                var token = tokenEnumerator.Current;
                sym = token.symbol;
                data = token.data;
            } else {
                sym = default(Symbol);
                data = null;
            }
        }

        private ParseException error(String errorMessage)
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

        private bool accept(Symbol s)
        {
            trace("FIQLParser.Parser.accept(" + s + ")");
            trace("token=" + token(sym, data));
            if (sym == s) {
                rejected.Clear();
                accepted.Push(token(sym, data));
                trace("accepted");
                getsym();
                return true;
            }
            rejected.Push(s);
            return false;
        }

        private bool expect(Symbol s)
        {
            trace("FIQLParser.Parser.expect(" + s + ")");
            if (accept(s))
                return true;
            throw error("expect: unexpected symbol " +
                    new Token(sym, data) + ", " + expectedMessage());
        }

        private Ast.Value value()
        {
            trace("FIQLParser.Parser.value()");
            if (accept(Symbol.date)
            || accept(Symbol.number)
            || accept(Symbol.stringtype)) {
                return new Ast.Value(lastToken());
            } else {
                throw error("value: syntax error: " + expectedMessage());
            }
        }

        private Ast.Value equatable()
        {
            trace("FIQLParser.Parser.equatable()");
            if (accept(Symbol.date)
            || accept(Symbol.number)
            || accept(Symbol.wildstring)
            || accept(Symbol.stringtype)
            || accept(Symbol.boolean)
            ) {
                return new Ast.Value(lastToken());
            } else {
                throw error("predicate: syntax error, " + expectedMessage());
            }
        }

        private Ast.Predicate predicate()
        {
            trace("FIQLParser.Parser.predicate()");
            if (accept(Symbol.ident)) {
                var predicate = new Ast.Predicate();
                predicate.ident = new Ast.Identifier(lastToken());
                if (accept(Symbol.lessthan)
                    || accept(Symbol.lessequal)
                    || accept(Symbol.greaterthan)
                    || accept(Symbol.greaterequal)
                ) {
                    predicate.op = new Ast.Operator(lastToken());
                    predicate.rvalue = value();
                } else if (accept(Symbol.equal) || accept(Symbol.notequal)) {
                    predicate.op = new Ast.Operator(lastToken());
                    predicate.rvalue = equatable();
                } else {
                    throw error("predicate: " + expectedMessage());
                }
                return predicate;
            }
            throw error("predicate: must start with identifier");
        }

        private Ast.BooleanFactor booleanFactor()
        {
            trace("FIQLParser.Parser.booleanFactor()");
            if (accept(Symbol.lparen)) {
                Ast.BooleanFactor result = condition();
                expect(Symbol.rparen);
                return result;
            } else {
                return predicate();
            }
        }

        private Ast.Conjunction booleanTerm()
        {
            trace("FIQLParser.Parser.booleanTerm()");
            var termNode = new Ast.Conjunction();
            termNode.AddChild(booleanFactor());
            while (accept(Symbol.semicolon)) {
                termNode.AddChild(booleanFactor());
            }
            return termNode;
        }

        private Ast.Disjunction condition()
        {
            trace("FIQLParser.Parser.condition()");
            var conditionNode = new Ast.Disjunction();
            conditionNode.AddChild(booleanTerm());
            while (accept(Symbol.comma)) {
                conditionNode.AddChild(booleanTerm());
            }
            return conditionNode;
        }

    }
}
