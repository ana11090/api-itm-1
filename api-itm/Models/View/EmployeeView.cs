using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using api_itm.Models.Employee;

namespace api_itm.Models.View
{
    public class EmployeeView
    {
        [JsonPropertyName("$type")]
        public string Type { get; set; }  // maps to "$type" in JSON

        public Header Header { get; set; }

        [JsonPropertyName("referintaSalariat")]
        public ReferintaSalariat ReferintaSalariat { get; set; }

        [JsonPropertyName("info")]

        public EmployeeInformation Info { get; set; }
    }

   
}