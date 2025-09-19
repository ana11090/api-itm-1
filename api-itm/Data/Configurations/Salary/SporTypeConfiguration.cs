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
    public class SporTypeConfiguration : IEntityTypeConfiguration<SporType>
    {
        public void Configure(EntityTypeBuilder<SporType> builder)
        {
            builder.ToTable("tipspor");

            builder.HasKey(e => e.SporTypeId);
            builder.Property(e => e.SporTypeId).HasColumnName("idspor");

            builder.Property(e => e.SporName)
                   .HasColumnName("denumirespor");

            builder.Property(e => e.SporCode)
                   .HasColumnName("codspor");

            builder.Property(e => e.SporTypeVersion)
                   .HasColumnName("versiunetipspor"); 

            builder.Property(e => e.SporTypeCode)
                   .HasColumnName("codtipspor");

            builder.Property(e => e.RegesId)
                   .HasColumnName("reges_id"); 
        }
    }
}