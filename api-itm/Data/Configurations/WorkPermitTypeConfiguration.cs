using api_itm.Data.Entity.Ru.ForginerWorkAuthorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Configurations
{
    public class WorkPermitTypeConfiguration : IEntityTypeConfiguration<WorkPermitType>
    {
        public void Configure(EntityTypeBuilder<WorkPermitType> builder)
        {
            builder.ToTable("tipautorizatiemunca");

            builder.HasKey(w => w.WorkPermitId);

            builder.Property(w => w.WorkPermitId)
                   .HasColumnName("idtipautorizatiemunca")
                   .IsRequired();

            builder.Property(w => w.WorkPermitName)
                   .HasColumnName("denumiretipautorizatiemunca")
                   .HasMaxLength(200) // optional, if you know the DB column length
                   .IsRequired();

            builder.Property(w => w.WorkPermitCode)
                   .HasColumnName("codtipautorizatiemunca")
                   .HasMaxLength(50) // optional, if you know the DB column length
                   .IsRequired();
        }
    }
}