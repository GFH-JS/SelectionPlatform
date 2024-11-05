using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Core.Config
{
    /// <summary>
    /// 配置跨域（CORS）
    /// </summary>
    public static class CorsSetup
    {
        public static void AddCorsSetup(this IServiceCollection services)
        {
            //services.AddCors(option =>
            //{
            //    option.AddPolicy("corsPolicy", opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            //});
        }
    }
}
