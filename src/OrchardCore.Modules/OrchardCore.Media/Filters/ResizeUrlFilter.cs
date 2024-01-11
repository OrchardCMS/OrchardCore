using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Liquid;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Filters
{
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

            IDictionary<string, string> queryStringParams;

            if (!profile.IsNil())
            {
                queryStringParams = await _mediaProfileService.GetMediaProfileCommands(profile.ToStringValue());

                // Additional commands to a profile must be named.
                var width = arguments["width"];
                var height = arguments["height"];
                var mode = arguments["mode"];
                var quality = arguments["quality"];
                var format = arguments["format"];
                var anchor = arguments["anchor"];
                var bgcolor = arguments["bgcolor"];

                ApplyQueryStringParams(queryStringParams, width, height, mode, quality, format, anchor, bgcolor);
            }
            else
            {
                queryStringParams = new Dictionary<string, string>();

                // Never mix named and indexed arguments as this leads to unpredictable results.
                var useNamed = arguments.Names.Any();

                var width = useNamed ? arguments["width"] : arguments.At(0);
                var height = useNamed ? arguments["height"] : arguments.At(1);
                var mode = useNamed ? arguments["mode"] : arguments.At(2);
                var quality = useNamed ? arguments["quality"] : arguments.At(3);
                var format = useNamed ? arguments["format"] : arguments.At(4);
                var anchor = useNamed ? arguments["anchor"] : arguments.At(5);
                var bgcolor = useNamed ? arguments["bgcolor"] : arguments.At(6);

                ApplyQueryStringParams(queryStringParams, width, height, mode, quality, format, anchor, bgcolor);
            }

            var resizedUrl = QueryHelpers.AddQueryString(url, queryStringParams);


            if (_options.UseTokenizedQueryString)
            {
                resizedUrl = _mediaTokenService.AddTokenToPath(resizedUrl);
            }

            return new StringValue(resizedUrl);
        }

        private static void ApplyQueryStringParams(IDictionary<string, string> queryStringParams, FluidValue width, FluidValue height, FluidValue mode, FluidValue quality, FluidValue format, FluidValue anchorValue, FluidValue bgcolor)
        {
            if (!width.IsNil())
            {
                queryStringParams["width"] = width.ToStringValue();
            }

            if (!height.IsNil())
            {
                queryStringParams["height"] = height.ToStringValue();
            }

            if (!mode.IsNil())
            {
                queryStringParams["rmode"] = mode.ToStringValue();
            }

            if (!quality.IsNil())
            {
                queryStringParams["quality"] = quality.ToStringValue();
            }

            if (!format.IsNil())
            {
                queryStringParams["format"] = format.ToStringValue();
            }

            if (!anchorValue.IsNil())
            {
                var obj = anchorValue.ToObjectValue();

                if (obj is not Anchor anchor)
                {
                    anchor = null;

                    if (obj is JObject jObject)
                    {
                        anchor = jObject.ToObject<Anchor>();
                    }
                }
                if (anchor != null)
                {
                    queryStringParams["rxy"] = anchor.X.ToString(CultureInfo.InvariantCulture) + ',' + anchor.Y.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (!bgcolor.IsNil())
            {
                queryStringParams["bgcolor"] = bgcolor.ToStringValue();
            }
        }
    }
}
