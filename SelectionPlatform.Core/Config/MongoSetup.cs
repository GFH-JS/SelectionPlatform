using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SelectionPlatform.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Core.Config
{
    public static class MongoSetup
    {
        
        public static void AddMongoSetup(this IServiceCollection services, MongoDbSettings mongoDbSettings)
        {
            var mongoClient = new MongoClient(
             mongoDbSettings.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                mongoDbSettings.DatabaseName);

            services.AddSingleton(mongoDatabase);
        }
    }

    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string BooksCollectionName { get; set; } = null!;
    }
}
