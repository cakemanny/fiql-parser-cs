using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cakemanny.FIQL
{
    public class SQLGenererator : Visitor<String>
    {

        private bool wildFlag = false;

        public String Visit(Ast.Predicate predicate) {
            try {
                wildFlag = (predicate.rvalue.Token.symbol == Symbol.wildstring);
                return predicate.ident.Accept(this)
                        + predicate.op.Accept(this)
                        + predicate.rvalue.Accept(this);
            } finally {
                wildFlag = false;
            }
        }

        public String Visit(Ast.Conjunction booleanTerm) {
            return String.Join(" AND ",
                    booleanTerm.Children.Select(child => child.Accept(this)));
        }

        public String Visit(Ast.Disjunction condition) {
            return "(" +
                String.Join(" OR ", condition.Children
                        .Select(child => child.Accept(this)))
                + ")";
        }

        public String Visit(Ast.Operator op) {
            switch (op.Token.symbol) {
                case Symbol.equal:
                    return wildFlag ? " LIKE " : " = ";
                case Symbol.greaterequal:
                    return " >= ";
                case Symbol.greaterthan:
                    return " > ";
                case Symbol.lessequal:
                    return " <= ";
                case Symbol.lessthan:
                    return " < ";
                case Symbol.notequal:
                    return wildFlag ? " NOT LIKE " : " <> ";
                default:
                    throw new AstException("Unexpected symbol for operator: " + op);

            }
        }

        public String Visit(Ast.Identifier identifier) {
            return identifier.Token.data;
        }

        public String Visit(Ast.Value value) {
            Token token = value.Token;
            switch (token.symbol) {
                case Symbol.boolean:
                case Symbol.number:
                    return token.data;
                case Symbol.date:
                case Symbol.stringtype:
                    return "'" + token.data + "'";
                case Symbol.wildstring:
                    return "'" + token.data.Replace("*", "%") + "'";
                default:
                    throw new AstException("Unexpected symbol for value: " + value);
            }
        }

    }
}

