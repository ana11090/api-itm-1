using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Reges
{
    /// <summary>
    /// Wire (transport) header sent to the external API.
    /// Not persisted in  DB.
    /// </summary>
    public sealed class HeaderView
    {
        public string MessageId { get; set; }
        public string ClientApplication { get; set; }
        public string Version { get; set; }
        public string Operation { get; set; }
        public string User { get; set; }
        public string AuthorId { get; set; }
        public string SessionId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public sealed class RequestHeaderEnvelope<TBody>
    {
        public HeaderView Header { get; set; }
        public TBody Body { get; set; }
    }
}