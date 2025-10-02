using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.SuspendariEnvelope
{
    public class Actiune
    {
        // Required by the API: "actiuneSuspendare"
        [JsonPropertyName("$type")]
        public string Type { get; set; } = "actiuneSuspendare";

        [JsonPropertyName("dataInceput")]
        public DateTime? DataInceput { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("dataSfarsit")]
        public DateTime? DataSfarsit { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("explicatie")]
        public string? Explicatie { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("temeiLegal")]
        public string? TemeiLegal { get; set; }
    }
}
