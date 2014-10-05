using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cakemanny.FIQL
{
    public static class Ast
    {
        private static readonly Regex pppattern =
            new Regex(@"(?<open>\s*\{\s*)|(?<close>\s*\}\s*)|(?<other>[^{}]+)");

        /// <summary>
        /// By deafult, the Ast will print to one line; pretty print instead
        /// </summary>
        public static string PrettyPrintAst(Node node)
        {
            string astString = node.ToString();
            var sb = new StringBuilder();
            int indent = 0;
            const char indentStr = '\t';
            const char nl = '\n';

            Match match = pppattern.Match(astString);
            while (match.Success)
            {
                if (match.Groups["open"].Success) {
                    if (sb.Length != 0 && sb[sb.Length - 1] == ' ')
                        sb.Remove(sb.Length - 1, 1); // 2nd arg is length
                    sb.Append(nl)
                        .Append(new String(indentStr, indent))
                        .Append("{");
                    indent += 1;
                }
                if (match.Groups["close"].Success) {
                    if (sb.Length != 0 && sb[sb.Length - 1] == ' ')
                        sb.Remove(sb.Length - 1, 1); // 2nd arg is length
                    indent -= 1;
                    sb.Append(nl)
                        .Append(new String(indentStr, indent))
                        .Append("}");
                }
                if (match.Groups["other"].Success) {
                    sb.Append(nl)
                        .Append(new String(indentStr, indent))
                        .Append(match.Groups["other"].Value);
                }
                match = match.NextMatch();
            }
            return sb.ToString();
        }

        public class Predicate : BooleanFactor
        {
            internal Identifier ident;
            internal Operator op;
            internal Value rvalue;

            public Predicate() : base(new Token(Symbol.none, ""))
            {}

            override public string ToString()
            {
                return String.Format("Predicate { {0} {1} {2} }", ident, op, rvalue);
            }
            override public  T Accept<T>(Visitor<T> v)
            {
                return v.Visit(this);
            }
        }

        public class Conjunction : Node, ICompoundTerm
        {
            public List<Node> Children { get; private set; }

            public Conjunction() : base(new Token(Symbol.semicolon, ";"))
            {
                Children = new List<Node>();
            }

            override public string ToString()
            {
                return TermString(this);
            }

            override public T Accept<T>(Visitor<T> v)
            {
                return v.Visit(this);
            }
        }

        public interface ICompoundTerm
        {
            Token Token { get; }
            List<Node> Children { get; }
        }
        public static void AddChild(this ICompoundTerm term, Node newChild)
        {
            if (newChild == null) throw new ArgumentNullException("newChild");
            term.Children.Add(newChild);
        }

        private static string TermString(ICompoundTerm term)
        {
            const string typeName = "TODO"; // use reflection to get type name
            return typeName +
                " { " +
                string.Join("", term.Children.Select(x => x.ToString())) +
                " }";
        }

        public abstract class BooleanFactor : Node
        {
            public BooleanFactor(Token token) : base(token) {}
        }

        public class Disjunction : BooleanFactor, ICompoundTerm
        {
            public List<Node> Children { get; private set; }

            public Disjunction() : base(new Token(Symbol.comma, ","))
            {
                Children = new List<Node>();
            }

            override public string ToString()
            {
                return TermString(this);
            }

            override public T Accept<T>(Visitor<T> v)
            {
                return v.Visit(this);
            }
        }

        abstract public class Node
        {
            public Token Token { get; private set; }

            protected Node(Token token)
            {
                Token = token;
            }

            abstract public T Accept<T>(Visitor<T> v);
        }

        abstract public class Leaf : Node
        {
            public Leaf(Token token) : base(token) {}
        }

        public class Operator : Leaf
        {
            public Operator(Token token) : base(token) {}
            override public string ToString()
            {
                return "Operator" + Token;
            }

            override public T Accept<T>(Visitor<T> v)
            {
                return v.Visit(this);
            }
        }

        public class Identifier : Leaf
        {
            public Identifier(Token token) : base(token) {}
            override public string ToString()
            {
                return "Identifier" + Token;
            }

            override public T Accept<T>(Visitor<T> v)
            {
                return v.Visit(this);
            }
        }

        public class Value : Leaf
        {
            public Value(Token token) : base(token) {}
            override public string ToString()
            {
                return "Value" + Token;
            }

            override public T Accept<T>(Visitor<T> v)
            {
                return v.Visit(this);
            }
        }



    }
}
