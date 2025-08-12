using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Sessions
{
    public interface ISessionContext
    {
        string SessionId { get; }
        string UserName { get; }
        string UserId { get; }
        bool IsAuthenticated { get; }
    }
}