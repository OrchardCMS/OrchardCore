using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.Indexing
{
    public class CoordinateFieldIndexHandler : ContentFieldIndexHandler<CoordinateField>
    {
        public override Task BuildIndexAsync(CoordinateField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, new DocumentIndex.Point
                {
                    X = field.Longitude,
                    Y = field.Latitude
                }, options);
            }

            return Task.CompletedTask;
        }
    }
}