using MongoDB.Bson;
using MongoDB.Driver;
using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Repository
{
    public class MongoBaseRepository<T> : IMongoBaseRepository<T> where T : class, new()
    {
        private readonly IMongoCollection<T> _mongoCollection;
        public MongoBaseRepository(IMongoDatabase mongoDatabase)
        {
            _mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);  //具体子类 T
        }
        public Task Delete(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
           return _mongoCollection.DeleteOneAsync(filter);
        }

        public Task<List<T>> FindAll()
        {
           return  _mongoCollection.Find(_ => true).ToListAsync();
        }

        public Task<List<T>> FindByExpress(Expression<Func<T, bool>> expression)
        {
            return _mongoCollection.Find(expression).ToListAsync();
        }

        public Task<List<T>> FindByBsonDocument(BsonDocument[] pipeline)
        {
            return _mongoCollection.Aggregate<T>(pipeline).ToListAsync();
        }

        public Task Insert(T entity)
        {
            return _mongoCollection.InsertOneAsync(entity);
        }

        public Task<T> QueryById(string id)
        {
            
            var filter = Builders<T>.Filter.Eq("_id",new ObjectId(id));
            return _mongoCollection.Find(filter).FirstOrDefaultAsync();
            //return await _mongoCollection.Find(x => x.Id.ToString() == id).FirstOrDefaultAsync();
        }

        public Task<int> SaveChangesAsync()
        {
            
            throw new NotImplementedException();
        }

        public Task Update(string id, T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
            return _mongoCollection.ReplaceOneAsync(filter, entity);
        }

        public  Task Update(FilterDefinition<T> filterDefinition,UpdateDefinition<T> updateDefinition)
        {
            return _mongoCollection.UpdateManyAsync(filterDefinition, updateDefinition);
        }

        public  void test()
        {
            var match1 = new BsonDocument
        {
            {
                "$match", new BsonDocument{
                    {
                        "projectId", ""
                    }
                }
            }
        };

            var group = new BsonDocument
        {
            {
                "$group", new BsonDocument{
                   {
                       "_id", "$name"
                   },
                   {
                       "count", new BsonDocument{
                           {"$sum", 1}
                       }
                   }
               }
            }
        };

            var match2 = new BsonDocument
        {
            {
                "$match", new BsonDocument{
                    {
                        "count", 1
                    }
                }
            }
        };


            BsonDocument db = new BsonDocument { { "_id", "$projectId" }, { "count", new BsonDocument("$sum", 1) } };

            var pipline = new BsonDocument[] {
            new BsonDocument("$sort",new BsonDocument{

               { "projectId",1},
                { "CreateTime",-1}
            }),
            new BsonDocument("$group",new BsonDocument
            {
               { "_id","$projectId"},
               { "latestRecord",new BsonDocument("$first","$$ROOT")},
               
            }),
            new BsonDocument("$replaceRoot",new BsonDocument("newRoot","$latestRecord"))
            };


            //var VV = _mongoCollection.Aggregate<ProjectInfoEntity>(pipline).ToList();
        }

        public Task<List<T>> FindByFilter(FilterDefinition<T> filterDefinition)
        {
            return _mongoCollection.Find(filterDefinition).ToListAsync();
        }

        public  Task InsertMany(IEnumerable<T> documents)
        {
             return _mongoCollection.InsertManyAsync(documents);
        }

        public Task Delete(FilterDefinition<T> filterDefinition)
        {
            return _mongoCollection.DeleteOneAsync(filterDefinition);
        }
    }
}
