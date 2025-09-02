using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{

    public sealed class TimpMunca
    {
        // Many are domain codes — keep as strings
        [JsonPropertyName("norma")] public string? Norma { get; set; }
        [JsonPropertyName("durata")] public int? Durata { get; set; }
        [JsonPropertyName("intervalTimp")] public string? IntervalTimp { get; set; }
        [JsonPropertyName("repartizare")] public string? Repartizare { get; set; }
        [JsonPropertyName("repartizareMunca")] public string? RepartizareMunca { get; set; }

        [JsonPropertyName("inceputInterval")] public DateTime? InceputInterval { get; set; }
        [JsonPropertyName("sfarsitInterval")] public DateTime? SfarsitInterval { get; set; }

        [JsonPropertyName("notaRepartizareMunca")] public string? NotaRepartizareMunca { get; set; }

        [JsonPropertyName("tipTura")] public string? TipTura { get; set; }

        [JsonPropertyName("observatiiTipTuraAlta")] public string? ObservatiiTipTuraAlta { get; set; }
    }
}
