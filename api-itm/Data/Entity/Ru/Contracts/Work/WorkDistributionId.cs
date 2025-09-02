using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.Work
{
    [Table("repartizaremunca")]
    //idrepartizaremunca	denumirerepartizaremunca	codrepartizaremunca
    // ex. use "repartizare":"OreDeZi",
    public class WorkDistributionId
    {
        [Key]
        [Column("idrepartizaremunca")]
        public int WorkDistributionIdId { get; set; }
        [Column("denumirerepartizaremunca")]
        public string? WorkDistributionIdName { get; set; }
        [Column("codrepartizaremunca")]
        public string? WorkDistributionIdCode { get; set; }
    }
}
