using NUnit.Framework;

namespace Thesis.Relinq.UnitTests
{
    [TestFixture]
    public class ExampleTest
    {
        [Test]
        public void example_passing_test()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void example_failing_test()
        {
            Assert.IsTrue(false);
        }
    }
}