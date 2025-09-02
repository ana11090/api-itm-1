using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public sealed class DetaliiL153
    {
        [JsonPropertyName("anexaL153")] public string? AnexaL153 { get; set; }
        [JsonPropertyName("capitolL153")] public string? CapitolL153 { get; set; }
        [JsonPropertyName("literaL153")] public string? LiteraL153 { get; set; }
        [JsonPropertyName("clasificareSuplimentaraL153")] public string? ClasificareSuplimentaraL153 { get; set; }
        [JsonPropertyName("functieL153")] public string? FunctieL153 { get; set; }
        [JsonPropertyName("specialitateFunctieL153")] public string? SpecialitateFunctieL153 { get; set; }
        [JsonPropertyName("structuraAprobataL153")] public string? StructuraAprobataL153 { get; set; }
        [JsonPropertyName("specialitateStructuraAprobataL153")] public string? SpecialitateStructuraAprobataL153 { get; set; }
        [JsonPropertyName("gradProfesionalL153")] public string? GradProfesionalL153 { get; set; }
        [JsonPropertyName("gradatieL153")] public string? GradatieL153 { get; set; }
        [JsonPropertyName("denumireAltaFunctieL153")] public string? DenumireAltaFunctieL153 { get; set; }
        [JsonPropertyName("explicatieFunctieL153")] public string? ExplicatieFunctieL153 { get; set; }
        [JsonPropertyName("altGradProfesionalL153")] public string? AltGradProfesionalL153 { get; set; }
    }
}
