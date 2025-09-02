using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Salary
{
    public class ContractSalaryBonus
    {
        public int ContractSalaryBonusId { get; set; }   // idcontractsalarizaresporuri
        public int ContractId { get; set; }              // idcontract
        public int? SalaryContractId { get; set; }       // idcontractsalarizare
        public int? Month { get; set; }                  // luna
        public int? Year { get; set; }                   // anul
        public DateTime? StartDate { get; set; }         // datai
        public DateTime? EndDate { get; set; }           // datas
        public int? ContractBonusHistoryId { get; set; } // idistoriccontractesporuri

        public decimal? Bonus1 { get; set; }             // spor1
        public decimal? Bonus2 { get; set; }             // spor2
        public decimal? Bonus3 { get; set; }             // spor3
        public decimal? Bonus4 { get; set; }             // spor4
        public decimal? Bonus5 { get; set; }             // spor5
        public decimal? Bonus6 { get; set; }             // spor6
        public decimal? Bonus7 { get; set; }             // spor7
        public decimal? Bonus8 { get; set; }             // spor8
        public decimal? Bonus9 { get; set; }             // spor9
        public decimal? Bonus10 { get; set; }            // spor10
        public decimal? Bonus11 { get; set; }            // spor11
        public decimal? Bonus12 { get; set; }            // spor12
        public decimal? Bonus13 { get; set; }            // spor13
        public decimal? Bonus14 { get; set; }            // spor14
        public decimal? Bonus15 { get; set; }            // spor15
        public decimal? Bonus16 { get; set; }            // spor16
        public decimal? Bonus17 { get; set; }            // spor17
    }
} 