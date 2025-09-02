using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Study
{
    public class EducationLevel
    {
        public int EducationLevelId { get; set; }       // idnivelstudii
        public string? EducationLevelName { get; set; } // denumirenivelstudii
        public string? EducationLevelCode { get; set; } // codnivelstudii
    }
}