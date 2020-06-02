using System.Threading.Tasks;

namespace OrchardCore.ShortCodes
{
    public interface IShortCode
    {
        string Name { get; }

        void Process(ShortCodeContext context, ShortCodeOutput output);

        Task ProcessAsync(ShortCodeContext context, ShortCodeOutput output);
    }
}
