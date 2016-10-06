using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Seekwell.Tests
{
    [TestClass]
    public class UnitTest1
    {
        sealed class Parent
        {
            public string Name { get; private set; }
            public Child A { get; private set; }
            public Child B { get; private set; }
        }

        sealed class Child
        {
            public int Id { get; private set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var command = new Command(@"Data Source=REVANS-PC\SQLEXPRESS;Database=Sandbox;Trusted_Connection=True;");
            var start = DateTime.Now;
            var result = command.Query<Parent>(
                @"Select 
                    'Parent' as 'Name'
                  , 1 as 'A.Id'
                  , 2 as 'B.Id'
                  , 'B' as 'B.Name'").First();
            var elapsed = DateTime.Now - start;
            Assert.AreEqual("Parent", result.Name);
            Assert.AreEqual(1, result.A.Id);
            Assert.AreEqual(2, result.B.Id);
            Assert.AreEqual("B", result.B.Name);
        }
    }
}
