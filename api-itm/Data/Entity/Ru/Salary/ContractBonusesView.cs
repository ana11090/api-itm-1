using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Salary
{
    /// <summary>
    /// Read-only EF Core entity for DB view "view_contracte_sporuri".
    /// </summary>
    [Keyless]
    public class ContractBonusesView
    {
        /// <summary>DB: idcontract</summary>
        public int ContractId { get; set; }

        /// <summary>DB: sportox</summary>
        public decimal? ToxicityBonus { get; set; }

        /// <summary>DB: sporvech</summary>
        public decimal? SeniorityBonus { get; set; }

        /// <summary>DB: sporcfp</summary>
        public decimal? CfpBonus { get; set; }

        /// <summary>DB: indemniz_cond</summary>
        public decimal? SpecialConditionsAllowance { get; set; }

        /// <summary>DB: sal_diferentiat</summary>
        public decimal? DifferentiatedSalary { get; set; }

        /// <summary>DB: sporvechneintrerupt</summary>
        public decimal? UninterruptedSeniorityBonus { get; set; }

        /// <summary>DB: gradatiemerit</summary>
        public decimal? MeritGradeBonus { get; set; }

        /// <summary>DB: sportitlustiintific</summary>
        public decimal? ScientificTitleBonus { get; set; }
    }
}