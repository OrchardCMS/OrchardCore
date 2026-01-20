namespace OrchardCore.DisplayManagement.TagHelpers;

public interface ITagHelpersProvider
{
    IEnumerable<Type> GetTypes();
}
