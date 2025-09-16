using api_itm.Data.Entity.Ru.Study;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Configurations.Education
{
    public class EducationLevelConfiguration : IEntityTypeConfiguration<EducationLevel>
    {
        public void Configure(EntityTypeBuilder<EducationLevel> builder)
        {
            builder.ToTable("nivelstudii");

            builder.HasKey(x => x.EducationLevelId);

            builder.Property(x => x.EducationLevelId)
                   .HasColumnName("idnivelstudii");

            builder.Property(x => x.EducationLevelName)
                   .HasColumnName("denumirenivelstudii");

            builder.Property(x => x.EducationLevelCode)
                  .HasColumnName("codnivelstudii");

            builder.Property(x => x.EducationLevelCodeReges)
                   .HasColumnName("codnivelstudiireges");
        }
    }
}
