using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.Work
{
    [Table("repartizaretimpmunca", Schema = "ru")]
    public class WorkTimeAllocation
    {
        //idrepartizaretimpmunca	denumirerepartizaretimpmunca	codrepartizaretimpmunca
        [Key]
        [Column("idrepartizaretimpmunca")]
        public int WorkTimeAllocationId { get; set; }
        [Column("denumirerepartizaretimpmunca")]
        public string? WorkTimeAllocationName { get; set; }
        [Column("codrepartizaretimpmunca")]
        public string? WorkTimeAllocationCode { get; set; }

    }
}
