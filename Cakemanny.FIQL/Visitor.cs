using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cakemanny.FIQL
{
    public interface Visitor<T>
    {

        T Visit(Ast.Predicate predicate);
        T Visit(Ast.Conjunction conjunction);
        T Visit(Ast.Disjunction disjunction);
        T Visit(Ast.Operator op);
        T Visit(Ast.Identifier identifier);
        T Visit(Ast.Value value);

    }
}
