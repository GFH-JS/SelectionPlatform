using SelectionPlatform.IRepository;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.IServices
{
    public interface IMongoBaseServices<T> : IMongoBaseRepository<T> where T : class, new() 
    {
    }
}
