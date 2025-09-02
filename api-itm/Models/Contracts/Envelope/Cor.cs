using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public sealed class Cor
    {
        [JsonPropertyName("cod")] public int Cod { get; set; }
        [JsonPropertyName("versiune")] public int Versiune { get; set; }
    }
}
