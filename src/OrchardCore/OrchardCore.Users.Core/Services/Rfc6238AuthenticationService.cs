using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using OrchardCore.Modules;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

/// <summary>
/// The following code is influenced by <see href="https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs"/>
/// </summary>
public sealed class Rfc6238AuthenticationService
{
    private static readonly UTF8Encoding _encoding = new(false, true);

    private readonly TimeSpan _timeSpan;
    private readonly TwoFactorEmailTokenLength _length;
    private readonly IClock _clock;

    private int? _modulo;
    private string _format;

    public Rfc6238AuthenticationService(
        TimeSpan timeSpan,
        TwoFactorEmailTokenLength length,
        IClock clock)
    {
        _timeSpan = timeSpan;
        _length = length;
        _clock = clock;
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

    private string GetStringFormat()
    {
        _format ??= _length switch
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

    public string GetString(int code)
        => code.ToString(GetStringFormat(), CultureInfo.InvariantCulture);

    public int ComputeTOTP(byte[] key, ulong timestepNumber, byte[] modifierBytes)
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

        Span<byte> hash = stackalloc byte[HMACSHA256.HashSizeInBytes];
        res = HMACSHA256.TryHashData(key, modifierCombinedBytes, hash, out var written);

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
        var delta = _clock.UtcNow - DateTimeOffset.UnixEpoch;

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

        var modifierBytes = modifier is not null ? _encoding.GetBytes(modifier) : null;

        // Check the current, previous, and next time steps.
        for (var i = -1; i <= 1; i++)
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
