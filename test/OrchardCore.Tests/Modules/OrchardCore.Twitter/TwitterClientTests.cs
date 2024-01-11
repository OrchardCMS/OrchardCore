using OrchardCore.Modules;
using OrchardCore.Twitter.Services;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Twitter
{
    public class TwitterClientTests
    {
        private const string ExpectedOauthHeader = "OAuth oauth_consumer_key=\"xvz1evFS4wEEPTGEFPHBog\", oauth_nonce=\"kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg\", oauth_signature=\"hCtSmYh%2BiHYCEqBWrE7C7hYmtUk%3D\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"1318622958\", oauth_token=\"370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb\", oauth_version=\"1.0\"";
        private const string UpdateStatusResponse = "{\r\n  \"created_at\": \"Fri Jan 25 22:04:26 +0000 2019\",\r\n  \"id\": 1088920554520961000,\r\n  \"id_str\": \"1088920554520961026\",\r\n  \"text\": \"okay\",\r\n  \"truncated\": false,\r\n  \"entities\": {\r\n    \"hashtags\": [],\r\n    \"symbols\": [],\r\n    \"user_mentions\": [],\r\n    \"urls\": []\r\n  },\r\n  \"source\": \"<a>Learning Twitter API - Jess Gars</a>\",\r\n  \"in_reply_to_status_id\": null,\r\n  \"in_reply_to_status_id_str\": null,\r\n  \"in_reply_to_user_id\": null,\r\n  \"in_reply_to_user_id_str\": null,\r\n  \"in_reply_to_screen_name\": null,\r\n  \"user\": {\r\n    \"id\": 15772978,\r\n    \"id_str\": \"15772978\",\r\n    \"name\": \"Jessica Garson\uD83E\uDD95\",\r\n    \"screen_name\": \"jessicagarson\",\r\n    \"location\": \"Brooklyn, NY\",\r\n    \"description\": \"Developer Advocate @Twitter. Python programmer. Noted thought leader on vegan snacks. Known sometimes as Messica Arson. She/They.\",\r\n    \"url\": \"https://t.co/ai7MDeRPa5\",\r\n    \"entities\": {\r\n      \"url\": {\r\n        \"urls\": [\r\n          {\r\n            \"url\": \"https://t.co/ai7MDeRPa5\",\r\n            \"expanded_url\": \"http://jessicagarson.com\",\r\n            \"display_url\": \"jessicagarson.com\",\r\n            \"indices\": [\r\n              0,\r\n              23\r\n            ]\r\n          }\r\n        ]\r\n      },\r\n      \"description\": {\r\n        \"urls\": []\r\n      }\r\n    },\r\n    \"protected\": false,\r\n    \"followers_count\": 2097,\r\n    \"friends_count\": 1986,\r\n    \"listed_count\": 88,\r\n    \"created_at\": \"Fri Aug 08 02:16:23 +0000 2008\",\r\n    \"favourites_count\": 9946,\r\n    \"utc_offset\": null,\r\n    \"time_zone\": null,\r\n    \"geo_enabled\": true,\r\n    \"verified\": false,\r\n    \"statuses_count\": 5514,\r\n    \"lang\": \"en\",\r\n    \"contributors_enabled\": false,\r\n    \"is_translator\": false,\r\n    \"is_translation_enabled\": false,\r\n    \"profile_background_color\": \"000000\",\r\n    \"profile_background_image_url\": \"http://abs.twimg.com/images/themes/theme6/bg.gif\",\r\n    \"profile_background_image_url_https\": \"https://abs.twimg.com/images/themes/theme6/bg.gif\",\r\n    \"profile_background_tile\": false,\r\n    \"profile_image_url\": \"http://pbs.twimg.com/profile_images/971062453453905920/DKzs-vf8_normal.jpg\",\r\n    \"profile_image_url_https\": \"https://pbs.twimg.com/profile_images/971062453453905920/DKzs-vf8_normal.jpg\",\r\n    \"profile_banner_url\": \"https://pbs.twimg.com/profile_banners/15772978/1520354408\",\r\n    \"profile_link_color\": \"FA743E\",\r\n    \"profile_sidebar_border_color\": \"000000\",\r\n    \"profile_sidebar_fill_color\": \"000000\",\r\n    \"profile_text_color\": \"000000\",\r\n    \"profile_use_background_image\": false,\r\n    \"has_extended_profile\": false,\r\n    \"default_profile\": false,\r\n    \"default_profile_image\": false,\r\n    \"following\": false,\r\n    \"follow_request_sent\": false,\r\n    \"notifications\": false,\r\n    \"translator_type\": \"none\"\r\n  },\r\n  \"geo\": null,\r\n  \"coordinates\": null,\r\n  \"place\": null,\r\n  \"contributors\": null,\r\n  \"is_quote_status\": false,\r\n  \"retweet_count\": 0,\r\n  \"favorite_count\": 0,\r\n  \"favorited\": false,\r\n  \"retweeted\": false,\r\n  \"lang\": \"en\"\r\n}";

        private readonly Mock<TwitterClientMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<FakeHttpMessageHandler> _mockFakeHttpMessageHandler;
        private readonly IDataProtectionProvider _mockDataProtectionProvider;
        public TwitterClientTests()
        {
            _mockFakeHttpMessageHandler = new Mock<FakeHttpMessageHandler>() { CallBase = true };

            var fakeDataProtector = new FakeDataProtector();
            var settings = new TwitterSettings
            {
                AccessToken = "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb",
                AccessTokenSecret = "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE",
                ConsumerKey = "xvz1evFS4wEEPTGEFPHBog",
                ConsumerSecret = "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw"
            };

            settings.AccessTokenSecret = fakeDataProtector.Protect(settings.AccessTokenSecret);
            settings.ConsumerSecret = fakeDataProtector.Protect(settings.ConsumerSecret);

            var signinSettings = new TwitterSigninSettings
            {
                CallbackPath = null,
            };

            _mockDataProtectionProvider = Mock.Of<IDataProtectionProvider>(dpp =>
                dpp.CreateProtector(It.IsAny<string>()) == fakeDataProtector
            );

            var ticks = 13186229580000000 + 621355968000000000;
            _mockHttpMessageHandler = new Mock<TwitterClientMessageHandler>(
                Mock.Of<IClock>(i => i.UtcNow == new DateTime(ticks)),
                Options.Create(settings),
                _mockDataProtectionProvider);
        }

        [Fact]
        /// <summary>
        /// Uses data from twitter's example to test the correct OAuth signature generation
        /// https://developer.twitter.com/en/docs/basics/authentication/guides/creating-a-signature.html
        /// </summary>
        public async Task HttpRequestMessageShouldHaveCorrectSignedOAuthHeader()
        {
            HttpRequestMessage message = null;

            _mockFakeHttpMessageHandler.Setup(c => c.Send(
                    It.IsAny<HttpRequestMessage>())
                ).Returns((HttpRequestMessage request) =>
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(UpdateStatusResponse),
                    }
                ).Callback<HttpRequestMessage>((msg) =>
                    message = msg
                );

            var mockTwitterClient = new Mock<TwitterClient>(
                new HttpClient(_mockFakeHttpMessageHandler.Object), Mock.Of<ILogger<TwitterClient>>())
            {
                CallBase = true,
            };

            _mockHttpMessageHandler.Setup(c => c.GetNonce())
                .Returns(() => "kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg");

            var result = await mockTwitterClient.Object.UpdateStatus("Hello Ladies + Gentlemen, a signed OAuth request!", "include_entities=true");

            await _mockHttpMessageHandler.Object.ConfigureOAuthAsync(message);

            Assert.NotNull(message.Headers.Authorization);
            Assert.Equal(ExpectedOauthHeader, message.Headers.Authorization.ToString());
        }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            throw new NotImplementedException("Now we can setup this method with our mocking framework");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request));
        }
    }

    public class FakeDataProtector : IDataProtector
    {
        public IDataProtector CreateProtector(string purpose)
        {
            throw new NotImplementedException();
        }

        public byte[] Protect(byte[] plaintext)
        {
            return plaintext;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return protectedData;
        }
    }
}
