using api_itm.Data.Entity.Ru;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_itm.Data.Configurations
{
    public class TypePaPartideConfiguration: IEntityTypeConfiguration<TypePapartid>
    {
        public void Configure(EntityTypeBuilder<TypePapartid> builder)
        {
            builder.ToTable("tipapatrid", schema: "ru");
            builder.HasKey(t => t.IdTypePapartid);
            builder.Property(t => t.IdTypePapartid).HasColumnName("idtipapatrid");
            builder.Property(t => t.PaPartidName).HasColumnName("denumiretipapatrid");
            builder.Property(t => t.CodPaPatrid).HasColumnName("codtipapatrid");
        }
    }
}
