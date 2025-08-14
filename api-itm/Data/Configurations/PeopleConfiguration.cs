using api_itm.Data.Entity.api_itm.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_itm.Data.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("persoana");

            builder.HasKey(p => p.IdPersoana);

            builder.Property(p => p.IdPersoana).HasColumnName("idpersoana");
            builder.Property(p => p.Cnp).HasColumnName("cnp");
            builder.Property(p => p.CnpVechi).HasColumnName("cnp_vechi");
            builder.Property(p => p.Nume).HasColumnName("nume");
            builder.Property(p => p.Prenume).HasColumnName("prenume");
            builder.Property(p => p.Titlu).HasColumnName("titlu");
            builder.Property(p => p.MarcaPersoana).HasColumnName("marcapersoana");
            builder.Property(p => p.Profesie).HasColumnName("profesie");
            builder.Property(p => p.Sex).HasColumnName("sex");
            builder.Property(p => p.DataNasterii).HasColumnName("datanasterii");
            builder.Property(p => p.LoculNasterii).HasColumnName("loculnasterii");
            builder.Property(p => p.StareCivila).HasColumnName("starecivila");
            builder.Property(p => p.IdTara).HasColumnName("idtara");
            builder.Property(p => p.Cetatenie).HasColumnName("cetatenie");
            builder.Property(p => p.IdTipNationalitate).HasColumnName("idtipnationalitate");
            builder.Property(p => p.IdTipApatrid).HasColumnName("idtipapatrid");
            builder.Property(p => p.StatutSpecial).HasColumnName("statutspecial");
            builder.Property(p => p.IdTipAct).HasColumnName("idtipact");
            builder.Property(p => p.SeriaActIdentitate).HasColumnName("seriaactidentitate");
            builder.Property(p => p.NumarActIdentitate).HasColumnName("numaractidentitate");
            builder.Property(p => p.OrganEmitentActIdentitate).HasColumnName("organemitentactidentitate");
            builder.Property(p => p.DataEliberareActIdentitate).HasColumnName("dataeliberareactidentitate");
            builder.Property(p => p.ValabilitateActIdentitate).HasColumnName("valabilitateactidentitate");
            builder.Property(p => p.ScopPermisDeSedere).HasColumnName("scoppermisdesedere");
            builder.Property(p => p.IdTaraDomiciliu).HasColumnName("idtaradomiciliu");
            builder.Property(p => p.IdJudet).HasColumnName("idjudet");
            builder.Property(p => p.IdLocalitate).HasColumnName("idlocalitate");
            builder.Property(p => p.CodSiruta).HasColumnName("codsiruta");
            builder.Property(p => p.Adresa).HasColumnName("adresa");
            builder.Property(p => p.Telefon).HasColumnName("telefon");
            builder.Property(p => p.TelefonUbb).HasColumnName("telefon_ubb");
            builder.Property(p => p.Email).HasColumnName("email");
            builder.Property(p => p.EmailUbb).HasColumnName("email_ubb");
            builder.Property(p => p.AdresaCorespondenta).HasColumnName("adresacorespondenta");
            builder.Property(p => p.IdTipAutorizatieMunca).HasColumnName("idtipautorizatiemunca");
            builder.Property(p => p.DataInceputAutorizatie).HasColumnName("datainceputautorizatie");
            builder.Property(p => p.DataSfarsitAutorizatie).HasColumnName("datasfarsitautorizatie");
            builder.Property(p => p.SitMilitara).HasColumnName("sitmilitara");
            builder.Property(p => p.SerieLivret).HasColumnName("serialivret");
            builder.Property(p => p.NumarLivret).HasColumnName("numarlivret");
            builder.Property(p => p.GradMilitar).HasColumnName("gradmilitar");
            builder.Property(p => p.SpecialitateMilitara).HasColumnName("specialitatemilitara");
            builder.Property(p => p.IdCasaDeSanatate).HasColumnName("idcasadesanatate");
            builder.Property(p => p.Pensionar).HasColumnName("pensionar");
            builder.Property(p => p.IdSistemAsigurariSociale).HasColumnName("idsistemasigurarisociale");
            builder.Property(p => p.ScutitCAS).HasColumnName("scutitcas");
            builder.Property(p => p.AnDocumentPortabil).HasColumnName("andocumentportabil");
            builder.Property(p => p.DovadaAsiguratUE_SEE).HasColumnName("dovadaasiguratue_see");
            builder.Property(p => p.IdGradHandicap).HasColumnName("idgradhandicap");
            builder.Property(p => p.IdCaracterHandicap).HasColumnName("idcaracterhandicap");
            builder.Property(p => p.DataRevizuireHandicap).HasColumnName("datarevizuirehandicap");
            builder.Property(p => p.AnCertificatRezidentaFiscala).HasColumnName("ancertificatrezidentafiscala");
            builder.Property(p => p.MembruDeSindicat).HasColumnName("membrudesindicat");
            builder.Property(p => p.Student).HasColumnName("student");
            builder.Property(p => p.IdFormaStudii).HasColumnName("idformastudii");
            builder.Property(p => p.ValabilitateAdeverinta).HasColumnName("valabilitateadeverinta");
            builder.Property(p => p.FunctieBaza).HasColumnName("functie_baza");
            builder.Property(p => p.InstitutieDeBaza).HasColumnName("institutiedebaza");
            builder.Property(p => p.Nerezident).HasColumnName("nerezident");
            builder.Property(p => p.Gravida).HasColumnName("gravida");
            builder.Property(p => p.ReducereTimpLucruGravida).HasColumnName("reduceretimplucrugravida");
            builder.Property(p => p.Alapteaza).HasColumnName("alapteaza");
            builder.Property(p => p.ReducereTimpLucruAlaptare).HasColumnName("reduceretimplucrualaptare");
            builder.Property(p => p.ConstituireDosarPersonal).HasColumnName("constituiredosarpersonal");
            builder.Property(p => p.LocatieArhivareDosarPersonal).HasColumnName("locatiearhivaredosarpersonal");
            builder.Property(p => p.LocatieArhivareDocumentatie).HasColumnName("locatiearhivaredocumentatie");
            builder.Property(p => p.ValabilitateDocumentatie).HasColumnName("valabilitatedocumentatie");
            builder.Property(p => p.ObservatiiDocumentatie).HasColumnName("observatiidocumentatie");
            builder.Property(p => p.DataAngajare).HasColumnName("dataangajare");
            builder.Property(p => p.Observatii).HasColumnName("observatii");
            builder.Property(p => p.Stare).HasColumnName("stare");
            builder.Property(p => p.DataModificare).HasColumnName("datamodificare");
            builder.Property(p => p.Preluat).HasColumnName("preluat");
            builder.Property(p => p.IdGradatie).HasColumnName("idgradatie");
            builder.Property(p => p.IdTransaVechimeInv).HasColumnName("idtransavechevechimeinv");
            builder.Property(p => p.IdTransaVechimeInv2).HasColumnName("idtransavechimeinv");
            builder.Property(p => p.AdresaMod).HasColumnName("adresamod");
            builder.Property(p => p.AdresaInitiala).HasColumnName("adresainitiala");
            builder.Property(p => p.DeSincronizat).HasColumnName("desincronizat");
            builder.Property(p => p.IdGradInvaliditate).HasColumnName("idgradinvaliditate");
            builder.Property(p => p.IdTipHandicap).HasColumnName("idtiphandicap");
            builder.Property(p => p.NrCertificatHandicap).HasColumnName("nrcertificathandicap");
            builder.Property(p => p.DataCertificatHandicap).HasColumnName("datacertificathandicap");
            builder.Property(p => p.NrAviz).HasColumnName("nraviz");
            builder.Property(p => p.DataAviz).HasColumnName("dataaviz");
            builder.Property(p => p.LocalitateNastere).HasColumnName("localitate_nastere");
        }
    }
}
