namespace OrchardVNext.Environment {
    /// <summary>
    /// Abstraction of the running environment
    /// </summary>
    public interface IHostEnvironment {
        bool IsFullTrust { get; }
        string MapPath(string virtualPath);

        bool IsAssemblyLoaded(string name);

        void RestartAppDomain();
    }
}