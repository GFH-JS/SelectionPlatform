using Microsoft.EntityFrameworkCore;
using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Respository
{
    public abstract class BaseRepository <T>: IBaseRepository<T> where T : class, new()
    {
        public MysqlDbContext _dbContext;
        public BaseRepository(MysqlDbContext dbContext)
        {
             _dbContext = dbContext;
        


        }
        public int Delete(T entity, bool saveNow = true)
        {
            //_dbContext.Set<T>().Update(entity);
            _dbContext.Set<T>().Remove(entity);
            if (saveNow) {
                return _dbContext.SaveChanges();
            }
            return -1;

           
        }

        public IQueryable<T> FindAll()
        {
            return _dbContext.Set<T>();
        }

        public IQueryable<T> FindByExpress(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            return _dbContext.Set<T>().Where(expression);
        }

        public int Insert(T entity, bool saveNow = true)
        {
            _dbContext.Set<T>().Add(entity);
            if (saveNow) {
                return _dbContext.SaveChanges();
            }
            return -1;
        }

        public T QueryById(object pkValue)
        {
            return _dbContext.Set<T>().Find(pkValue);     //只能查询pk
        }

        public Task<int> SaveChangesAsync()
        {
            return _dbContext.SaveChangesAsync();
        }

        public int Update(T entity, bool saveNow = true)
        {
            _dbContext.Set<T>().Update(entity);
            if (saveNow) {
                return _dbContext.SaveChanges();
            }
            return -1;
           
        }
    }
}
