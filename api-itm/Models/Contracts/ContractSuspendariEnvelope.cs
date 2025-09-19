using api_itm.Models.Contracts.Envelope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts
{
    public class ContractSuspendariEnvelope
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "contract";

        [JsonPropertyName("header")] public Header Header { get; set; } = new();

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("actiune")]
        //public Actiune? Actiune { get; set; }

        //[JsonPropertyName("documentJustificativ")] public DocumentJustificativ DocumentJustificativ { get; set; } = new();
    }
}
