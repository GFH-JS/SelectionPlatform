using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Utility.Helper
{
    public class DataShaper<T>
    {
        private readonly List<PropertyInfo> _requiredProperties = new List<PropertyInfo>();
        public DataShaper(string fieldString)
        {
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (string.IsNullOrWhiteSpace(fieldString))
            {
                _requiredProperties = propertyInfos.ToList();
            }
            else
            {
                var fields = fieldString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var field in fields)
                {
                    var property = propertyInfos.FirstOrDefault(p => p.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (property == null) continue;

                    _requiredProperties.Add(property);
                }
            }

        }


        public ExpandoObject FetchData(T source)
        {
            var shapedObject = new ExpandoObject();
            foreach (var property in _requiredProperties)
            {
                var objectPrppertyValue = property.GetValue(source);
                shapedObject.TryAdd(property.Name, objectPrppertyValue);
            }

            return shapedObject;
        }

        public IEnumerable<ExpandoObject> FetchData(IEnumerable<T> sources)
        {
            var shapedData = new List<ExpandoObject>();
            foreach (var item in sources)
            {
                shapedData.Add(FetchData(item));
            }
            return shapedData;
        }
    }
}
