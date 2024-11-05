using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.Models
{
    public class ParamComparison
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string FromParm { get; set; }
        public string ToParm { get; set; }
    }
}
