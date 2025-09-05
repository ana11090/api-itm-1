using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.Work
{
    [Table("tipnorma", Schema = "ru")]
    public class WorkNormType
    {
        //idtipnorma denumiretipnorma    codtipnorma
        [Key]
        [Column("idtipnorma")]
        public int WorkNormTypeId { get; set; }
        [Column("denumiretipnorma")]
        public string? WorkNormTypeName { get; set; }
        [Column("codtipnorma")]
        public string? WorkNormTypeCode { get; set; }
    }
}
