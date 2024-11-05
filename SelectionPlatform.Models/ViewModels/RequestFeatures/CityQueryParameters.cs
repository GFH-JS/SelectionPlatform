using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.RequestFeatures
{
    public class CityQueryParameters
    {
        public int? country { get; set; }
        public int? province { get; set; }
        public int? city { get; set; }
     
    }
}
