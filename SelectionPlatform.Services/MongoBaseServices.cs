using MongoDB.Bson;
using MongoDB.Driver;
using SelectionPlatform.IRepository;
using SelectionPlatform.IServices;
using SelectionPlatform.Models.Models;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Services
{
    public class MongoBaseServices<T> : IMongoBaseServices<T> where T : class, new()
    {
        public MongoBaseServices(IMongoBaseRepository<T> mongoBaseRepository)   ///serveics 层调用repsitory
        {
            baseDal = mongoBaseRepository;
        }
        private readonly IMongoBaseRepository<T> baseDal;   //由具体实现的子类传递对象

        public async Task Delete(string id)
        {
            await baseDal.Delete(id);
        }

        public async Task<List<T>> FindAll()
        {
            return await baseDal.FindAll();
        }

        public Task<List<T>> FindByExpress(Expression<Func<T, bool>> expression)
        {
            return baseDal.FindByExpress(expression);
        }

        public async Task Insert(T entity)
        {
            await baseDal.Insert(entity);
        }

        public async Task<T> QueryById(string id)
        {
            return await baseDal.QueryById(id);
        }

        public async Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task Update(string id, T entity)
        {
            await baseDal.Update(id, entity);
        }

        public void test()
        {
            baseDal.test();
        }

        public Task<List<T>> FindByFilter(FilterDefinition<T> filterDefinition)
        {
            return baseDal.FindByFilter(filterDefinition);
        }

        public Task InsertMany(IEnumerable<T> documents)
        {
            return baseDal.InsertMany(documents);
        }

        public Task Update(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition)
        {
            return baseDal.Update(filterDefinition, updateDefinition);
        }

        public Task Delete(FilterDefinition<T> filterDefinition)
        {
            return baseDal.Delete(filterDefinition);
        }

        public Task<List<T>> FindByBsonDocument(BsonDocument[] pipeline)
        {
            return baseDal.FindByBsonDocument(pipeline);
        }
    }
}
