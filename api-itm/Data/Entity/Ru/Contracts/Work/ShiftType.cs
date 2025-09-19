using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.Work
{
    [Table("tiptura", Schema = "ru")]
    public class ShiftType
    {
        //idtiptura	denumiretiptura	codtiptura
        [Key]
        [Column("idtiptura")]
        public int ShiftTypeId { get; set; }
        [Column("denumiretiptura")]
        public string ShiftTypeName { get; set; }
        [Column("codtiptura")]
        public string ShiftTypeCode { get; set; }

        
    }
}
