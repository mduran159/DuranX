namespace OpeniddictServer.Configuration
{
    public class ScopesConfiguration
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<string> Audience { get; set; }
    }
}
