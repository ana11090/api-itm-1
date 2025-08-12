using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure.Sessions
{
    public sealed class SessionManager : ISessionManager
    {
        private readonly SessionContext _ctx; // concrete, to set internal setters

        public SessionManager(ISessionContext ctx)
        {
            _ctx = (SessionContext)ctx;
        }

        public Task StartAsync(string userName, string userId)
        {
            _ctx.SessionId = Guid.NewGuid().ToString("D"); // new session per login
            _ctx.UserName = userName;
            _ctx.UserId = userId;
            return Task.CompletedTask;
        }

        public Task EndAsync()
        {
            _ctx.SessionId = null;
            _ctx.UserName = null;
            _ctx.UserId = null;
            return Task.CompletedTask;
        }
    }
}