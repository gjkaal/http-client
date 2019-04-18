using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using N2.Http.Extensions;

namespace XUnitHttpClientTests
{
    public class QueryStringExtensionsShould
    {
        internal readonly ITestOutputHelper _outputHelper;

        public QueryStringExtensionsShould(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData("a", "a")]
        [InlineData(10.0d, "10")]
        [InlineData(10.1d, "10.1")]
        [InlineData(12.3e4, "123000")]
        public void SerializeSimpleObjects(object value, string expectResult)
        {
            var actual = value.AsQueryString();
            Assert.NotNull(actual);
            _outputHelper.WriteLine(actual);
            Assert.Equal(expectResult, actual);
        }

        [Fact]
        public void SerializeComplexObjects()
        {
            var foo = new Foo { Name = "name", Value = 1 };
            var actual = foo.AsQueryString();
            Assert.NotNull(actual);
            _outputHelper.WriteLine(actual);
            Assert.Equal("name=name&value=1", actual);
        }

        [Fact]
        public void SerializeComplexObjectsWithChildElements()
        {
            var foo = new Foo { Name = "name", Value = 1 };
            var bar = new Bar { Name = "bar", Value = 2, Foo = foo };
            var actual = bar.AsQueryString();
            Assert.NotNull(actual);
            _outputHelper.WriteLine(actual);
            Assert.Equal("name=bar&value=2&foo.name=name&foo.value=1", actual);
        }

        [Fact]
        public void SerializeArrays()
        {
            var list = new[] { 1, 2, 3, 4, 5 };
            var actual = list.AsQueryString();
            Assert.NotNull(actual);
            _outputHelper.WriteLine(actual);
            Assert.Equal("1,2,3,4,5", actual);
        }

        [Fact]
        public void SerializeDictionary()
        {
            var list = new[] { 1, 2, 3, 4, 5 };
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("list", list);
            var actual = dictionary.AsQueryString();
            Assert.NotNull(actual);
            _outputHelper.WriteLine(actual);
            Assert.Equal("list=1,2,3,4,5", actual);
        }

        [Fact]
        public void SerializeDictionaryWithMultipleObjects()
        {
            var list = new[] { 1, 2, 3, 4, 5 };
            var dictionary = new Dictionary<string, object>();
            dictionary.Add("list", list);
            dictionary.Add("integer", 12244);
            dictionary.Add("foo", new Foo { Name = "name", Value = 1 });
            const string expected = "list=1,2,3,4,5&integer=12244&foo.name=name&foo.value=1";
            var actual = dictionary.AsQueryString();
            Assert.NotNull(actual);
            _outputHelper.WriteLine(expected);
            _outputHelper.WriteLine(actual);
            Assert.Equal(expected, actual);
        }

        public class Foo
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public class Bar
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public Foo Foo { get; set; }
        }

    }
}
