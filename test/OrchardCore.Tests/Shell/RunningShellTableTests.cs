using OrchardCore.Environment.Shell;
using Xunit;

namespace OrchardCore.Tests.Shell
{
    public class RunningShellTableTests
    {
        [Fact]
        public void NoShellsGiveNoMatch()
        {
            var table = new RunningShellTable();
            var match = table.Match("localhost", "/yadda");
            Assert.Null(match);
        }

        [Fact]
        public void DefaultShellMatchesByDefault()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            table.Add(settings);
            var match = table.Match("localhost", "/yadda");
            Assert.Equal(settings, match);
        }

        [Fact]
        public void DefaultShellMatchesAllPortsByDefault()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            table.Add(settings);
            var match = table.Match("localhost:443", "/yadda");
            Assert.Equal(settings, match);
        }

        [Fact]
        public void AnotherShellMatchesByHostHeader()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match("a.example.com", "/foo/bar");
            Assert.Equal(settingsA, match);
        }

        [Fact]
        public void DefaultStillCatchesWhenOtherShellsMiss()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match("b.example.com", "/foo/bar");
            Assert.Equal(settings, match);
        }

        [Fact]
        public void DefaultWontFallbackIfItHasCriteria()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match("b.example.com", "/foo/bar");
            Assert.Null(match);
        }

        [Fact]
        public void DefaultWillCatchRequestsIfItMatchesCriteria()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match("www.example.com", "/foo/bar");
            Assert.Equal(settings, match);
        }

        [Fact]
        public void NonDefaultCatchallWillFallbackIfNothingElseMatches()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName, RequestUrlHost = "www.example.com" };
            var settingsA = new ShellSettings { Name = "Alpha" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match("b.example.com", "/foo/bar");
            Assert.Equal(settingsA, match);
        }

        [Fact]
        public void DefaultCatchallIsFallbackEvenWhenOthersAreUnqualified()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "b.example.com" };
            var settingsG = new ShellSettings { Name = "Gamma" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);
            var match = table.Match("a.example.com", "/foo/bar");
            Assert.Equal(settings, match);
        }

        [Fact]
        public void PathAlsoCausesMatch()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlPrefix = "foo" };
            table.Add(settings);
            table.Add(settingsA);
            var match = table.Match("a.example.com", "/foo/bar");
            Assert.Equal(settingsA, match);
        }

        [Fact]
        public void PathAndHostMustBothMatch()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName, RequestUrlHost = "www.example.com", };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "wiki.example.com", RequestUrlPrefix = "foo" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "wiki.example.com", RequestUrlPrefix = "bar" };
            var settingsG = new ShellSettings { Name = "Gamma", RequestUrlHost = "wiki.example.com" };
            var settingsD = new ShellSettings { Name = "Delta", RequestUrlPrefix = "Quux" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);
            table.Add(settingsD);

            Assert.Equal(settingsA, table.Match("wiki.example.com", "/foo/bar"));
            Assert.Equal(settingsB, table.Match("wiki.example.com", "/bar/foo"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/baaz"));
            Assert.Equal(settings, table.Match("www.example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("www.example.com", "/bar/foo"));
            Assert.Equal(settings, table.Match("www.example.com", "/baaz"));
            Assert.Null(table.Match("a.example.com", "/foo/bar"));
                   
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/quux/quad"));
            Assert.Equal(settings, table.Match("www.example.com", "/quux/quad"));
            Assert.Equal(settingsD, table.Match("a.example.com", "/quux/quad"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/yarg"));
            Assert.Equal(settings, table.Match("www.example.com", "/yarg"));
            Assert.Null(table.Match("a.example.com", "/yarg"));
        }

        [Fact]
        public void PathAndHostMustMatchOnFullUrl()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName, RequestUrlHost = "www.example.com", };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "wiki.example.com", RequestUrlPrefix = "bar" };
            var settingsG = new ShellSettings { Name = "Gamma", RequestUrlHost = "wiki.example.com" };
            table.Add(settings);
            table.Add(settingsB);
            table.Add(settingsG);

            Assert.Equal(settingsB, table.Match("wiki.example.com", "/bar/foo"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/baaz"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/barbaz"));
        }
        [Fact]
        public void PathAloneWillMatch()
        {
            var table = new RunningShellTable();
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlPrefix = "foo" };
            table.Add(settingsA);

            Assert.Equal(settingsA, table.Match("wiki.example.com", "/foo/bar"));
            Assert.Null(table.Match("wiki.example.com", "/bar/foo"));
        }

        [Fact]
        public void HostNameMatchesRightmostIfRequestIsLonger()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "example.com" };
            table.Add(settings);
            table.Add(settingsA);
            Assert.Equal(settings, table.Match("www.example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("wiki.example.com", "/foo/bar"));
            Assert.Equal(settingsA, table.Match("example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("localhost", "/foo/bar"));
        }

        [Fact]
        public void HostNameMatchesRightmostIfStar()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "*.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            Assert.Equal(settingsA, table.Match("www.example.com", "/foo/bar"));
            Assert.Equal(settingsA, table.Match("wiki.example.com", "/foo/bar"));
            Assert.Equal(settingsA, table.Match("example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("localhost", "/foo/bar"));
        }

        [Fact]
        public void LongestMatchingHostHasPriority()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "www.example.com" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "*.example.com" };
            var settingsG = new ShellSettings { Name = "Gamma", RequestUrlHost = "wiki.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            table.Add(settingsG);

            Assert.Equal(settingsA, table.Match("www.example.com", "/foo/bar"));
            Assert.Equal(settingsG, table.Match("wiki.example.com", "/foo/bar"));
            Assert.Equal(settingsB, table.Match("username.example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("localhost", "/foo/bar"));
        }

        [Fact]
        public void ShellNameUsedToDistinctThingsAsTheyAreAdded()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "removed.example.com" };
            var settingsB = new ShellSettings { Name = "Alpha", RequestUrlHost = "added.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            table.Remove(settingsA);
            table.Add(settingsB);

            Assert.Equal(settings, table.Match("removed.example.com", "/foo/bar"));
            Assert.Equal(settingsB, table.Match("added.example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("localhost", "/foo/bar"));
        }

        [Fact]
        public void MultipleHostsOnShellAreAdded()
        {
            var table = new RunningShellTable();
            var settingsAlpha = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com,b.example.com" };
            var settingsBeta = new ShellSettings { Name = "Beta", RequestUrlHost = "c.example.com,d.example.com,e.example.com" };
            table.Add(settingsAlpha);
            table.Add(settingsBeta);

            Assert.Equal(settingsAlpha, table.Match("a.example.com", "/foo/bar"));
            Assert.Equal(settingsAlpha, table.Match("b.example.com", "/foo/bar"));
            Assert.Equal(settingsBeta, table.Match("c.example.com", "/foo/bar"));
            Assert.Equal(settingsBeta, table.Match("d.example.com", "/foo/bar"));
            Assert.Equal(settingsBeta, table.Match("e.example.com", "/foo/bar"));
        }

        [Fact]
        public void HostContainsSpaces()
        {
            var table = new RunningShellTable();
            var settingsAlpha = new ShellSettings { Name = "Alpha", RequestUrlHost = "   a.example.com,  b.example.com     " };
            table.Add(settingsAlpha);

            Assert.Equal(settingsAlpha, table.Match("a.example.com", "/foo/bar"));
            Assert.Equal(settingsAlpha, table.Match("b.example.com", "/foo/bar"));
        }

        [Fact]
        public void PortAreIgnoredIfNotSetInSettings()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            Assert.Equal(settingsA, table.Match("a.example.com:80", "/foo/bar"));
            Assert.Equal(settings, table.Match("foo.com:80", "/foo/bar"));
        }

        [Fact]
        public void ShouldNotFallBackToDefault()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com" };
            table.Add(settings);
            table.Add(settingsA);
            Assert.Equal(settingsA, table.Match("a.example.com", "/foo/bar"));
            Assert.Equal(settings, table.Match("foo.com", "/foo/bar"));
            Assert.Null(table.Match("foo.com", "/foo/bar", false));
        }

        [Fact]
        public void PortAreNotIgnoredIfSetInSettings()
        {
            var table = new RunningShellTable();
            var settings = new ShellSettings { Name = ShellHelper.DefaultShellName };
            var settingsA = new ShellSettings { Name = "Alpha", RequestUrlHost = "a.example.com:80" };
            var settingsB = new ShellSettings { Name = "Beta", RequestUrlHost = "a.example.com:8080" };
            table.Add(settings);
            table.Add(settingsA);
            table.Add(settingsB);
            Assert.Equal(settingsA, table.Match("a.example.com:80", "/foo/bar"));
            Assert.Equal(settingsB, table.Match("a.example.com:8080", "/foo/bar"));
            Assert.Equal(settings, table.Match("a.example.com:123", "/foo/bar"));
            Assert.Null(table.Match("a.example.com:123", "/foo/bar", false));
        }
    }
}
