using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DisplayManagement
{
    /// <summary>
    /// Used to create a dynamic, contextualized Display object to dispatch shape rendering
    /// </summary>
    public interface IDisplayHelperFactory
    {
        dynamic CreateHelper(ViewContext viewContext);
    }
}