using System.Buffers.Text;
using System.Security.Cryptography;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes;

public class UserByPasskeyIndex : MapIndex
{
    /// <summary>
    /// Gets or sets the hash of the passkey credential identifier.
    /// </summary>
    /// <remarks>
    /// A credential identifier can be up to 1023 bytes long, which doesn't fit in an index key on all databases,
    /// so the index stores a fixed-length hash of it.
    /// </remarks>
    public string CredentialIdHash { get; set; }

    /// <summary>
    /// Computes the value stored in <see cref="CredentialIdHash"/> for a credential identifier.
    /// </summary>
    /// <param name="credentialId">The passkey credential identifier.</param>
    public static string HashCredentialId(byte[] credentialId)
    {
        ArgumentNullException.ThrowIfNull(credentialId);

        return Base64Url.EncodeToString(SHA256.HashData(credentialId));
    }
}

public class UserByPasskeyIndexProvider : IndexProvider<User>
{
    public override void Describe(DescribeContext<User> context)
    {
        context.For<UserByPasskeyIndex>()
            .Map(user => user.Passkeys.Select(x => new UserByPasskeyIndex
            {
                CredentialIdHash = UserByPasskeyIndex.HashCredentialId(x.CredentialId),
            }));
    }
}
