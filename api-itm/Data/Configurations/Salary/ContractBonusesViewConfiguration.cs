using api_itm.Data.Entity.Ru.Salary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Configurations.Salary
{
    public class ContractBonusesViewConfiguration : IEntityTypeConfiguration<ContractBonusesView>
    {
        public void Configure(EntityTypeBuilder<ContractBonusesView> builder)
        {
            // It's a VIEW, not a table, and it has no key
            builder.ToView("view_contracte_sporuri");
            builder.HasNoKey();

            builder.Property(e => e.ContractId).HasColumnName("idcontract");

            builder.Property(e => e.ToxicityBonus).HasColumnName("sportox");
            builder.Property(e => e.SeniorityBonus).HasColumnName("sporvech");
            builder.Property(e => e.CfpBonus).HasColumnName("sporcfp");
            builder.Property(e => e.SpecialConditionsAllowance).HasColumnName("indemniz_cond");
            builder.Property(e => e.DifferentiatedSalary).HasColumnName("sal_diferentiat");
            builder.Property(e => e.UninterruptedSeniorityBonus).HasColumnName("sporvechneintrerupt");
            builder.Property(e => e.MeritGradeBonus).HasColumnName("gradatiemerit");
            builder.Property(e => e.ScientificTitleBonus).HasColumnName("sportitlustiintific");
        }
    }
}