using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.SuspendariEnvelope
{
    public class DocumentJustificativ
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("tipDocumentJustificativ")]
        public string? TipDocumentJustificativ { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("numarDocumentJustificativ")]
        public string? NumarDocumentJustificativ { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("dataDocumentJustificativ")]
        public DateTime? DataDocumentJustificativ { get; set; }
    }
}
