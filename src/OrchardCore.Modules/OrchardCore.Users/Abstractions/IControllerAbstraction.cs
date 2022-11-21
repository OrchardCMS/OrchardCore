using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Users.Abstractions;
public interface IControllerAbstraction<T> where T : Controller
{
    T BindController { get; set; }
}
