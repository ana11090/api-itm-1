using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("norma")]
    public class WorkSchedule
    {
        //idnorma	denumirenorma	codnorma	tipnorma
        [Key]
        [Column("idnorma")]
        public int WorkScheduleId { get; set; }
        [Column("denumirenorma")]
        public string? WorkScheduleName { get; set; }
        [Column("codnorma")]
        public string? WorkScheduleCode { get; set; }
        [Column("tipnorma")]
        public string? WorkScheduleType { get; set; }
    }
}
