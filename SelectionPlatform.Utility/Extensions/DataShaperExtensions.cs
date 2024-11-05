using SelectionPlatform.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Utility.Extensions
{
    public static class DataShaperExtensions
    {
        public static IEnumerable<dynamic> ShapeData<T>(this IEnumerable<T> sources ,string fields)
        {
            var dataShaper = new DataShaper<T>(fields);
            return dataShaper.FetchData(sources);
        }

        public static dynamic ShapeData<T>(this T sources, string fields)
        {
            var dataShaper = new DataShaper<T>(fields);
            return dataShaper.FetchData(sources);
        }
    }
}
