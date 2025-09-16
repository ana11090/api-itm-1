using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_itm.Data.Entity.Ru
{
    [Table("idsreges_salariat_modificari", Schema = "ru")]
    public class RegesSyncModificationEmployee
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("idpersoana")]
        public int? PersonId { get; set; }

        [Column("idutilizator")]
        public int? UserId { get; set; }

        [Column("id_raspuns_mesaj")]
        public Guid? MessageResponseId { get; set; }

        [Column("id_rezultat_mesaj")]
        public Guid? MessageResultId { get; set; }

        [Column("idautor")]
        public Guid? AuthorId { get; set; }

        [Column("reges_salariat_id")]
        public Guid? RegesEmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
