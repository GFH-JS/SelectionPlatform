using SelectionPlatform.IRepository;
using SelectionPlatform.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Services
{
    public class BaseServices<T>:IBaseServices<T> where T : class,new()
    {
        public IBaseRepository<T> baseDal;   //由具体实现的子类传递对象
       
        public int Delete(T entity, bool saveNow = true)
        {
            return baseDal.Delete(entity,saveNow);
          
        }

        public IQueryable<T> FindAll()
        {
            return baseDal.FindAll();
        }

        public IQueryable<T> FindByExpress(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            return baseDal.FindByExpress(expression);
        }

        public int Insert(T entity, bool saveNow = true)
        {
            return baseDal.Insert(entity,saveNow);
        }

        public T QueryById(object pkValue)
        {
            return baseDal.QueryById(pkValue);
        }

        public Task<int> SaveChangesAsync()
        {
            return baseDal.SaveChangesAsync();
        }

        public int Update(T entity , bool saveNow = true)
        {
            return baseDal.Update(entity,saveNow);
           
        }
    }
}
