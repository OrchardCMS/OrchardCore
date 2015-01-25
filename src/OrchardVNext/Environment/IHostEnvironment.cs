namespace OrchardVNext.Environment {
    /// <summary>
    /// Abstraction of the running environment
    /// </summary>
    public interface IHostEnvironment : ISingletonDependency {
        string MapPath(string virtualPath);
    }
}