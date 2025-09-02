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
    public class ContractSalaryBonusConfiguration : IEntityTypeConfiguration<ContractSalaryBonus>
    {
        public void Configure(EntityTypeBuilder<ContractSalaryBonus> builder)
        {
            builder.ToTable("salarizare_contracte_sporuri");

            builder.HasKey(x => x.ContractSalaryBonusId);

            builder.Property(x => x.ContractSalaryBonusId).HasColumnName("idcontractsalarizaresporuri");
            builder.Property(x => x.ContractId).HasColumnName("idcontract");
            builder.Property(x => x.SalaryContractId).HasColumnName("idcontractsalarizare");
            builder.Property(x => x.Month).HasColumnName("luna");
            builder.Property(x => x.Year).HasColumnName("anul");
            builder.Property(x => x.StartDate).HasColumnName("datai");
            builder.Property(x => x.EndDate).HasColumnName("datas");
            builder.Property(x => x.ContractBonusHistoryId).HasColumnName("idistoriccontractesporuri");

            builder.Property(x => x.Bonus1).HasColumnName("spor1");
            builder.Property(x => x.Bonus2).HasColumnName("spor2");
            builder.Property(x => x.Bonus3).HasColumnName("spor3");
            builder.Property(x => x.Bonus4).HasColumnName("spor4");
            builder.Property(x => x.Bonus5).HasColumnName("spor5");
            builder.Property(x => x.Bonus6).HasColumnName("spor6");
            builder.Property(x => x.Bonus7).HasColumnName("spor7");
            builder.Property(x => x.Bonus8).HasColumnName("spor8");
            builder.Property(x => x.Bonus9).HasColumnName("spor9");
            builder.Property(x => x.Bonus10).HasColumnName("spor10");
            builder.Property(x => x.Bonus11).HasColumnName("spor11");
            builder.Property(x => x.Bonus12).HasColumnName("spor12");
            builder.Property(x => x.Bonus13).HasColumnName("spor13");
            builder.Property(x => x.Bonus14).HasColumnName("spor14");
            builder.Property(x => x.Bonus15).HasColumnName("spor15");
            builder.Property(x => x.Bonus16).HasColumnName("spor16");
            builder.Property(x => x.Bonus17).HasColumnName("spor17");
        }
    }
}