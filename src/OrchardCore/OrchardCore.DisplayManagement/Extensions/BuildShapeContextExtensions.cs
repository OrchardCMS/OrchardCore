namespace OrchardCore.DisplayManagement.Handlers;

public static class BuildShapeContextExtensions
{
    public static void AddTenantReloadWarningWrapper(this BuildShapeContext context)
        => context.Shape.Metadata.Wrappers.Add("Settings_Wrapper__Reload");
}
