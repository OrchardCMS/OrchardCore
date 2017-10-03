using System;

namespace OrchardCore.Environment.Extensions.Features.Attributes
{
    /// <summary>
    /// Use this attribute to automatically register the class with the service container.
    /// To be used in conjunction with <seealso cref="ServiceAttribute"/>, where the <seealso cref="ServiceAttribute"/> is applied to the type to register this implementation as.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ServiceImplAttribute : Attribute
    {
    }
}