using System.Globalization;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Filters;

public class ResizeUrlFilter : ILiquidFilter
{
    private readonly IMediaProfileService _mediaProfileService;
    private readonly IMediaTokenService _mediaTokenService;
    private readonly MediaOptions _options;

    public ResizeUrlFilter(IMediaProfileService mediaProfileService, IMediaTokenService mediaTokenService, IOptions<MediaOptions> options)
    {
        _mediaProfileService = mediaProfileService;
        _mediaTokenService = mediaTokenService;
        _options = options.Value;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var url = input.ToStringValue();

        // Profile is a named argument only.
        var profile = arguments["profile"];

        var mediaCommands = new MediaCommands();
        FluidValue width, height, mode, quality, format, anchor, bgcolor;

        // Never mix named and indexed arguments as this leads to unpredictable results.
        // Additional commands to a profile must be named as well.
        var useNamed = !profile.IsNil() || arguments.Names.Any();

        if (!profile.IsNil())
        {
            mediaCommands.SetCommands(await _mediaProfileService.GetMediaProfileCommands(profile.ToStringValue()));
        }

        width = useNamed ? arguments["width"] : arguments.At(0);
        height = useNamed ? arguments["height"] : arguments.At(1);
        mode = useNamed ? arguments["mode"] : arguments.At(2);
        quality = useNamed ? arguments["quality"] : arguments.At(3);
        format = useNamed ? arguments["format"] : arguments.At(4);
        anchor = useNamed ? arguments["anchor"] : arguments.At(5);
        bgcolor = useNamed ? arguments["bgcolor"] : arguments.At(6);

        ApplyMediaCommands(mediaCommands, width, height, mode, quality, format, anchor, bgcolor);

        var resizedUrl = QueryHelpers.AddQueryString(url, mediaCommands.GetValues());

        if (_options.UseTokenizedQueryString)
        {
            resizedUrl = _mediaTokenService.AddTokenToPath(resizedUrl);
        }

        return new StringValue(resizedUrl);
    }

    private static void ApplyMediaCommands(MediaCommands mediaCommands, FluidValue width, FluidValue height, FluidValue mode, FluidValue quality, FluidValue format, FluidValue anchorValue, FluidValue bgcolor)
    {
        if (!width.IsNil())
        {
            mediaCommands.Width = width.ToStringValue();
        }

        if (!height.IsNil())
        {
            mediaCommands.Height = height.ToStringValue();
        }

        if (!mode.IsNil())
        {
            mediaCommands.ResizeMode = mode.ToStringValue();
        }

        if (!format.IsNil())
        {
            mediaCommands.Format = format.ToStringValue();
        }

        if (!quality.IsNil())
        {
            mediaCommands.Quality = quality.ToStringValue();
        }

        if (!anchorValue.IsNil())
        {
            var obj = anchorValue.ToObjectValue();

            if (obj is not Anchor anchor)
            {
                anchor = null;

                if (obj is JsonObject jObject)
                {
                    anchor = jObject.ToObject<Anchor>();
                }
            }
            if (anchor != null)
            {
                mediaCommands.ResizeFocalPoint = anchor.X.ToString(CultureInfo.InvariantCulture) + ',' + anchor.Y.ToString(CultureInfo.InvariantCulture);
            }
        }

        if (!bgcolor.IsNil())
        {
            mediaCommands.BackgroundColor = bgcolor.ToStringValue();
        }
    }
}
