using AutoMapper;

namespace SampleAPI.Mappings
{
    public class ApiMappings : Profile
    {
        public ApiMappings()
        {
            this.ToEntities();
            this.FromEntities();
        }

        private void FromEntities()
        {
        }

        private void ToEntities()
        {
        }
    }
}