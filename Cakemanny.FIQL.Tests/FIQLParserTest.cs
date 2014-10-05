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

        [Test]
        public void ConvertsACommaToAnOR()
        {
            string input = "user_id==5,action==money";
            string expected = "(user_id = 5 OR action = 'money')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void ConvertsASemicolonToAnAND()
        {
            string input = "user_id==5;action==money";
            string expected = "(user_id = 5 AND action = 'money')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void ErrorsOnUnkownFieldName()
        {
            try
            {
                parser.parseQuery("unknown_field==10");
                Assert.Fail("Expected a ParseException to be thrown");
            }
            catch (ParseException e)
            {
                Assert.That(e.Message, Contains.Substring("not valid fields")
                        & Contains.Substring("unknown_field"));
            }
        }

        [Test]
        public void ConvertsStarInStringToLIKE()
        {
            string input = "action==mon*y";
            string expected = "(action LIKE 'mon%y')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void CanHandleAnIpAddress()
        {
            string input = "ip_address==192.168.0.1";
            string expected = "(ip_address = '192.168.0.1')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void CanHandleIpv6()
        {
            string input = "ip_address==2001:0db8:85a3:0000:0000:8a2e:0370:7334";
            string expected = "(ip_address = '2001:0db8:85a3:0000:0000:8a2e:0370:7334')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void CanHandleIpv4Like()
        {
            string input = "ip_address==192.168.*";
            string expected = "(ip_address LIKE '192.168.%')";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void AcceptsMultipleParensAtFront()
        {
            string input = "((ip_address==1),ip_address==2),ip_address==3";
            string expected = "(((ip_address = 1) OR ip_address = 2) OR ip_address = 3)";
            Assert.That(parser.parseQuery(input), Is.EqualTo(expected));
        }

        [Test]
        public void CanHandleSuperLongInput_2048Chars()
        {
            // The max queryString length on IIS is by default 2048
            // Let's check whether we can overflow the stack with that sort
            // of space
            string input = new String('(', 2048);
            try
            {
                parser.parseQuery(input);
                Assert.Fail("Expected a ParseException");
            }
            catch (ParseException e)
            {
                Assert.That(e.Message, Is.EqualTo("predicate: must start with identifier"));
            }
        }
    }
}
