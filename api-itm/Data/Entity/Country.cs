using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity
{
    [Table("tari")]
    public class Country
    {
        [Key]
        [Column("idtara")]
        public int CountryId { get; set; }

        [Column("denumiretara")]
        public string CountryName { get; set; }

        [Column("tiptara")]
        public string CountryType { get; set; }

        [Column("cod_alpha_2")]
        public string Alpha2Code { get; set; }

        [Column("cod_alpha_3")]
        public string Alpha3Code { get; set; }

        [Column("cod_iso_international")]
        public string IsoInternationalCode { get; set; }

        [Column("cod_numeric")]
        public int? NumericCode { get; set; }

        [Column("denumiretara_revisal")]
        public string CountryNameRevisal { get; set; }
    }
}
