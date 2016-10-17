using System.Linq;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Xunit;

namespace Orchard.Tests.ContentManagement
{
    public class ContentIdentityTests
    {
        [Fact]
        public void ContentIdentityParsesIdentities()
        {
            var identity1 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 'bar' }");

            Assert.Equal("bar", identity1.Get("foo"));
        }

        [Fact]
        public void ContentIdentityNonStringIdentities()
        {
            var identity1 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: null }");
            var identity2 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 123 }");

            Assert.False(identity1.Has("foo"));
            Assert.False(identity2.Has("foo"));
        }

        [Fact]
        public void ContentIdentityAreEqual()
        {
            var identity1 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 'bar' }");
            var identity2 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 'bar' }");

            Assert.Equal(identity1, identity2);
        }

        [Fact]
        public void ContentIdentityAreEquivalent()
        {
            var identity1 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 'bar', biz: 'buz' }");
            var identity2 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 'bar', baz: 'boz' }");

            Assert.Equal(identity1, identity2);
        }

        [Fact]
        public void ContentIdentityCanBeSerialized()
        {
            var identity1 = JsonConvert.DeserializeObject<ContentIdentity>(@"{ foo: 'bar', biz: 'buz' }");

            var text = JsonConvert.SerializeObject(identity1);

            var identity2 = JsonConvert.DeserializeObject<ContentIdentity>(text);


            Assert.Equal(identity1, identity2);
            Assert.Equal(2, identity2.Names.Count());
            Assert.True(identity2.Has("foo"));
            Assert.True(identity2.Has("biz"));
        }
    }
}
