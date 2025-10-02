using api_itm.Data.Entity.Ru;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_itm.Data.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("persoana");

            // Key
            builder.HasKey(p => p.PersonId);
            builder.Property(p => p.PersonId).HasColumnName("idpersoana");

            // Strings / simple columns
            builder.Property(p => p.NationalId).HasColumnName("cnp");
            builder.Property(p => p.OldNationalId).HasColumnName("cnp_vechi");

            // nume / prenume are nullable in DB -> not required here
            builder.Property(p => p.LastName).HasColumnName("nume").IsRequired(false);
            builder.Property(p => p.FirstName).HasColumnName("prenume").IsRequired(false);

            builder.Property(p => p.Title).HasColumnName("titlu");
            builder.Property(p => p.EmployeeCode).HasColumnName("marcapersoana");
            builder.Property(p => p.Occupation).HasColumnName("profesie");
            builder.Property(p => p.Sex).HasColumnName("sex");

            builder.Property(p => p.BirthDate).HasColumnName("datanasterii");
            builder.Property(p => p.BirthPlace).HasColumnName("loculnasterii");
            builder.Property(p => p.MaritalStatus).HasColumnName("starecivila");

            builder.Property(p => p.CountryId).HasColumnName("idtara");

            // cetatenie is NOT NULL in DB -> mark required
            builder.Property(p => p.Citizenship).HasColumnName("cetatenie").IsRequired();

            builder.Property(p => p.NationalityTypeId).HasColumnName("idtipnationalitate");
            builder.Property(p => p.IdTipApatrid).HasColumnName("idtipapatrid");
            builder.Property(p => p.SpecialStatus).HasColumnName("statutspecial");

            builder.Property(p => p.IdentityDocTypeId).HasColumnName("idtipact");
            builder.Property(p => p.IdentitySeries).HasColumnName("seriaactidentitate");
            builder.Property(p => p.IdentityNumber).HasColumnName("numaractidentitate");
            builder.Property(p => p.IdentityIssuer).HasColumnName("organemitentactidentitate");
            builder.Property(p => p.IdentityIssueDate).HasColumnName("dataeliberareactidentitate");
            builder.Property(p => p.IdentityExpiryDate).HasColumnName("valabilitateactidentitate");

            builder.Property(p => p.ResidencePermitPurpose).HasColumnName("scoppermisdesedere");

            builder.Property(p => p.DomicileCountryId).HasColumnName("idtaradomiciliu");
            builder.Property(p => p.CountyId).HasColumnName("idjudet");
            builder.Property(p => p.CityId).HasColumnName("idlocalitate");
            builder.Property(p => p.SirutaCode).HasColumnName("codsiruta");

            builder.Property(p => p.Address).HasColumnName("adresa");
            builder.Property(p => p.Phone).HasColumnName("telefon");
            builder.Property(p => p.PhoneUbb).HasColumnName("telefon_ubb");
            builder.Property(p => p.Email).HasColumnName("email");
            builder.Property(p => p.EmailUbb).HasColumnName("email_ubb");
            builder.Property(p => p.CorrespondenceAddress).HasColumnName("adresacorespondenta");

            builder.Property(p => p.WorkPermitTypeId).HasColumnName("idtipautorizatiemunca");
            builder.Property(p => p.WorkPermitStartDate).HasColumnName("datainceputautorizatie");
            builder.Property(p => p.WorkPermitEndDate).HasColumnName("datasfarsitautorizatie");

            builder.Property(p => p.MilitaryStatus).HasColumnName("sitmilitara");
            builder.Property(p => p.MilitaryBookSeries).HasColumnName("serialivret");
            builder.Property(p => p.MilitaryBookNumber).HasColumnName("numarlivret");
            builder.Property(p => p.MilitaryRank).HasColumnName("gradmilitar");
            builder.Property(p => p.MilitarySpecialty).HasColumnName("specialitatemilitara");

            builder.Property(p => p.HealthInsuranceHouseId).HasColumnName("idcasadesanatate");

            builder.Property(p => p.Retired).HasColumnName("pensionar");
            builder.Property(p => p.SocialInsuranceSystemId).HasColumnName("idsistemasigurarisociale");
            builder.Property(p => p.CasExempt).HasColumnName("scutitcas");

            builder.Property(p => p.PortableDocYear).HasColumnName("andocumentportabil");
            builder.Property(p => p.EuSeeInsuredProof).HasColumnName("dovadaasiguratue_see");

            builder.Property(p => p.DisabilityGradeId).HasColumnName("idgradhandicap");
            builder.Property(p => p.DisabilityCharacterId).HasColumnName("idcaracterhandicap");
            builder.Property(p => p.DisabilityReviewDate).HasColumnName("datarevizuirehandicap");
            builder.Property(p => p.DisabilityReviewDate).HasColumnName("datarevizuirehandicap");

            builder.Property(p => p.TaxResidenceCertificateYear).HasColumnName("ancertificatrezidentafiscala");

            builder.Property(p => p.UnionMember).HasColumnName("membrudesindicat");
            builder.Property(p => p.Student).HasColumnName("student");
            builder.Property(p => p.StudyFormId).HasColumnName("idformastudii");
            builder.Property(p => p.CertificateValidity).HasColumnName("valabilitateadeverinta");

            builder.Property(p => p.BaseFunction).HasColumnName("functie_baza");
            builder.Property(p => p.BaseInstitution).HasColumnName("institutiedebaza");
            builder.Property(p => p.NonResident).HasColumnName("nerezident");
            builder.Property(p => p.Pregnant).HasColumnName("gravida");
            builder.Property(p => p.ReducedWorkTimePregnancy).HasColumnName("reduceretimplucrugravida");
            builder.Property(p => p.Breastfeeding).HasColumnName("alapteaza");
            builder.Property(p => p.ReducedWorkTimeBreastfeeding).HasColumnName("reduceretimplucrualaptare");

            builder.Property(p => p.PersonalFileSetup).HasColumnName("constituiredosarpersonal");
            builder.Property(p => p.PersonalFileArchiveLocation).HasColumnName("locatiearhivaredosarpersonal");
            builder.Property(p => p.DocumentationArchiveLocation).HasColumnName("locatiearhivaredocumentatie");
            builder.Property(p => p.DocumentationValidity).HasColumnName("valabilitatedocumentatie");
            builder.Property(p => p.DocumentationNotes).HasColumnName("observatiidocumentatie");

            builder.Property(p => p.HireDate).HasColumnName("dataangajare");
            builder.Property(p => p.Notes).HasColumnName("observatii");
            builder.Property(p => p.Status).HasColumnName("stare");
            builder.Property(p => p.ModifiedAt).HasColumnName("datamodificare");
            builder.Property(p => p.Imported).HasColumnName("preluat");

            builder.Property(p => p.SeniorityGradeId).HasColumnName("idgradatie");

            // Both seniority columns
            builder.Property(p => p.SeniorityBandOldId).HasColumnName("idtransavechevechimeinv");
            builder.Property(p => p.SeniorityBandId).HasColumnName("idtransavechimeinv");

            builder.Property(p => p.AddressModified).HasColumnName("adresamod");
            builder.Property(p => p.InitialAddress).HasColumnName("adresainitiala");

            builder.Property(p => p.Desynchronized).HasColumnName("desincronizat");

            builder.Property(p => p.InvalidityGradeId).HasColumnName("idgradinvaliditate");
            builder.Property(p => p.HandicapTypeId).HasColumnName("idtiphandicap");
            builder.Property(p => p.HandicapCertificateNumber).HasColumnName("nrcertificathandicap");
            builder.Property(p => p.HandicapCertificateDate).HasColumnName("datacertificathandicap");

            builder.Property(p => p.ApprovalNumber).HasColumnName("nraviz");
            builder.Property(p => p.ApprovalDate).HasColumnName("dataaviz");

            builder.Property(p => p.BirthLocality).HasColumnName("localitate_nastere");

            builder.Property(c => c.RegesSyncVariable).HasColumnName("reges_sincronizare");//RegesSyncVariable

        }
    }
}
