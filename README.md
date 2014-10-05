# C# FIQL Parser

An implementation of parser of basic set of features of the Feed Item Query
Language http://tools.ietf.org/html/draft-nottingham-atompub-fiql-00

More precisely, it is a naive translation to SQL

## Known Limitations - Hopefully upcoming features
 * Only date, not time accepted in dates
 * Relative time/date not supported
 * Negative and floating point numbers not supported


This was put together based on the examples given in the apache-cxf docs
https://cxf.apache.org/docs/jax-rs-search.html

## Usage - Direct translation to SQL
Add a reference to your project.

```cs
using Cakemanny.FIQL;

public class YourController
{

    private readonly FIQLParser parser = new FIQLParser(new List<string>() {
        "field1", "field2", "field3"
    });

    public IEnumerable<Thing> Get(string search)
    {
        var myQuery = "select field1, field2, field3 from things where "
                + parser.parseQuery(search);
        /* Execute query */
        return allTheResults;
    }
}
```

You can also access the AST directly - the output of Cakemanny.FIQL.Parser is
this. The Visitor interface is designed to be extended to produce other forms
of output.

Copyright (c) 2014 Daniel Golding. Licensed under the Apache 2.0 license

