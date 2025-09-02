using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public sealed class SporSalariu
    {
        [JsonPropertyName("isProcent")] public bool IsProcent { get; set; }

        [JsonPropertyName("valoare")] public decimal Valoare { get; set; }

        // Optional; present in the 2nd item of the sample
        [JsonPropertyName("moneda")] public string? Moneda { get; set; }

        [JsonPropertyName("tip")] public SporTip Tip { get; set; } = new();
    }

}
