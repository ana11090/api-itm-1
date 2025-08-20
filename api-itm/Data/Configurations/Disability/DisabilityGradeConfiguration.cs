using api_itm.Data.Entity.Ru.Disability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Configurations.Disability
{
    public sealed class DisabilityGradeConfiguration : IEntityTypeConfiguration<DisabilityGrade>
    {
        public void Configure(EntityTypeBuilder<DisabilityGrade> b)
        {
            b.ToTable("gradhandicap");

            b.HasKey(x => x.DisabilityGradeId);

            b.Property(x => x.DisabilityGradeId)
             .HasColumnName("idgradhandicap");

            b.Property(x => x.DisabilityGradeName)
             .HasColumnName("denumiregradhandicap");

            b.Property(x => x.DisabilityGradeCode)
             .HasColumnName("codgradhandicap");
        }
    }
}
