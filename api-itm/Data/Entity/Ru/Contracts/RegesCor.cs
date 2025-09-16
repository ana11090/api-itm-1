using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("reges_cor", Schema = "ru")]
    public class RegesCor
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("nume")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("versiune")]
        public int Version { get; set; }

        [Required]
        [Column("cod")]
        public int Code { get; set; }

        [Required]
        [Column("cor_id")]
        public Guid CorId { get; set; }

        [Required]
        [Column("versiune_curenta")]
        public bool IsCurrentVersion { get; set; }

        [Required]
        [Column("ocupatie")]
        public bool IsOccupation { get; set; }
    }
}