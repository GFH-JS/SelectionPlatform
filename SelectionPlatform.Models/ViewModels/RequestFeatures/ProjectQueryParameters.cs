using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.RequestFeatures
{
    public class ProjectQueryParameters:QueryStringParameters
    {
        public ProjectQueryParameters()
        {
            OrderBy = "CREATE_DATE desc";
        }

        public DateTime MinCreateTime { get; set; }
        public DateTime MaxCreateTime { get; set; } = DateTime.Now;
        public bool ValidDataRange => MaxCreateTime > MinCreateTime;
        public string? projectId { get; set; }
        public string? Id { get; set; }
        public bool ShowLatest { get; set; } = true;

    }
}
