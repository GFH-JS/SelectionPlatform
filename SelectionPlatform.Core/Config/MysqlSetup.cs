using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SelectionPlatform.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Core.Config
{
    public static class MysqlSetup
    {
        public static void AddMysqlSetup(this IServiceCollection services, IConfiguration configuration)
        {
            var connctionstr = configuration.GetConnectionString("MysqlDB");
            services.AddDbContext<MysqlDbContext>(
                builder => builder.UseMySql(connctionstr, MySqlServerVersion.LatestSupportedServerVersion)  //MySqlServerVersion.LatestSupportedServerVersion
                );
        }
    }
}
