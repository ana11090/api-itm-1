using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace api_itm.Models.Contracts.Envelope
{
    [NotMapped]
    public sealed class SporTip
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = default!;
        [JsonPropertyName("referinta")] public Referinta Referinta { get; set; } = new();
        [JsonPropertyName("nume")] public string Nume { get; set; } = default!;
    }

    [NotMapped]
    public class Referinta
    {
        [JsonPropertyName("$type")] public string Type { get; set; } = "referinta";
        public string Id { get; set; } = default!;
    }
}
