using Microsoft.Extensions.DependencyInjection;
using SimpleMapper.Abstracts;

namespace SimpleMapper.Extensions
{
    public static class RegistrationExtension
    {
        public static void AddSimpleMapper(this IServiceCollection services)
        {
            services.AddSingleton<IMapper, Mapper>();
            
            Mapper.Init();
        }
    }
}