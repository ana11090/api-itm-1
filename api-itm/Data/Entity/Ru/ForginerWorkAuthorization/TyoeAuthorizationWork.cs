using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.ForginerWorkAuthorization
{
    [Table("tipautorizatiemunca")]
    public class TyoeAuthorizationWork
    {
        [Column("idtipautorizatiemunca")]
        string WorkPermitId { get; set; }
        [Column("denumiretipautorizatiemunca")]
        string WorkPermitName { get; set; }
        [Column("codtipautorizatiemunca")]
        string WorkPermitCode { get; set; }
    }
}
