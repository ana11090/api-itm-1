using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public class ReferintaContract
    {
        [JsonPropertyName("$type")] 
        public string Type { get; set; } = "referinta";

        [JsonPropertyName("id")] 
        public string Id { get; set; } = default!;
    }
}
