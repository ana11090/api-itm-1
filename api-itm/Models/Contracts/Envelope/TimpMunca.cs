using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts.Envelope
{
    public class TimpMunca
    {
        public string? Norma { get; set; }
        public int? Durata { get; set; }
        public string? IntervalTimp { get; set; }
        public string? Repartizare { get; set; }
        public string? RepartizareMunca { get; set; }

        public DateTime? InceputInterval { get; set; }
        public DateTime? SfarsitInterval { get; set; }

        public string? NotaRepartizareMunca { get; set; }
        public string? TipTura { get; set; }
        public string? ObservatiiTipTuraAlta { get; set; }
    }
}
