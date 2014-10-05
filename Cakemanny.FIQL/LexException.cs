using System;

namespace Cakemanny.FIQL
{
    public class LexException : Exception
    {
        static readonly string nl = Environment.NewLine;
        private readonly string input;
        private readonly int position;

        override public string Message {
            get {
                return base.Message + nl + input + nl +
                    new String(' ', position) + "^";
            }
        }

        public LexException(string message, string input, int position)
            : base(message)
        {
            this.input = input;
            this.position = position;
        }
    }
}

