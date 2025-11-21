using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("functieprestari", Schema = "ru")]
    public class ServiceFunction
    {
        [Key]
        [Column("idfunctieprestari")]
        public int ServiceFunctionId { get; set; }

        [Column("denumirefunctieprestari")]
        public string? ServiceFunctionName { get; set; }
    }
}