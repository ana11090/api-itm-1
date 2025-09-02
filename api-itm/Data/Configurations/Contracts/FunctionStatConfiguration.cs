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
    public class FunctionStatConfiguration : IEntityTypeConfiguration<FunctionStat>
    {
        public void Configure(EntityTypeBuilder<FunctionStat> builder)
        {
            builder.ToTable("statfunctii");

            builder.HasKey(x => x.FunctionStatId);

            builder.Property(x => x.FunctionStatId).HasColumnName("idstatfunctii");
            builder.Property(x => x.UnitCode).HasColumnName("codunitate");
            builder.Property(x => x.FunctionTypeId).HasColumnName("idtipfunctie");
            builder.Property(x => x.AcademicYearId).HasColumnName("idanuniversitar");
            builder.Property(x => x.LeadershipFunctionId).HasColumnName("idfunctieconducere");
            builder.Property(x => x.PostCode).HasColumnName("codpost");
            builder.Property(x => x.FunctionId).HasColumnName("idfunctie");
            builder.Property(x => x.QualificationTypeId).HasColumnName("idtipcalificare");
            builder.Property(x => x.EducationLevelId).HasColumnName("idnivelstudii");
            builder.Property(x => x.ProfessionalGradeId).HasColumnName("idgradprofesional");
            builder.Property(x => x.Period).HasColumnName("perioada");
            builder.Property(x => x.CreatedAt).HasColumnName("datacreare");
            builder.Property(x => x.AbolishedAt).HasColumnName("datadesfiintare");
            builder.Property(x => x.EstablishmentAct).HasColumnName("actinfiintare");
            builder.Property(x => x.DidacticLoad).HasColumnName("incarcaturadidactica");
            builder.Property(x => x.Position).HasColumnName("pozitie");
            builder.Property(x => x.Status).HasColumnName("stare");
            builder.Property(x => x.Notes).HasColumnName("observatii");
            builder.Property(x => x.CompetitionProcedure).HasColumnName("proceduradeconcurs");
            builder.Property(x => x.DaDi).HasColumnName("da_di");
            builder.Property(x => x.DocumentId).HasColumnName("iddocument");
            builder.Property(x => x.TransferDate).HasColumnName("datatransfer");
            builder.Property(x => x.TransferAct).HasColumnName("acttransfer");
            builder.Property(x => x.DeletionDate).HasColumnName("datastergere");
            builder.Property(x => x.AbolishmentAct).HasColumnName("actdesfiintare");
            builder.Property(x => x.TransformationDate).HasColumnName("datatransformare");
            builder.Property(x => x.TransformationAct).HasColumnName("acttransformare");
        }
    }
}