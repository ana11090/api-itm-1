using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Salary
{
    public class ContractBonuses
    {
        public int IdContracteSporuri { get; set; }  // idcontractesporuri (PK)
        public int IdContract { get; set; }  // idcontract
        public int IdSpor { get; set; }  // idspor
        public decimal? ValoareSpor { get; set; }  // valoarespor
        public decimal? ValoareSporProcent { get; set; }  // valoaresporprocent
        public DateTime? DataInceputSpor { get; set; }  // datainceputspor
        public DateTime? DataSfarsitSpor { get; set; }  // datasfarsitspor
        public DateTime? DataIncetareSpor { get; set; }  // dataincetarespor
        public DateTime? DataModificareSpor { get; set; }  // datamodificarespor
        public int? IdTipCompensatie { get; set; }  // idtipcompensatie
        public bool? ModificSpor { get; set; }  // modificspor (flag)
    }
}