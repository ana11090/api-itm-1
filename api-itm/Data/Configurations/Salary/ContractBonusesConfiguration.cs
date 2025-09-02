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

    public class ContractBonusesConfiguration : IEntityTypeConfiguration<ContractBonuses>
    {
        public void Configure(EntityTypeBuilder<ContractBonuses> builder)
        {
            builder.ToTable("contracte_sporuri");

            builder.HasKey(x => x.IdContracteSporuri);

            builder.Property(x => x.IdContracteSporuri).HasColumnName("idcontractesporuri");
            builder.Property(x => x.IdContract).HasColumnName("idcontract");
            builder.Property(x => x.IdSpor).HasColumnName("idspor");
            builder.Property(x => x.ValoareSpor).HasColumnName("valoarespor");
            builder.Property(x => x.ValoareSporProcent).HasColumnName("valoaresporprocent");
            builder.Property(x => x.DataInceputSpor).HasColumnName("datainceputspor");
            builder.Property(x => x.DataSfarsitSpor).HasColumnName("datasfarsitspor");
            builder.Property(x => x.DataIncetareSpor).HasColumnName("dataincetarespor");
            builder.Property(x => x.DataModificareSpor).HasColumnName("datamodificarespor");
            builder.Property(x => x.IdTipCompensatie).HasColumnName("idtipcompensatie");
            builder.Property(x => x.ModificSpor).HasColumnName("modificspor");
        }
    }
}