using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru
{
    [Table("judete", Schema = "ru")]
    public class County
    {
        //idjudet	numejudet	indicativjudet	numejudet_fd
        [Key]
        [Column("idjudet")]
        public int CountyId { get; set; }
        [Column("numejudet")]
        public string CountyName { get; set; }
        [Column("indicativjudet")]
        public string CountyCode { get; set; }
        [Column("numejudet_fd")]
        public string CountyNameFd { get; set; }
    }
}
