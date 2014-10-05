using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cakemanny.FIQL
{
    /// <summary>
    /// This is thrown when an illegal ast construct is discovered such as
    /// an operator with a non-operator symbol
    /// </summary>
    public class AstException : Exception
    {
        public AstException(string message) : base(message) { }
    }
}
