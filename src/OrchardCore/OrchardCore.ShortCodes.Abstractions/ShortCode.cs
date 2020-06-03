using System.Threading.Tasks;

namespace OrchardCore.ShortCodes
{
    public abstract class ShortCode : IShortCode
    {
        public virtual void Process(ShortCodeContext context, ShortCodeOutput output)
        {

        }

        public virtual Task ProcessAsync(ShortCodeContext context, ShortCodeOutput output)
        {
            Process(context, output);

            return Task.CompletedTask;
        }
    }
}
