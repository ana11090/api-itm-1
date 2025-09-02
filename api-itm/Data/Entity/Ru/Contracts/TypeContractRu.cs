using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("tipcontract_ru", Schema = "ru")]
    public class TypeContractRu
    {
        //idtipcontract	denumiretipcontract	codtipcontract
        [Column("idtipcontract")]
        public int TypeContractRuId { get; set; }
        [Column("denumiretipcontract")]
        public string? TypeContractRuName { get; set; }
        [Column("codtipcontract")]
        public string? TypeContractRuCode { get; set; }
    }
}
