using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Sessions
{
    // Holds the current session in memory 
    public sealed class SessionContext : ISessionContext
    {
        public string SessionId { get; internal set; }
        public string UserName { get; internal set; }
        public string UserId { get; internal set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(SessionId);
    }
}