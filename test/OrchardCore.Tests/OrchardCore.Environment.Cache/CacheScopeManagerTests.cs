using OrchardCore.Environment.Cache;

namespace OrchardCore.Tests.Environment.Cache
{
    public class CacheScopeManagerTests
    {
        [Fact]
        public void ScopesCanBeEnteredAndExited()
        {
            var scopeA = new CacheContext("a");
            var scopeB = new CacheContext("b");
            var scopeC = new CacheContext("c");
            var scopeD = new CacheContext("d");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();

            sut.EnterScope(scopeC);

            sut.ExitScope();
            sut.ExitScope();

            sut.EnterScope(scopeD);

            sut.ExitScope();
        }

        [Fact]
        public void ContextsAreBubbledUp1()
        {
            var scopeA = new CacheContext("a");
            var scopeB = new CacheContext("b").AddContext("1", "2");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Collection(scopeA.Contexts, context => Assert.Contains("1", context),
                                               context => Assert.Contains("2", context));

            Assert.Collection(scopeB.Contexts, context => Assert.Contains("1", context),
                                               context => Assert.Contains("2", context));
        }

        [Fact]
        public void ContextsAreBubbledUp2()
        {
            var scopeA = new CacheContext("a").AddContext("1", "2");
            var scopeB = new CacheContext("b");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Collection(scopeA.Contexts, context => Assert.Contains("1", context),
                                               context => Assert.Contains("2", context));

            Assert.False(scopeB.Contexts.Any());
        }

        [Fact]
        public void ContextsAreBubbledUp3()
        {
            var scopeA = new CacheContext("a").AddContext("3", "4");
            var scopeB = new CacheContext("b").AddContext("1", "2");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Collection(scopeA.Contexts, context => Assert.Contains("3", context),
                                               context => Assert.Contains("4", context),
                                               context => Assert.Contains("1", context),
                                               context => Assert.Contains("2", context));

            Assert.Collection(scopeB.Contexts, context => Assert.Contains("1", context),
                                               context => Assert.Contains("2", context));
        }

        [Fact]
        public void TagsAreBubbledUp1()
        {
            var scopeA = new CacheContext("a");
            var scopeB = new CacheContext("b").AddTag("1", "2");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Collection(scopeA.Tags, tag => Assert.Contains("1", tag),
                                           tag => Assert.Contains("2", tag));

            Assert.Collection(scopeB.Tags, tag => Assert.Contains("1", tag),
                                           tag => Assert.Contains("2", tag));
        }

        [Fact]
        public void TagsAreBubbledUp2()
        {
            var scopeA = new CacheContext("a").AddTag("1", "2");
            var scopeB = new CacheContext("b");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Collection(scopeA.Tags, tag => Assert.Contains("1", tag),
                                           tag => Assert.Contains("2", tag));

            Assert.False(scopeB.Tags.Any());
        }

        [Fact]
        public void TagsAreBubbledUp3()
        {
            var scopeA = new CacheContext("a").AddTag("3", "4");
            var scopeB = new CacheContext("b").AddTag("1", "2");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Collection(scopeA.Tags, tag => Assert.Contains("3", tag),
                                           tag => Assert.Contains("4", tag),
                                           tag => Assert.Contains("1", tag),
                                           tag => Assert.Contains("2", tag));

            Assert.Collection(scopeB.Tags, tag => Assert.Contains("1", tag),
                                           tag => Assert.Contains("2", tag));
        }

        [Fact]
        public void ExpiryOnIsBubbledUp1()
        {
            var scopeA = new CacheContext("a");
            var scopeB = new CacheContext("b").WithExpiryOn(new DateTime(2018, 10, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new DateTime(2018, 10, 26), scopeA.ExpiresOn);
            Assert.Equal(new DateTime(2018, 10, 26), scopeB.ExpiresOn);
        }

        [Fact]
        public void ExpiryOnIsBubbledUp2()
        {
            var scopeA = new CacheContext("a").WithExpiryOn(new DateTime(2018, 10, 26));
            var scopeB = new CacheContext("b");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new DateTime(2018, 10, 26), scopeA.ExpiresOn);
            Assert.Null(scopeB.ExpiresOn);
        }

