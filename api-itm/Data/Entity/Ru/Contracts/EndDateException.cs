using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("exceptiedatasfarsit")]
    public class EndDateException
    {
        [Key]
        [Column("idexceptiedatasfarsit")]
        public int EndDateExceptionId { get; set; }

        [Column("denumireexceptiedatasfarsit")]
        public string? EndDateExceptionName { get; set; }

        [Column("codexceptiedatasfarsit")]
        public string? EndDateExceptionCode { get; set; }
    }
}