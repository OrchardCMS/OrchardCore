using Nancy;

namespace OrchardCore.Nancy.HelloWorld
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get("/{category}", parameters => "My category is " + parameters.category);

            Get("/", _ => "Hello from Nancy");
        }
    }
}
