using Coypu;
using Coypu.Drivers;
using Coypu.NUnit.Matchers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace OrchardVNext.AcceptanceTests.Features.Setup {
    [Binding]
    [Scope(Feature = "Setup")]
    public class SetupSteps : IDisposable {
        private Lazy<BrowserSession> _lazybrowser;
        private BrowserSession _browser {
            get {
                return _lazybrowser.Value;
            }
        }

        private Process _process;

        public SetupSteps() {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "D:\\Brochard\\run.cmd";
            process.StartInfo = startInfo;
            process.Start();

            _process = process;
            _lazybrowser = new Lazy<BrowserSession>(() => {
                var tenantOneUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["TenantOneUrl"]);
                return new BrowserSession(new SessionConfiguration {
                    AppHost = tenantOneUrl.Host,
                    SSL = false,
                    Port = tenantOneUrl.Port,
                    Browser = Browser.Chrome,
                    Timeout = TimeSpan.FromSeconds(5)
                });
            });
        }

        public void Dispose() {
            _process.Dispose();

            if (_lazybrowser.IsValueCreated) {
                _browser.Dispose();
            }

            if (Directory.Exists("D:\\Brochard\\src\\OrchardVNext.Web\\App_Data\\Sites")) {
                Directory.Delete("D:\\Brochard\\src\\OrchardVNext.Web\\App_Data\\Sites", true);
            }
        }

        [When(@"I create a site")]
        public void WhenICreateASite() {
            _browser.Visit("/setup/index");
            _browser.FillIn("What is the name of your site?").With("FooSite");
            _browser.ClickButton("Finish Setup");
        }

        [Then(@"I should be able to see that site")]
        public void ThenIShouldBeAbleToSeeThatSite() {
            Assert.That(_browser, Shows.Content("Hi from tenant Default - Orchard VNext Rocks"));
        }
    }
}