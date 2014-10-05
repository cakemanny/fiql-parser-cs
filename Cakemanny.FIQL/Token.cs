using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cakemanny.FIQL
{
    public struct Token
    {
        public readonly Symbol symbol;
        public readonly String data;

        public Token(Symbol symbol, String data)
        {
            this.symbol = symbol;
            this.data = data;
        }

        override public string ToString()
        {
            return String.Format("({0} {1})", symbol, data);
        }
    }
}

