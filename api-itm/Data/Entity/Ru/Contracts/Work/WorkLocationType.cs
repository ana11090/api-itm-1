using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.Work
{
    [Table("tiplocmunca")]
    public class WorkLocationType
    {
        [Key]
        [Column("idtiplocmunca")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // remove if not identity
        public int WorkLocationTypeId { get; set; }

        [Column("denumiretiplocmunca")] 
        public string WorkLocationTypeName { get; set; }

        [Column("codtiplocmunca")] 
        public string WorkLocationTypeCode { get; set; }
    }
}