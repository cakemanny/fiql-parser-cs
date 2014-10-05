using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cakemanny.FIQL
{
    public enum Symbol
    {
        none, // null type symbol
        comma,
        semicolon,
        lparen,
        rparen,
        equal,
        notequal,
        lessthan,
        greaterthan,
        lessequal,
        greaterequal,
        boolean,
        ident,
        date,
        number,
        stringtype,
        wildstring
    }
}
