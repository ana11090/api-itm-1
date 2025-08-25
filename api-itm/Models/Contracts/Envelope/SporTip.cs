using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public class SporTip
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = default!;
        public Referinta Referinta { get; set; } = new();
        public string Nume { get; set; } = default!;
    }

    public class Referinta
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "referinta";
        public string Id { get; set; } = default!;
    }
}
