using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Geography
{

    [Table("localitati", Schema = "ru")]
    public class City
    {
        [Key]
        public Guid Id { get; set; }

        // db: ru.localitati.codsiruta
        [Column("codsiruta")]
        public int SirutaCode { get; set; }

        // db: ru.localitati.nume
        [Column("nume")]
        [Required, StringLength(200)]
        public string CityName { get; set; } = string.Empty;

        // db: ru.localitati.judetid
        [Column("judetid")]
        [Display(Name = "County Id")]
        public int CountyId { get; set; }
    }
}