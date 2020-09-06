namespace SampleAPI.Models.Common
{
    public class PagedResult<T>
    {
        public T[] Data { get; set; }

        public uint Page { get; set; }

        public uint PageSize { get; set; }
    }
}