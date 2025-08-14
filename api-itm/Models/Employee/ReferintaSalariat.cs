using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Employee
{
    public class ReferintaSalariat
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
