using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity
{
    [Table("tipnationalitate")]
    public class NationalityType
    {
        [Key]
        [Column("idtipnationalitate")]
        public int NationalityTypeId { get; set; }

        [Column("denumiretipnationalitate")]
        public string NationalityTypeName { get; set; }

        [Column("codtipnationalitate")]
        public string NationalityTypeCode { get; set; }
    }
}