using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("tipdurata")]
    public class ContractTypeDuration
    {//idtipdurata	denumiretipdurata	codtipdurata

        [Key]
        [Column("idtipdurata")]
        public int ContractTypeDurationId { get; set; }
        [Column("denumiretipdurata")]
        public string? ContractTypeDurationName { get; set; }
        [Column("codtipdurata")]
        public string? ContractTypeDurationCode { get; set; }
        }
}
