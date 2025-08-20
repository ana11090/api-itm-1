using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Disability
{
    public class DisabilityType
    {
        public int DisabilityTypeId { get; set; }   // idtiphandicap
        public string? DisabilityTypeName { get; set; }   // denumiretiphandicap
        public string? DisabilityTypeCode { get; set; }   // codtiphandicap
    }
}