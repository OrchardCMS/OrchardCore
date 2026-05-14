using System.Text;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.DataProtection;

namespace OrchardCore.Liquid.Filters;

internal sealed class EncryptFilter : ILiquidFilter
{
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public EncryptFilter(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var protector = _dataProtectionProvider.CreateProtector(OrchardCoreConstants.Security.ScriptingEncryptionPurpose);
        var bytes = Encoding.UTF8.GetBytes(input?.ToStringValue() ?? string.Empty);
        var encryptedBytes = protector.Protect(bytes);
        return new ValueTask<FluidValue>(new StringValue(Convert.ToBase64String(encryptedBytes)));
    }
}

internal sealed class DecryptFilter : ILiquidFilter
{
    private readonly IDataProtectionProvider _dataProtectionProvider;

    public DecryptFilter(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtectionProvider = dataProtectionProvider;
    }

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var inputString = input?.ToStringValue();

        if (string.IsNullOrEmpty(inputString))
        {
            return new ValueTask<FluidValue>(EmptyValue.Instance);
        }

        try
        {
            var encryptedbytes = Convert.FromBase64String(inputString);

            var protector = _dataProtectionProvider.CreateProtector(OrchardCoreConstants.Security.ScriptingEncryptionPurpose);
            var bytes = protector.Unprotect(encryptedbytes);

            return new ValueTask<FluidValue>(new StringValue(Encoding.UTF8.GetString(bytes)));
        }
        catch (Exception) { }

        return new ValueTask<FluidValue>(NilValue.Instance);
    }
}
