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
    public sealed class DisabilityTypeConfiguration : IEntityTypeConfiguration<DisabilityType>
    {

        public void Configure(EntityTypeBuilder<DisabilityType> b)
    {
        b.ToTable("tiphandicap");

        b.HasKey(x => x.DisabilityTypeId);

        b.Property(x => x.DisabilityTypeId)
         .HasColumnName("idtiphandicap");

        b.Property(x => x.DisabilityTypeName)
         .HasColumnName("denumiretiphandicap");

        b.Property(x => x.DisabilityTypeCode)
         .HasColumnName("codtiphandicap");
    }
}
}