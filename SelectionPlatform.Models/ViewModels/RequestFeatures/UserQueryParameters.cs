using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.RequestFeatures
{
    public class UserQueryParameters:QueryStringParameters
    {
        public UserQueryParameters()
        {
            OrderBy = "CreateTime";
        }
        public DateTime MinCreateTime { get; set; }
        public DateTime MaxCreateTime { get; set; } = DateTime.Now;
        public bool ValidDataRange => MaxCreateTime > MinCreateTime;
        public string? Account { get; set; }
    }
}
