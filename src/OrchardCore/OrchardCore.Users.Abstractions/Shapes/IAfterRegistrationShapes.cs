using System.Collections.Generic;

namespace OrchardCore.Users.Shapes
{
    public interface IAfterRegistrationShapes
    {
        IEnumerable<dynamic> GetShapes();
    }
}