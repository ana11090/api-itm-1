using api_itm.Infrastructure.Sessions;
using api_itm.Models.Reges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using api_itm.Data.Entity;

namespace api_itm.Infrastructure.Header
{
    public class HeaderFactory
    {
        public static string ClientApplication = "api_itm_client";
        public static string Version = "1.0.0";

        // Map these to YOUR actual property names on User
        public static HeaderView Create(string operation, User user,  ISessionContext sesion)
     => new HeaderView
     {
         MessageId = Guid.NewGuid().ToString("D"),
         ClientApplication = ClientApplication,
         Version = Version,
         Operation = operation,
         User = user.Username,
         AuthorId = user.IdUser.ToString(),
         SessionId = sesion.SessionId,
         Timestamp = DateTime.UtcNow
     };

    }
}