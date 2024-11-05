using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.EntityFramework
{
    public class MongoDbContext:DbContext
    {
        public MongoDbContext(DbContextOptions<MongoDbContext> options):base(options)
        {
            
        }
        public DbSet<UserEntity> Users { get; set; }

    }
}
