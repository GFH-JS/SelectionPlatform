using MongoDB.Bson;
using MongoDB.Driver;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IRepository
{
    public interface IMongoBaseRepository<T> where T :class , new()
    {
        Task<List<T>> FindAll();
        Task<List<T>> FindByExpress(Expression<Func<T, bool>> expression);
        Task<List<T>> FindByFilter(FilterDefinition<T> filterDefinition);
        Task<List<T>> FindByBsonDocument(BsonDocument[] pipeline);
        Task<T> QueryById(string id);
        Task Insert(T entity);
        Task InsertMany(IEnumerable<T> documents);
        Task Update(string id, T entity);
        Task Update(FilterDefinition<T> filterDefinition, UpdateDefinition<T> updateDefinition);
        Task Delete(string id);
        Task Delete(FilterDefinition<T> filterDefinition);
        Task<int> SaveChangesAsync();
        void test();
    }
}
