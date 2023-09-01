using OrchardCore.Documents;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Options;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretServiceTests
{
    [Fact]
    public async Task ShouldGetTextSecret()
    {
        var textSecret = new TextSecret()
        {
            Text = "myemailpassword",
        };

        var store = Mock.Of<ISecretStore>();
        Mock.Get(store).Setup(s => s.GetSecretAsync("email", typeof(TextSecret))).ReturnsAsync(textSecret);
        var bindingsManager = Mock.Of<IDocumentManager<SecretBindingsDocument>>();

        Mock.Get(bindingsManager).Setup(m => m.GetOrCreateImmutableAsync(It.IsAny<Func<Task<SecretBindingsDocument>>>()))
            .ReturnsAsync(() =>
            {
                var document = new SecretBindingsDocument();
                document.SecretBindings["email"] = new SecretBinding() { Name = "email", Type = typeof(TextSecret).Name };
                return document;
            });

        var secretOptions = new SecretOptions();
        secretOptions.SecretTypes.Add(typeof(TextSecret));
        var options = Options.Create(secretOptions);

        var secretService = new SecretService(new SecretBindingsManager(bindingsManager), new[] { store }, options);
        var secret = await secretService.GetSecretAsync<TextSecret>("email");

        Assert.Equal(secret, textSecret);
    }
}
