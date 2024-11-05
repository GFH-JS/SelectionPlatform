using Microsoft.EntityFrameworkCore;
using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository;
using SelectionPlatform.Repository;

namespace SelectionPlatformWeb.Extentions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(option => {
                option.AddPolicy("corsPolicy", opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
        }


        //public static void AddRepositoryWrapperSetup(this IServiceCollection services)
        //{
        //    services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        //}
    }
}
