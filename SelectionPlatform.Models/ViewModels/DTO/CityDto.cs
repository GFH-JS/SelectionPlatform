using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.ViewModels.DTO
{
    public class CityResponseDto
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? city_code { get; set; }
        public string? level { get; set; }
        public string? parent_id { get; set; }
        public string? parent_ids { get; set; }
    }
}
