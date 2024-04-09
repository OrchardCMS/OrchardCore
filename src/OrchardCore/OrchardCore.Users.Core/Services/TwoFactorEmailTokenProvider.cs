using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class TwoFactorEmailTokenProvider : IUserTwoFactorTokenProvider<IUser>
{
    private readonly Rfc6238AuthenticationService _service;
    private readonly TwoFactorEmailTokenProviderOptions _options;
    
    private string _format;

    public TwoFactorEmailTokenProvider(IOptions<TwoFactorEmailTokenProviderOptions> options)
    {
        _options = options.Value;
        _service = new Rfc6238AuthenticationService(options.Value.TokenLifespan, options.Value.TokenLength);
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<IUser> manager, IUser user)
        => Task.FromResult(manager is not null && user is not null);

    public async Task<string> GenerateAsync(string purpose, UserManager<IUser> manager, IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var token = await manager.CreateSecurityTokenAsync(user);
        var modifier = await GetUserModifierAsync(purpose, manager, user);

        return _service.GenerateCode(token, modifier)
            .ToString(GetStringFormat(), CultureInfo.InvariantCulture);
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<IUser> manager, IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!int.TryParse(token, out var code))
        {
            return false;
        }

        var securityToken = await manager.CreateSecurityTokenAsync(user);
        var modifier = await GetUserModifierAsync(purpose, manager, user);

        return securityToken != null &&
            _service.ValidateCode(securityToken, code, modifier);
    }

    private string _format;

    private string GetStringFormat()
    {
        // Number of 0's is length of the generated pin.
        _format ??= _options.TokenLength switch
        {
            TwoFactorEmailTokenLength.Two => "D2",
            TwoFactorEmailTokenLength.Three => "D3",
            TwoFactorEmailTokenLength.Four => "D4",
            TwoFactorEmailTokenLength.Five => "D5",
            TwoFactorEmailTokenLength.Six => "D6",
            TwoFactorEmailTokenLength.Seven => "D7",
            TwoFactorEmailTokenLength.Eight or TwoFactorEmailTokenLength.Default => "D8",
            _ => throw new NotSupportedException("Unsupported token length.")
        };

        return _format;
    }
    /// <summary>
    /// Returns a constant, provider and user unique modifier used for entropy in generated tokens from user information.
    /// </summary>
    /// <param name="purpose">The purpose the token will be generated for.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user a token should be generated for.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a constant modifier for the specified 
    /// <paramref name="user"/> and <paramref name="purpose"/>.
    /// </returns>
    private static async Task<string> GetUserModifierAsync(string purpose, UserManager<IUser> manager, IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var userId = await manager.GetUserIdAsync(user);

        return $"Totp:{purpose}:{userId}";
    }
}

/// <summary>
/// The following code is influenced by <see href="https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs"/>
/// </summary>
internal sealed class Rfc6238AuthenticationService
{
    private static readonly UTF8Encoding _encoding = new(false, true);

    private readonly TimeSpan _timeSpan;
    private readonly TwoFactorEmailTokenLength _length;
    private int? _modulo;

    public Rfc6238AuthenticationService(TimeSpan timeSpan, TwoFactorEmailTokenLength length)
    {
        _timeSpan = timeSpan;
        _length = length;
    }

    private int GetModuloValue()
    {
        // Number of 0's is length of the generated PIN.
        _modulo ??= _length switch
        {
            TwoFactorEmailTokenLength.Two => 100,
            TwoFactorEmailTokenLength.Three => 1000,
            TwoFactorEmailTokenLength.Four => 10000,
            TwoFactorEmailTokenLength.Five => 100000,
            TwoFactorEmailTokenLength.Six => 1000000,
            TwoFactorEmailTokenLength.Seven => 10000000,
            TwoFactorEmailTokenLength.Eight or TwoFactorEmailTokenLength.Default => 100000000,
            _ => throw new NotSupportedException("Unsupported token length.")
        };

        return _modulo.Value;
    }

    internal int ComputeTOTP(byte[] key, ulong timestepNumber, byte[] modifierBytes)
    {
        // See https://tools.ietf.org/html/rfc4226
        // We can add an optional modifier.
        Span<byte> timestepAsBytes = stackalloc byte[sizeof(long)];
        var res = BitConverter.TryWriteBytes(timestepAsBytes, IPAddress.HostToNetworkOrder((long)timestepNumber));
        Debug.Assert(res);

        var modifierCombinedBytes = timestepAsBytes;
        if (modifierBytes is not null)
        {
            modifierCombinedBytes = ApplyModifier(timestepAsBytes, modifierBytes);
        }

        Span<byte> hash = stackalloc byte[HMACSHA1.HashSizeInBytes];
        res = HMACSHA1.TryHashData(key, modifierCombinedBytes, hash, out var written);
        Debug.Assert(res);
        Debug.Assert(written == hash.Length);

        // Generate DT string.
        var offset = hash[hash.Length - 1] & 0xf;
        Debug.Assert(offset + 4 < hash.Length);
        var binaryCode = (hash[offset] & 0x7f) << 24
                            | (hash[offset + 1] & 0xff) << 16
                            | (hash[offset + 2] & 0xff) << 8
                            | (hash[offset + 3] & 0xff);

        return binaryCode % GetModuloValue();
    }

    private static byte[] ApplyModifier(Span<byte> input, byte[] modifierBytes)
    {
        var combined = new byte[checked(input.Length + modifierBytes.Length)];
        input.CopyTo(combined);
        Buffer.BlockCopy(modifierBytes, 0, combined, input.Length, modifierBytes.Length);

        return combined;
    }

    /// <summary>
    /// More info: https://tools.ietf.org/html/rfc6238#section-4
    /// </summary>
    private ulong GetCurrentTimeStepNumber()
    {
        var delta = DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch;

        return (ulong)(delta.Ticks / _timeSpan.Ticks);
    }

    public int GenerateCode(byte[] securityToken, string modifier = null)
    {
        ArgumentNullException.ThrowIfNull(securityToken);

        var currentTimeStep = GetCurrentTimeStepNumber();

        var modifierBytes = modifier is not null ? _encoding.GetBytes(modifier) : null;

        return ComputeTOTP(securityToken, currentTimeStep, modifierBytes);
    }

    public bool ValidateCode(byte[] securityToken, int code, string modifier = null)
    {
        ArgumentNullException.ThrowIfNull(securityToken);

        var currentTimeStep = GetCurrentTimeStepNumber();

        if (var modifierBytes = modifier is not null)
        {
            _encoding.GetBytes(modifier);
        }

        // Allow a variance of no greater than 9 minutes in either direction.
        for (var i = -2; i <= 2; i++)
        {
            var computedTOTP = ComputeTOTP(securityToken, (ulong)((long)currentTimeStep + i), modifierBytes);

            if (computedTOTP == code)
            {
                return true;
            }
        }

        // No match.
        return false;
    }
}
