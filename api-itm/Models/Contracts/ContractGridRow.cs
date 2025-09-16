using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts
{
    public sealed class ContractGridRow
    {
        public int IdContract { get; set; }
        public string? NumarContract { get; set; }

        // Dates
        public DateTime? DataConsemnare { get; set; }
        public DateTime? DataContract { get; set; }
        public DateTime? DataInceputContract { get; set; }

        // Lookups / derived
        public string? ExceptieDataSfarsit { get; set; }
        public bool Radiat { get; set; }
        public int? Salariu { get; set; }
        public string? Moneda { get; set; }
        public string? NivelStudii { get; set; }

        // TimpMunca
        public string? Norma { get; set; }
        public int? Durata { get; set; }
        public string? IntervalTimp { get; set; }
        public string? Repartizare { get; set; }
        public string? RepartizareMunca { get; set; }
        public DateTime? InceputInterval { get; set; }
        public DateTime? SfarsitInterval { get; set; }
        public string? TipTura { get; set; }

        // Tipuri
        public string? TipContract { get; set; }
        public string? TipDurata { get; set; }
        public string? TipNorma { get; set; }

        // Loc munca
        public string? TipLocMunca { get; set; }

        // REGES reference shown for clarity
        public Guid? RegesEmployeeId { get; set; }
    }
}