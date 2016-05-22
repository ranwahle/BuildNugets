namespace BuildNugets
{
    internal class NugetSettings
    {
        public string Configuration { get; internal set; }
        public string Exclude { get; internal set; }
        public string NugetPath { get; internal set; }
        public string Version { get; internal set; }
    }
}