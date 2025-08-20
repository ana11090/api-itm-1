using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Disability
{
    [Table("caracterhandicap", Schema = "ru")]
    public class DisabilityCharacter
    {
        [Key]
        [Column("idcaracterhandicap")]
        public int DisabilityCharacterId { get; set; }

        [Column("denumirecaracterhandicap")]
        public string? DisabilityCharacterName { get; set; }
    }
}
