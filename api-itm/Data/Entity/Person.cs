using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace api_itm.Domain.Employees
    {
        [Table("persoana")]
        public class Person
        {
            [Key]
            [Column("idpersoana")]
            public int IdPersoana { get; set; }

            [Column("cnp")]
            public string Cnp { get; set; }

            [Column("cnp_vechi")]
            public string CnpVechi { get; set; }

            [Column("nume")]
            public string Nume { get; set; }

            [Column("prenume")]
            public string Prenume { get; set; }

            [Column("titlu")]
            public string Titlu { get; set; }

            [Column("marcapersoana")]
            public string MarcaPersoana { get; set; }

            [Column("profesie")]
            public string Profesie { get; set; }

            [Column("sex")]
            public string Sex { get; set; }

            [Column("datanasterii")]
            public DateTime? DataNasterii { get; set; }

            [Column("loculnasterii")]
            public string LoculNasterii { get; set; }

            [Column("starecivila")]
            public string StareCivila { get; set; }

            [Column("idtara")]
            public int? IdTara { get; set; }

            [Column("cetatenie")]
            public string Cetatenie { get; set; }

            [Column("idtipnationalitate")]
            public int? IdTipNationalitate { get; set; }

            [Column("idtipapatrid")]
            public int? IdTipApatrid { get; set; }

            [Column("statutspecial")]
            public string StatutSpecial { get; set; }

            [Column("idtipact")]
            public int? IdTipAct { get; set; }

            [Column("seriaactidentitate")]
            public string SeriaActIdentitate { get; set; }

            [Column("numaractidentitate")]
            public string NumarActIdentitate { get; set; }

            [Column("organemitentactidentitate")]
            public string OrganEmitentActIdentitate { get; set; }

            [Column("dataeliberareactidentitate")]
            public DateTime? DataEliberareActIdentitate { get; set; }

            [Column("valabilitateactidentitate")]
            public DateTime? ValabilitateActIdentitate { get; set; }

            [Column("scoppermisdesedere")]
            public string ScopPermisDeSedere { get; set; }

            [Column("idtaradomiciliu")]
            public int? IdTaraDomiciliu { get; set; }

            [Column("idjudet")]
            public int? IdJudet { get; set; }

            [Column("idlocalitate")]
            public int? IdLocalitate { get; set; }

            [Column("codsiruta")]
            public int? CodSiruta { get; set; }

            [Column("adresa")]
            public string Adresa { get; set; }

            [Column("telefon")]
            public string Telefon { get; set; }

            [Column("telefon_ubb")]
            public string TelefonUbb { get; set; }

            [Column("email")]
            public string Email { get; set; }

            [Column("email_ubb")]
            public string EmailUbb { get; set; }

            [Column("adresacorespondenta")]
            public string AdresaCorespondenta { get; set; }

            [Column("idtipautorizatiemunca")]
            public int? IdTipAutorizatieMunca { get; set; }

            [Column("datainceputautorizatie")]
            public DateTime? DataInceputAutorizatie { get; set; }

            [Column("datasfarsitautorizatie")]
            public DateTime? DataSfarsitAutorizatie { get; set; }

            [Column("sitmilitara")]
            public string SitMilitara { get; set; }

            [Column("serialivret")]
            public string SerieLivret { get; set; }

            [Column("numarlivret")]
            public string NumarLivret { get; set; }

            [Column("gradmilitar")]
            public string GradMilitar { get; set; }

            [Column("specialitatemilitara")]
            public string SpecialitateMilitara { get; set; }

            [Column("idcasadesanatate")]
            public int? IdCasaDeSanatate { get; set; }

            [Column("pensionar")]
            public bool? Pensionar { get; set; }

            [Column("idsistemasigurarisociale")]
            public int? IdSistemAsigurariSociale { get; set; }

            [Column("scutitcas")]
            public bool? ScutitCAS { get; set; }

            [Column("andocumentportabil")]
            public string AnDocumentPortabil { get; set; }

            [Column("dovadaasiguratue_see")]
            public string DovadaAsiguratUE_SEE { get; set; }

            [Column("idgradhandicap")]
            public int? IdGradHandicap { get; set; }

            [Column("idcaracterhandicap")]
            public int? IdCaracterHandicap { get; set; }

            [Column("datarevizuirehandicap")]
            public DateTime? DataRevizuireHandicap { get; set; }

            [Column("ancertificatrezidentafiscala")]
            public string AnCertificatRezidentaFiscala { get; set; }

            [Column("membrudesindicat")]
            public bool? MembruDeSindicat { get; set; }

            [Column("student")]
            public bool? Student { get; set; }

            [Column("idformastudii")]
            public int? IdFormaStudii { get; set; }

            [Column("valabilitateadeverinta")]
            public DateTime? ValabilitateAdeverinta { get; set; }

            [Column("functie_baza")]
            public bool? FunctieBaza { get; set; }

            [Column("institutiedebaza")]
            public string InstitutieDeBaza { get; set; }

            [Column("nerezident")]
            public bool? Nerezident { get; set; }

            [Column("gravida")]
            public bool? Gravida { get; set; }

            [Column("reduceretimplucrugravida")]
            public bool? ReducereTimpLucruGravida { get; set; }

            [Column("alapteaza")]
            public bool? Alapteaza { get; set; }

            [Column("reduceretimplucrualaptare")]
            public bool? ReducereTimpLucruAlaptare { get; set; }

            [Column("constituiredosarpersonal")]
            public string ConstituireDosarPersonal { get; set; }

            [Column("locatiearhivaredosarpersonal")]
            public string LocatieArhivareDosarPersonal { get; set; }

            [Column("locatiearhivaredocumentatie")]
            public string LocatieArhivareDocumentatie { get; set; }

            [Column("valabilitatedocumentatie")]
            public DateTime? ValabilitateDocumentatie { get; set; }

            [Column("observatiidocumentatie")]
            public string ObservatiiDocumentatie { get; set; }

            [Column("dataangajare")]
            public DateTime? DataAngajare { get; set; }

            [Column("observatii")]
            public string Observatii { get; set; }

            [Column("stare")]
            public string Stare { get; set; }

            [Column("datamodificare")]
            public DateTime? DataModificare { get; set; }

            [Column("preluat")]
            public bool? Preluat { get; set; }

            [Column("idgradatie")]
            public int? IdGradatie { get; set; }

            [Column("idtransavechevechimeinv")]
            public int? IdTransaVechimeInv { get; set; }

            [Column("idtransavechimeinv")]
            public int? IdTransaVechimeInv2 { get; set; }

            [Column("adresamod")]
            public string AdresaMod { get; set; }

            [Column("adresainitiala")]
            public string AdresaInitiala { get; set; }

            [Column("desincronizat")]
            public bool? DeSincronizat { get; set; }

            [Column("idgradinvaliditate")]
            public int? IdGradInvaliditate { get; set; }

            [Column("idtiphandicap")]
            public int? IdTipHandicap { get; set; }

            [Column("nrcertificathandicap")]
            public string NrCertificatHandicap { get; set; }

            [Column("datacertificathandicap")]
            public DateTime? DataCertificatHandicap { get; set; }

            [Column("nraviz")]
            public string NrAviz { get; set; }

            [Column("dataaviz")]
            public DateTime? DataAviz { get; set; }

            // Appears at the end of your list; keep as alias of the birthplace text if needed
            [Column("localitate_nastere")]
            public string LocalitateNastere { get; set; }
        }
    }
}
