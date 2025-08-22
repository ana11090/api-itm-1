using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace api_itm.Models.Reges
{
    public sealed class SyncResponse
    {
        public Guid responseId { get; set; }
        public SyncHeader header { get; set; }
    }

    public sealed class SyncHeader
    {
        public string clientIP { get; set; }
        public Guid messageId { get; set; }
        public string clientApplication { get; set; }
        public string version { get; set; }
        public string operation { get; set; }
        public Guid authorId { get; set; }
        public Guid sessionId { get; set; }
        public string user { get; set; }
        public string userId { get; set; }
        public DateTime timestamp { get; set; }
    }
}
