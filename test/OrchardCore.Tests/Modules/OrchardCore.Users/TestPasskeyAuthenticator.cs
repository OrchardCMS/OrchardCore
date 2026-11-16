using System.Buffers.Binary;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

/// <summary>
/// A software authenticator that answers WebAuthn registrations and assertions like a browser would,
/// so that passkeys can be tested end to end.
/// </summary>
internal sealed class TestPasskeyAuthenticator : IDisposable
{
    private const byte UserPresentFlag = 0x01;
    private const byte UserVerifiedFlag = 0x04;
    private const byte AttestedCredentialDataFlag = 0x40;

    private readonly ECDsa _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);

    private uint _signCount;
    private string _userHandle;

    public byte[] CredentialId { get; } = RandomNumberGenerator.GetBytes(16);

    /// <summary>
    /// Gets or sets whether the authenticator reports that it verified the user, with a biometric or a PIN.
    /// </summary>
    public bool VerifiesUser { get; set; } = true;

    /// <summary>
    /// Answers the options of <c>navigator.credentials.create()</c>.
    /// </summary>
    public string CreateAttestation(string optionsJson, string origin)
    {
        var options = JsonNode.Parse(optionsJson);

        _userHandle = options["user"]["id"].GetValue<string>();

        var clientDataJson = CreateClientData("webauthn.create", options["challenge"].GetValue<string>(), origin);

        var authenticatorData = new List<byte>();
        authenticatorData.AddRange(CreateAuthenticatorDataHeader(options["rp"]["id"].GetValue<string>(), AttestedCredentialDataFlag));

        // The attested credential data: an empty AAGUID, then the credential identifier and its public key.
        authenticatorData.AddRange(new byte[16]);
        authenticatorData.AddRange(ToBigEndian((ushort)CredentialId.Length));
        authenticatorData.AddRange(CredentialId);
        authenticatorData.AddRange(EncodeCosePublicKey());

        var credential = new JsonObject
        {
            ["id"] = Base64Url.EncodeToString(CredentialId),
            ["rawId"] = Base64Url.EncodeToString(CredentialId),
            ["type"] = "public-key",
            ["authenticatorAttachment"] = "platform",
            ["clientExtensionResults"] = new JsonObject(),
            ["response"] = new JsonObject
            {
                ["clientDataJSON"] = Base64Url.EncodeToString(clientDataJson),
                ["attestationObject"] = Base64Url.EncodeToString(EncodeNoneAttestationObject([.. authenticatorData])),
                ["transports"] = new JsonArray("internal"),
            },
        };

        return credential.ToJsonString();
    }

    /// <summary>
    /// Answers the options of <c>navigator.credentials.get()</c>.
    /// </summary>
    /// <param name="optionsJson">The request options returned by the server.</param>
    /// <param name="origin">The origin of the page.</param>
    /// <param name="tamper">Whether to corrupt the signature, like an attacker without the private key would.</param>
    public string CreateAssertion(string optionsJson, string origin, bool tamper = false)
    {
        var options = JsonNode.Parse(optionsJson);

        _signCount++;

        var clientDataJson = CreateClientData("webauthn.get", options["challenge"].GetValue<string>(), origin);
        var authenticatorData = CreateAuthenticatorDataHeader(options["rpId"].GetValue<string>(), 0);

        var signature = _key.SignData([.. authenticatorData, .. SHA256.HashData(clientDataJson)], HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);

        if (tamper)
        {
            signature[^1] ^= 0xFF;
        }

        var credential = new JsonObject
        {
            ["id"] = Base64Url.EncodeToString(CredentialId),
            ["rawId"] = Base64Url.EncodeToString(CredentialId),
            ["type"] = "public-key",
            ["authenticatorAttachment"] = "platform",
            ["clientExtensionResults"] = new JsonObject(),
            ["response"] = new JsonObject
            {
                ["clientDataJSON"] = Base64Url.EncodeToString(clientDataJson),
                ["authenticatorData"] = Base64Url.EncodeToString(authenticatorData),
                ["signature"] = Base64Url.EncodeToString(signature),
                ["userHandle"] = _userHandle,
            },
        };

        return credential.ToJsonString();
    }

    public void Dispose()
    {
        _key.Dispose();
    }

    private static byte[] CreateClientData(string type, string challenge, string origin)
    {
        return JsonSerializer.SerializeToUtf8Bytes(new JsonObject
        {
            ["type"] = type,
            ["challenge"] = challenge,
            ["origin"] = origin,
            ["crossOrigin"] = false,
        });
    }

    private byte[] CreateAuthenticatorDataHeader(string rpId, byte flags)
    {
        flags |= UserPresentFlag;

        if (VerifiesUser)
        {
            flags |= UserVerifiedFlag;
        }

        return [.. SHA256.HashData(Encoding.UTF8.GetBytes(rpId)), flags, .. ToBigEndian(_signCount)];
    }

    // A COSE EC2 key: { 1 (kty): 2 (EC2), 3 (alg): -7 (ES256), -1 (crv): 1 (P-256), -2 (x): bytes, -3 (y): bytes }.
    private byte[] EncodeCosePublicKey()
    {
        var parameters = _key.ExportParameters(false);

        return [0xA5, 0x01, 0x02, 0x03, 0x26, 0x20, 0x01, 0x21, .. EncodeBytes(parameters.Q.X), 0x22, .. EncodeBytes(parameters.Q.Y)];
    }

    // { "fmt": "none", "attStmt": {}, "authData": bytes }.
    private static byte[] EncodeNoneAttestationObject(byte[] authenticatorData)
    {
        return [0xA3, .. EncodeText("fmt"), .. EncodeText("none"), .. EncodeText("attStmt"), 0xA0, .. EncodeText("authData"), .. EncodeBytes(authenticatorData)];
    }

    private static byte[] EncodeText(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        return [(byte)(0x60 | bytes.Length), .. bytes];
    }

    private static byte[] EncodeBytes(byte[] value)
    {
        return value.Length <= byte.MaxValue
            ? [0x58, (byte)value.Length, .. value]
            : [0x59, .. ToBigEndian((ushort)value.Length), .. value];
    }

    private static byte[] ToBigEndian(ushort value)
    {
        var bytes = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);

        return bytes;
    }

    private static byte[] ToBigEndian(uint value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);

        return bytes;
    }
}
