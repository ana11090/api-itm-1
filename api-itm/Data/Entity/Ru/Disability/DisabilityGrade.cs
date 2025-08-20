using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Disability
{
    public class DisabilityGrade
    {
        public int DisabilityGradeId { get; set; }     // idgradhandicap
        public string? DisabilityGradeName { get; set; } // denumiregradhandicap
        public string? DisabilityGradeCode { get; set; } // codgradhandicap
    }
}
