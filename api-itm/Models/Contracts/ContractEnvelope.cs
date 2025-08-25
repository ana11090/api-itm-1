using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts
{
    public class ContractEnvelope
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "contract";
        public Header Header { get; set; } = new();
        public ContinutContract Continut { get; set; } = new();
    }
}
