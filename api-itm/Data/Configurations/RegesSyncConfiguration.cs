using api_itm.Data.Entity.Ru;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_itm.Data.Configurations.Ru
{
    public class RegesSyncConfiguration : IEntityTypeConfiguration<RegesSync>
    {
        // api_itm.Data.Configurations.Ru.RegesSyncConfiguration
        public void Configure(EntityTypeBuilder<RegesSync> b)
        {
            b.ToTable("idsreges");
            b.HasKey(e => e.Id);

            b.Property(e => e.Id).HasColumnName("id").IsRequired();

            // these names come from your SQL helper
            b.Property(e => e.PersonId).HasColumnName("idpersoana");               // INTEGER (we’ll fix schema below)
            b.Property(e => e.UserId).HasColumnName("idutilizator");               // INTEGER (we’ll fix schema below)
            b.Property(e => e.MessageResponseId).HasColumnName("id_raspuns_mesaj"); // UUID
            b.Property(e => e.MessageResultId).HasColumnName("id_rezultat_mesaj");  // UUID
            b.Property(e => e.AuthorId).HasColumnName("idautor");
            b.Property(e => e.RegesEmployeeId).HasColumnName("reges_salariat_id"); 

            b.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            b.Property(e => e.ErrorMessage).HasColumnName("error_message");
            b.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            b.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
        }
    }
}