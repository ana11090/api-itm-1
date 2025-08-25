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
    public class ContinutContract
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "continutContract";

        public ReferintaSalariat ReferintaSalariat { get; set; } = new();
        public Cor Cor { get; set; } = new();

        public DateTime? DataConsemnare { get; set; }
        public DateTime? DataContract { get; set; }
        public DateTime? DataInceputContract { get; set; }

        public string? ExceptieDataSfarsit { get; set; }
        public string? NumarContract { get; set; }
        public bool? Radiat { get; set; }
        public decimal? Salariu { get; set; }
        public decimal? SalariuValuta { get; set; }
        public string? Moneda { get; set; }
        public string? NivelStudii { get; set; }

        public List<SporSalariu> SporuriSalariu { get; set; } = new();
        public StareCurenta StareCurenta { get; set; } = new();
        public TimpMunca TimpMunca { get; set; } = new();

        public string? TipContract { get; set; }
        public string? TipDurata { get; set; }
        public string? TipNorma { get; set; }
        public string? TipLocMunca { get; set; }
        public string? JudetLocMunca { get; set; }

        public bool? AplicaL153 { get; set; }
        public DetaliiL153? DetaliiL153 { get; set; }
    }
}
