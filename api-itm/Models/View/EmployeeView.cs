using System.Text.Json.Serialization;
using api_itm.Models.Employee;
using api_itm.Models.Reges;   // <-- bring HeaderView into scope

namespace api_itm.Models.View
{
    public class EmployeeView
    {
        [JsonIgnore] public int PersonId { get; set; }   // UI key only

        [JsonPropertyName("$type")]
        public string Type { get; set; }  // maps to "$type" in JSON

        // Use the wire/transport header type
        public HeaderView Header { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("referintaSalariat")]
        public ReferintaSalariat? ReferintaSalariat { get; set; }


        [JsonPropertyName("info")]
        public EmployeeInformation Info { get; set; }
    }
}
