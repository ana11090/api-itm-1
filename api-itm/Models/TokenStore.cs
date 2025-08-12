using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models
{
    public class TokenStore
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Password { get; set; }
        public DateTime Expiration { get; set; } 
        public bool IsExpiringSoon() => DateTime.UtcNow >= Expiration.AddSeconds(-60); //not used for now, maybe usefull later for checking the token before sending the api
    }

}
