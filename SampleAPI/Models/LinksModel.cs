namespace SampleAPI.Models
{
    public class LinksModel
    {
        public LinksModel(string href, string rel, string method)
        {
            this.Href = href;
            this.Rel = rel;
            this.Method = method;
        }

        public string Href { get; }

        public string Method { get; }

        public string Rel { get; }
    }
}