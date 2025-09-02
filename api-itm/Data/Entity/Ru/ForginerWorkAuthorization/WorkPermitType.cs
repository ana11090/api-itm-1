using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.ForginerWorkAuthorization
{
    [Table("tipautorizatiemunca")]
    public class WorkPermitType
    {
        [Key]
        [Column("idtipautorizatiemunca")]
        public int WorkPermitId { get; set; }

        [Column("denumiretipautorizatiemunca")]
        public string WorkPermitName { get; set; }

        [Column("codtipautorizatiemunca")]
        public string WorkPermitCode { get; set; }
    }
}
