using System;
using System.Text;
using System.Xml.Linq;
using OrchardCore.Security;

namespace OrchardCore.Workflows.Services
{
    public class SignalService : ISignalService
    {
        private readonly IEncryptionService _encryptionService;

        public SignalService(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public string CreateNonce(string correlationId, string signal)
        {
            var challengeToken = new XElement("n", new XAttribute("c", correlationId), new XAttribute("n", signal)).ToString();
            var data = Encoding.UTF8.GetBytes(challengeToken);
            return Convert.ToBase64String(_encryptionService.Encode(data));
        }

        public bool DecryptNonce(string nonce, out string correlationId, out string signal)
        {
            correlationId = null;
            signal = "";

            try
            {
                var data = _encryptionService.Decode(Convert.FromBase64String(nonce));
                var xml = Encoding.UTF8.GetString(data);
                var element = XElement.Parse(xml);
                correlationId = element.Attribute("c").Value;
                signal = element.Attribute("n").Value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}