        [Fact]
        public void ExpiryOnIsBubbledUp3()
        {
            var scopeA = new CacheContext("a").WithExpiryOn(new DateTime(2020, 10, 26));
            var scopeB = new CacheContext("b").WithExpiryOn(new DateTime(2018, 10, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new DateTime(2018, 10, 26), scopeA.ExpiresOn);
            Assert.Equal(new DateTime(2018, 10, 26), scopeB.ExpiresOn);
        }

        [Fact]
        public void ExpiryAfterIsBubbledUp1()
        {
            var scopeA = new CacheContext("a");
            var scopeB = new CacheContext("b").WithExpiryAfter(new TimeSpan(3, 30, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresAfter);
            Assert.Equal(new TimeSpan(3, 30, 26), scopeB.ExpiresAfter);
        }

        [Fact]
        public void ExpiryAfterIsBubbledUp2()
        {
            var scopeA = new CacheContext("a").WithExpiryAfter(new TimeSpan(3, 30, 26));
            var scopeB = new CacheContext("b");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresAfter);
            Assert.Null(scopeB.ExpiresAfter);
        }

        [Fact]
        public void ExpiryAfterIsBubbledUp3()
        {
            var scopeA = new CacheContext("a").WithExpiryAfter(new TimeSpan(5, 30, 26));
            var scopeB = new CacheContext("b").WithExpiryAfter(new TimeSpan(3, 30, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresAfter);
            Assert.Equal(new TimeSpan(3, 30, 26), scopeB.ExpiresAfter);
        }

        [Fact]
        public void ExpirySlidingIsBubbledUp1()
        {
            var scopeA = new CacheContext("a");
            var scopeB = new CacheContext("b").WithExpirySliding(new TimeSpan(3, 30, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresSliding);
            Assert.Equal(new TimeSpan(3, 30, 26), scopeB.ExpiresSliding);
        }

        [Fact]
        public void ExpirySlidingIsBubbledUp2()
        {
            var scopeA = new CacheContext("a").WithExpirySliding(new TimeSpan(3, 30, 26));
            var scopeB = new CacheContext("b");

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresSliding);
            Assert.Null(scopeB.ExpiresAfter);
        }

        [Fact]
        public void ExpirySlidingIsBubbledUp3()
        {
            var scopeA = new CacheContext("a").WithExpirySliding(new TimeSpan(5, 30, 26));
            var scopeB = new CacheContext("b").WithExpirySliding(new TimeSpan(3, 30, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeB);

            sut.ExitScope();
            sut.ExitScope();

            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresSliding);
            Assert.Equal(new TimeSpan(3, 30, 26), scopeB.ExpiresSliding);
        }

        [Fact]
        public void ComplexNesting()
        {
            var scopeA = new CacheContext("a")
                .AddContext("c1")
                .AddTag("t1");

            var scopeAA = new CacheContext("aa")
                .AddContext("c2")
                .WithExpiryAfter(new TimeSpan(0, 1, 0));

            var scopeAB = new CacheContext("ab")
                .WithExpirySliding(new TimeSpan(3, 30, 26))
                .WithExpiryAfter(new TimeSpan(0, 5, 0));

            var scopeABA = new CacheContext("aaa")
                .AddContext("deepestcontext")
                .AddTag("deepesttag")
                .WithExpiryOn(new DateTime(2018, 10, 26));

            var sut = new CacheScopeManager();

            sut.EnterScope(scopeA);
            sut.EnterScope(scopeAA);
            sut.ExitScope(); // scopeAA
            sut.EnterScope(scopeAB);
            sut.EnterScope(scopeABA);
            sut.ExitScope(); // scopeABA
            sut.ExitScope(); // scopeAB
            sut.ExitScope(); // scopeA

            // Scope A
            Assert.Collection(scopeA.Contexts, context => Assert.Contains("c1", context),
                                               context => Assert.Contains("c2", context),
                                               context => Assert.Contains("deepestcontext", context));

            Assert.Collection(scopeA.Tags, tag => Assert.Contains("t1", tag),
                                           tag => Assert.Contains("deepesttag", tag));

            Assert.Equal(new TimeSpan(0, 1, 0), scopeA.ExpiresAfter);
            Assert.Equal(new TimeSpan(3, 30, 26), scopeA.ExpiresSliding);
            Assert.Equal(new DateTime(2018, 10, 26), scopeA.ExpiresOn);

            // Scope AA
            Assert.Collection(scopeAA.Contexts, context => Assert.Contains("c2", context));

            Assert.False(scopeAA.Tags.Any());

            Assert.Equal(new TimeSpan(0, 1, 0), scopeAA.ExpiresAfter);
            Assert.Null(scopeAA.ExpiresSliding);
            Assert.Null(scopeAA.ExpiresOn);

            // Scope AB
            Assert.Collection(scopeAB.Contexts, context => Assert.Contains("deepestcontext", context));

            Assert.Collection(scopeAB.Tags, tag => Assert.Contains("deepesttag", tag));

            Assert.Equal(new TimeSpan(0, 5, 0), scopeAB.ExpiresAfter);
            Assert.Equal(new TimeSpan(3, 30, 26), scopeAB.ExpiresSliding);
            Assert.Equal(new DateTime(2018, 10, 26), scopeAB.ExpiresOn);

            // Scope ABA
            Assert.Collection(scopeABA.Contexts, context => Assert.Contains("deepestcontext", context));

            Assert.Collection(scopeABA.Tags, tag => Assert.Contains("deepesttag", tag));

            Assert.Equal(new DateTime(2018, 10, 26), scopeABA.ExpiresOn);
            Assert.Null(scopeABA.ExpiresSliding);
            Assert.Null(scopeABA.ExpiresAfter);
        }
    }
}
