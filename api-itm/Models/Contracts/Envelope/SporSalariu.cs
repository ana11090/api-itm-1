using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public class SporSalariu
    {
        public bool IsProcent { get; set; }
        public decimal Valoare { get; set; }
        public string? Moneda { get; set; }
        public SporTip Tip { get; set; } = new();
    }
}
