using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.DisplayManagement
{
    /// <summary>
    /// Used to create a dynamic, contextualized Display object to dispatch shape rendering
    /// </summary>
    public interface IDisplayHelperFactory
    {
        IDisplayHelper CreateHelper(ViewContext viewContext);
    }
}