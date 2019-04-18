
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using N2.Http.Extensions;

namespace XUnitHttpClientTests
{
    public class BindingExtensionsShould
    {

        [Fact]
        public void BindSimpleObjects()
        {
            var foo = new Foo { Name = "name", Value = 1 };
            var bar = new Bar();
            bar.Bind(foo);
            Assert.Equal("name", bar.Name);
            Assert.Equal(1, bar.Value);

        }

        [Fact]
        public void BindFunctionObjects()
        {
            var foo = new FooFunc("name") { Value = 1 };
            var bar = new Bar();
            bar.Bind(foo);
            Assert.Equal("name", bar.Name);
            Assert.Equal(1, bar.Value);

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
        }

        public class FooFunc
        {
            private readonly string _name;
            public FooFunc(string name)
            {
                _name = name;
            }
            public string Name() { return _name; }
            public int Value { get; set; }
        }
    }
}
