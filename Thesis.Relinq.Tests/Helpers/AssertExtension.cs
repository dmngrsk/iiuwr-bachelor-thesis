using Newtonsoft.Json;
using Xunit;

namespace Thesis.Relinq.Tests.Helpers
{
    public static class AssertExtension
    {
       public static void EqualByJson(object expected, object actual)
        {
            string expectedJson = JsonConvert.SerializeObject(expected, Formatting.Indented);
            string actualJson = JsonConvert.SerializeObject(actual, Formatting.Indented);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}