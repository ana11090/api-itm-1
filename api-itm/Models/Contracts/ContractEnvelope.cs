using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using api_itm.Models.Contracts.Envelope;

namespace api_itm.Models.Contracts
{
    public sealed class ContractEnvelope
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "contract";

        [JsonPropertyName("header")] public Header Header { get; set; } = new();
        [JsonPropertyName("referintaContract")] public ReferintaContract? ReferintaContract { get; set; } = new();

        [JsonPropertyName("continut")] public ContinutContract Continut { get; set; } = new();
    }
}
