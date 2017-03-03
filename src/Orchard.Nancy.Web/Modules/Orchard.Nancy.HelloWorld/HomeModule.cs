using Nancy;

namespace Orchard.Nancy.HelloWorld
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get("/{category}", parameters => "My category is " + parameters.category);

            Get("/sayhello", _ => "Hello from Nancy");
        }
    }
}
