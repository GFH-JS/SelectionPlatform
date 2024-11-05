using Microsoft.EntityFrameworkCore;
using SelectionPlatform.EntityFramework.Mappings;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.EntityFramework
{
    public class MysqlDbContext:DbContext
    {
        public MysqlDbContext(DbContextOptions<MysqlDbContext> options):base(options)
        {
                
        }

        //public DbSet<UserEntity> Users { get; set; }

        //public DbSet<ProjectbaseinfoEntity> Projects { get; set; }

        //public DbSet<CityEntity> City { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //通过这句话就会加载所有的配置类 实现 IEntityTypeConfiguration
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());  //映射程序集所有的

            //modelBuilder.ApplyConfiguration(new UserMap());

            //modelBuilder.Entity<UserEntity>().HasData(); //填充默认数据
        }
    }
}
