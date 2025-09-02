using api_itm.Data.Entity.Ru.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Configurations.Contracts
{

    public class ContractStateConfiguration : IEntityTypeConfiguration<ContractState>
    {
        public void Configure(EntityTypeBuilder<ContractState> builder)
        {
            builder.ToTable("starecontract");                 // table name

            builder.HasKey(x => x.ContractStateId);

            builder.Property(x => x.ContractStateId)
                   .HasColumnName("idstarecontract");

            builder.Property(x => x.ContractStateName)
                   .HasColumnName("denumirestarecontract");

            builder.Property(x => x.ContractStateCode)
                   .HasColumnName("codstarecontract");
        }
    }
}