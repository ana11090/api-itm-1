using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("intervaltimpmunca")]
    public class WorkingTimeInterval
    {
        // Columns: idintervaltimpmunca | denumireintervaltimpmunca | codintervaltimpmunca

        [Key]
        [Column("idintervaltimpmunca")]
        public int WorkingTimeIntervalId { get; set; }

        [Column("denumireintervaltimpmunca")]
        public string? WorkingTimeIntervalName { get; set; }

        [Column("codintervaltimpmunca")]
        public string? WorkingTimeIntervalCode { get; set; }
    }
}
