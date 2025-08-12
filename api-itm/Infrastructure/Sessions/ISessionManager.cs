using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Sessions
{
    internal interface ISessionManager
    {
        // Writes/updates the session info after login/refresh/logout
        public interface ISessionManager
        {
            Task StartAsync(string userName, string userId);
            Task EndAsync();
        }
    }
}
