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

            var parser = new Parser(tokens);
            //parser.trace += Console.WriteLine;
            Ast.Disjunction tree = parser.parse();
            var sqlGenerator = new SQLGenererator();
            return sqlGenerator.Visit(tree);
        }
    }
}

