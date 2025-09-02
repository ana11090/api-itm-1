using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.ForginerWorkAuthorization
{
    [Table("tipautorizatieexceptie")]
    public class AuthorizationExceptionType
    {
        [Key]
        [Column("idtipautorizatieexceptie")]
        public int AuthorizationExceptionTypeId { get; set; }

        [Column("denumiretipautorizatieexceptie")]
        public string AuthorizationExceptionTypeName { get; set; }

        [Column("codtipautorizatieexceptie")]
        public string AuthorizationExceptionTypeCode { get; set; }
    }
}