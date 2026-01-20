namespace OrchardCore.Infrastructure.Entities;

public class ListEntitiesWithOptionsViewModel<TOptions>
{
    public TOptions Options { get; set; }

    public dynamic Pager { get; set; }
}
