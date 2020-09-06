using System.Collections.Generic;

namespace SampleAPI.Models.View
{
    public abstract class ViewModelBase
    {
        public List<LinksModel> Links { get; } = new List<LinksModel>();
    }
}