using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;

using Cakemanny.FIQL;

namespace Cakemanny.FIQL.Tests
{
    [TestFixture]
    public class FIQLParserTest
    {
        private FIQLParser parser = new FIQLParser(new List<String>() {
                "audit_action_id", "user_id", "user",
                "action", "timestamp", "client", "ip_address"
        });

        [Test]
        public void CanHandleDateInGreaterEqualComparison()
        {
            string input = "timestamp=ge=2014-09-29";
            string expected = "(timestamp >= '2014-09-29')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }
    }
}
