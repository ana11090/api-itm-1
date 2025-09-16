using api_itm.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public sealed class ContinutContract
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "continutContract";

        [JsonPropertyName("referintaSalariat")] public ReferintaSalariat ReferintaSalariat { get; set; } = new();

        [JsonPropertyName("cor")] public Cor Cor { get; set; } = new();

        [JsonPropertyName("dataConsemnare")] public DateTime? DataConsemnare { get; set; }

        [JsonPropertyName("dataContract")] public DateTime? DataContract { get; set; }

        [JsonPropertyName("dataInceputContract")] public DateTime? DataInceputContract { get; set; }

        [JsonPropertyName("dataSfarsitContract")] public DateTime? DataSfarsitContract { get; set; }

        // Kept as string (e.g., "Art83LitH") to avoid enum mismatch
        [JsonPropertyName("exceptieDataSfarsit")] public string? ExceptieDataSfarsit { get; set; }

        [JsonPropertyName("numarContract")] public string? NumarContract { get; set; }

        [JsonPropertyName("radiat")] public bool? Radiat { get; set; }

        [JsonPropertyName("salariu")] public int? Salariu { get; set; }

        //[JsonPropertyName("salariuValuta")] public string? SalariuValuta { get; set; }  // nu sunt sigura daca avem nevoie de asta

        // Keep currency as string (e.g., "EUR")
        [JsonPropertyName("moneda")] public string? Moneda { get; set; }

        // e.g., "MG"
        [JsonPropertyName("nivelStudii")] public string? NivelStudii { get; set; }

        [JsonPropertyName("sporuriSalariu")] public List<SporSalariu> SporuriSalariu { get; set; } = new();

        // Empty object in the sample — modelled as an empty class
        [JsonPropertyName("stareCurenta")] public StareCurenta StareCurenta { get; set; } = new();

        [JsonPropertyName("timpMunca")] public TimpMunca TimpMunca { get; set; } = new();

        // Domain strings (let API validate): "ContractIndividualMunca", "Nedeterminata", etc.
        [JsonPropertyName("tipContract")] public string? TipContract { get; set; }
        [JsonPropertyName("tipDurata")] public string? TipDurata { get; set; }
        [JsonPropertyName("tipNorma")] public string? TipNorma { get; set; }
        [JsonPropertyName("tipLocMunca")] public string? TipLocMunca { get; set; }

        // 2-letter county code (e.g., "AG")
        [JsonPropertyName("judetLocMunca")] public string? JudetLocMunca { get; set; }

        [JsonPropertyName("aplicaL153")] public bool? AplicaL153 { get; set; }

        [JsonPropertyName("detaliiL153")] public DetaliiL153? DetaliiL153 { get; set; }
    }
}
