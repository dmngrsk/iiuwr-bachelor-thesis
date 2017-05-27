using Dapper;
using Npgsql;
using System.Linq;
using System.Reflection;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;
using Xunit;
using System.Collections.Generic;
using System;

namespace Thesis.Relinq.Tests
{
    public class Foobar
    {
        public string Name { get; set; }
        public int Id { get; set; }
     
        public Foobar(string v1, int v2)
        {
            this.Name = v1;
            this.Id = v2;
        }
    }

    public class SandboxTests : TestClassBase
    {
        [Fact]
        public void sample_test()
        {
            var lol = new List<Foobar>()
            {
                new Foobar("hello", 1),
                new Foobar("hello", 2),
                new Foobar("hello", 3),
                new Foobar("world", 4),
                new Foobar("world", 5)
            };

            var lol2 = lol.GroupBy(i => i.Name);

            Assert.Equal(2, lol2.Count());

            foreach (var group in lol2)
            {
                Console.WriteLine("foo");
            }

        }
    }
}