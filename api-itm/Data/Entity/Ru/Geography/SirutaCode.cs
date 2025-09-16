using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Geography
{
    /// <summary>
    /// SIRUTA codes table  
    /// </summary>
    [Table("codurisiruta")]
    public sealed class SirutaCode
    {
        /// <summary>
        /// SIRUTA code (unique code for the locality).
        /// </summary>
        [Key]
        [Column("codsiruta")]
        public string SirutaCodeValue { get; set; } = default!;

        /// <summary>
        /// Locality name.
        /// </summary>
        [Column("denumirelocalitate")]
        public string? LocalityName { get; set; }

        /// <summary>
        /// Environment / setting (e.g., Urban/Rural).
        /// </summary>
        [Column("mediu_codurisiruta")]
        public string? Environment { get; set; }

        /// <summary>
        /// County code.
        /// </summary>
        [Column("codjudet")]
        public string? CountyCode { get; set; }

        /// <summary>
        /// Parent locality (the locality it belongs to).
        /// </summary>
        [Column("localitate_apartenenta")]
        public string? ParentLocality { get; set; }
    }
}