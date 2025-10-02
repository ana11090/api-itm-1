using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Geography
{
    [Table("localitati_orase", Schema = "ru")]
    public class CountyCity
    {
        [Key]
        [Column("idlocalitate")]
        public int CountyCityId { get; set; }                 // idlocalitate

        [Column("localitate_apartenenta")]
        [StringLength(200)]
        public string? ParentLocality { get; set; }            // localitate_apartenenta

        [Column("denumirelocalitate")]
        [Required, StringLength(200)]
        public string LocalityName { get; set; } = string.Empty; // denumirelocalitate

        [Column("idjudet")]
        public int CountyId { get; set; }                      // idjudet

        [Column("codjudet")]
        [StringLength(10)]
        public string CountyCode { get; set; } = string.Empty; // codjudet

        [Column("mediu")]
        [StringLength(10)]
        public string? EnvironmentType { get; set; }           // mediu (Urban/Rural etc.)

        [Column("localitate_apartenenta_fd")]
        [StringLength(200)]
        public string? ParentLocalityNoDiacritics { get; set; } // localitate_apartenenta_fd

        [Column("denumirelocalitate_fd")]
        [StringLength(200)]
        public string? LocalityNameNoDiacritics { get; set; }  // denumirelocalitate_fd

        [Column("id_rmu")]
        public int? RmuId { get; set; }                        // id_rmu

        [Column("codsiruta")]
        public int SirutaCode { get; set; }                    // codsiruta

        [Column("codsiruta_sup")]
        public int? SirutaCodeSuperior { get; set; }           // codsiruta_sup
    }
}