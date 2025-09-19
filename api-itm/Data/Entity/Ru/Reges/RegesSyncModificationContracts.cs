using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Reges
{
    [Table("idsreges_contracte_modificari", Schema = "ru")]
    public class RegesSyncModificationContracts
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("idcontract")]
        public int? IdContract { get; set; }

        [Column("idutilizator")]
        public int? IdUtilizator { get; set; }

        [Column("id_raspuns_mesaj")]
        public Guid? Id_Raspuns_Mesaj { get; set; }

        [Column("id_rezultat_mesaj")]
        public Guid? Id_Rezultat_Mesaj { get; set; }

        [Column("idautor")]
        public Guid? IdAutor { get; set; }

        [Column("reges_contract_id")]
        public Guid? RegesContractId { get; set; } // maps reges_contract_id

        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Column("error_message")]
        public string? Error_Message { get; set; }

        [Column("created_at")]
        public DateTime Created_At { get; set; }

        [Column("updated_at")]
        public DateTime Updated_At { get; set; }
    }
}
