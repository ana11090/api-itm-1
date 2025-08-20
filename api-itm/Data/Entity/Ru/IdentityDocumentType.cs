using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru
{
    [Table("tip_act")]
    public class IdentityDocumentType
    {
        [Key]
        [Column("idtipact")]
        public int IdentityDocumentTypeId { get; set; }

        [Column("denumiretipact")]
        public string IdentityDocumentName { get; set; }

        [Column("codtipact")]
        public string IdentityDocumentCode { get; set; }
    }
}