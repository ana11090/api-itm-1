using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_itm.Data.Entity.Ru
{
    [Table("persoana")]
    public class Person
    {
        [Key]
        [Column("idpersoana")]
        public int PersonId { get; set; }                             // integer, NOT NULL

        [Column("cnp")]
        public string? NationalId { get; set; }                       // character, NULL

        [Column("cnp_vechi")]
        public string? OldNationalId { get; set; }                    // character, NULL

        [Column("nume")]
        public string? LastName { get; set; }                         // varchar, NULL

        [Column("prenume")]
        public string? FirstName { get; set; }                        // varchar, NULL

        [Column("titlu")]
        public string? Title { get; set; }                            // varchar, NULL

        [Column("marcapersoana")]
        public string? EmployeeCode { get; set; }                     // varchar, NULL

        [Column("profesie")]
        public string? Occupation { get; set; }                       // varchar, NULL

        [Column("sex")]
        public string? Sex { get; set; }                              // character, NULL

        [Column("datanasterii")]
        public DateTime? BirthDate { get; set; }                      // date, NULL

        [Column("loculnasterii")]
        public string? BirthPlace { get; set; }                       // varchar, NULL

        [Column("starecivila")]
        public string? MaritalStatus { get; set; }                    // varchar, NULL

        [Column("idtara")]
        public int? CountryId { get; set; }                           // integer, NULL

        [Column("cetatenie")]
        public string Citizenship { get; set; } = "";                 // varchar, NOT NULL

        [Column("idtipnationalitate")]
        public int? NationalityTypeId { get; set; }                   // integer, NULL

        [Column("idtipapatrid")]
        public int? IdTipApatrid { get; set; }                     // integer, NULL

        [Column("statutspecial")]
        public string? SpecialStatus { get; set; }                    // varchar, NULL

        [Column("idtipact")]
        public int? IdentityDocTypeId { get; set; }                   // integer, NULL

        [Column("seriaactidentitate")]
        public string? IdentitySeries { get; set; }                   // varchar, NULL

        [Column("numaractidentitate")]
        public string? IdentityNumber { get; set; }                   // varchar, NULL

        [Column("organemitentactidentitate")]
        public string? IdentityIssuer { get; set; }                   // varchar, NULL

        [Column("dataeliberareactidentitate")]
        public DateTime? IdentityIssueDate { get; set; }              // date, NULL

        [Column("valabilitateactidentitate")]
        public DateTime? IdentityExpiryDate { get; set; }             // date, NULL

        [Column("scoppermisdesedere")]
        public string? ResidencePermitPurpose { get; set; }           // varchar, NULL

        [Column("idtaradomiciliu")]
        public int? DomicileCountryId { get; set; }                   // integer, NULL

        [Column("idjudet")]
        public int? CountyId { get; set; }                            // integer, NULL

        [Column("idlocalitate")]
        public int? CityId { get; set; }                              // integer, NULL

        [Column("codsiruta")]
        public int? SirutaCode { get; set; }                          // integer, NULL

        [Column("adresa")]
        public string? Address { get; set; }                          // varchar, NULL

        [Column("telefon")]
        public string? Phone { get; set; }                            // varchar, NULL

        [Column("telefon_ubb")]
        public string? PhoneUbb { get; set; }                         // varchar, NULL

        [Column("email")]
        public string? Email { get; set; }                            // varchar, NULL

        [Column("email_ubb")]
        public string? EmailUbb { get; set; }                         // varchar, NULL

        [Column("adresacorespondenta")]
        public string? CorrespondenceAddress { get; set; }            // varchar, NULL

        [Column("idtipautorizatiemunca")]
        public int? WorkPermitTypeId { get; set; }                    // integer, NULL

        [Column("datainceputautorizatie")]
        public DateTime? WorkPermitStartDate { get; set; }            // date, NULL

        [Column("datasfarsitautorizatie")]
        public DateTime? WorkPermitEndDate { get; set; }              // date, NULL

        [Column("sitmilitara")]
        public string? MilitaryStatus { get; set; }                   // varchar, NULL

        [Column("serialivret")]
        public string? MilitaryBookSeries { get; set; }               // varchar, NULL

        [Column("numarlivret")]
        public string? MilitaryBookNumber { get; set; }               // varchar, NULL

        [Column("gradmilitar")]
        public string? MilitaryRank { get; set; }                     // varchar, NULL

        [Column("specialitatemilitara")]
        public string? MilitarySpecialty { get; set; }                // varchar, NULL

        [Column("idcasadesanatate")]
        public int? HealthInsuranceHouseId { get; set; }              // integer, NULL

        [Column("pensionar")]
        public bool? Retired { get; set; }                            // boolean, NULL

        [Column("idsistemasigurarisociale")]
        public int? SocialInsuranceSystemId { get; set; }             // integer, NULL

        [Column("scutitcas")]
        public bool? CasExempt { get; set; }                          // boolean, NULL

        [Column("andocumentportabil")]
        public decimal? PortableDocYear { get; set; }                 // numeric, NULL

        [Column("dovadaasiguratue_see")]
        public bool? EuSeeInsuredProof { get; set; }                  // boolean, NULL

        [Column("idgradhandicap")]
        public int? DisabilityGradeId { get; set; }                   // integer, NULL

        [Column("idcaracterhandicap")]
        public int? DisabilityCharacterId { get; set; }               // integer, NULL

        [Column("datarevizuirehandicap")]
        public DateTime? DisabilityReviewDate { get; set; }           // date, NULL

        [Column("ancertificatrezidentafiscala")]
        public decimal? TaxResidenceCertificateYear { get; set; }     // numeric, NULL

        [Column("membrudesindicat")]
        public bool? UnionMember { get; set; }                        // boolean, NULL

        [Column("student")]
        public bool? Student { get; set; }                            // boolean, NULL

        [Column("idformastudii")]
        public int? StudyFormId { get; set; }                         // integer, NULL

        [Column("valabilitateadeverinta")]
        public DateTime? CertificateValidity { get; set; }            // date, NULL

        [Column("functie_baza")]
        public bool? BaseFunction { get; set; }                       // boolean, NULL

        [Column("institutiedebaza")]
        public string? BaseInstitution { get; set; }                  // varchar, NULL

        [Column("nerezident")]
        public bool? NonResident { get; set; }                        // boolean, NULL

        [Column("gravida")]
        public bool? Pregnant { get; set; }                           // boolean, NULL

        [Column("reduceretimplucrugravida")]
        public bool? ReducedWorkTimePregnancy { get; set; }           // boolean, NULL

        [Column("alapteaza")]
        public bool? Breastfeeding { get; set; }                      // boolean, NULL

        [Column("reduceretimplucrualaptare")]
        public bool? ReducedWorkTimeBreastfeeding { get; set; }       // boolean, NULL

        [Column("constituiredosarpersonal")]
        public bool? PersonalFileSetup { get; set; }                  // boolean, NULL

        [Column("locatiearhivaredosarpersonal")]
        public string? PersonalFileArchiveLocation { get; set; }      // character, NULL

        [Column("locatiearhivaredocumentatie")]
        public string? DocumentationArchiveLocation { get; set; }     // character, NULL

        [Column("valabilitatedocumentatie")]
        public DateTime? DocumentationValidity { get; set; }          // date, NULL

        [Column("observatiidocumentatie")]
        public string? DocumentationNotes { get; set; }               // varchar, NULL

        [Column("dataangajare")]
        public DateTime? HireDate { get; set; }                       // date, NULL

        [Column("observatii")]
        public string? Notes { get; set; }                            // varchar, NULL

        [Column("stare")]
        public string? Status { get; set; }                           // character, NULL

        [Column("datamodificare")]
        public DateTime? ModifiedAt { get; set; }                     // date, NULL

        [Column("preluat")]
        public bool? Imported { get; set; }                           // boolean, NULL

        [Column("idgradatie")]
        public int? SeniorityGradeId { get; set; }                    // integer, NULL

        [Column("idtransavechevechimeinv")]
        public int? SeniorityBandOldId { get; set; }                  // integer, NULL

        [Column("idtransavechimeinv")]
        public int? SeniorityBandId { get; set; }                     // integer, NULL

        [Column("adresamod")]
        public string? AddressModified { get; set; }                  // varchar, NULL

        [Column("adresainitiala")]
        public string? InitialAddress { get; set; }                   // varchar, NULL

        [Column("desincronizat")]
        public int? Desynchronized { get; set; }                      // integer, NULL

        [Column("idgradinvaliditate")]
        public int? InvalidityGradeId { get; set; }                   // integer, NULL

        [Column("idtiphandicap")]
        public int? HandicapTypeId { get; set; }                      // integer, NULL

        [Column("nrcertificathandicap")]
        public string? HandicapCertificateNumber { get; set; }        // varchar, NULL

        [Column("datacertificathandicap")]
        public DateTime? HandicapCertificateDate { get; set; }        // date, NULL

        [Column("nraviz")]
        public string? ApprovalNumber { get; set; }                   // varchar, NULL

        [Column("dataaviz")]
        public DateTime? ApprovalDate { get; set; }                   // date, NULL

        [Column("localitate_nastere")]
        public string? BirthLocality { get; set; }                    // varchar, NULL

        [Column("reges_sincronizare")]
        public int? RegesSyncVariable { get; set; }                    // varchar, NULL
    }
}
