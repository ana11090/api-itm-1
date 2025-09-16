using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Disability
{
    [Table("gradinvaliditate")]
    public class DisabilityInvalidityGrade
    {
        [Key]
        [Column("idgradinvaliditate")]
        public int InvalidityGradeId { get; set; }

        [Column("denumiregradinvaliditate")]
        public string? InvalidityGradeName { get; set; }

        [Column("codgradinvaliditate")]
        public string? InvalidityGradeCode { get; set; }
    }
}