using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Reges
{
    public sealed class MessageResult
    {
        public string messageId { get; set; }
        public string receiptId { get; set; }
        public bool success { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public DateTime? timestamp { get; set; }
    }

    public sealed class AckRequest
    {
        public string messageId { get; set; }
    }
}
