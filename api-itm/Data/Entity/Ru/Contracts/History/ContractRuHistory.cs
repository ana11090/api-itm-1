using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.History
{

    [Table("istoric_contracte_ru", Schema = "ru")]
    public class ContractRuHistory
    {
        [Key]
        [Column("idistoriccontracte_ru")]
        public int ContractHistoryRuId { get; set; }

        [Column("dataoperare")] public DateTime? LoggedAt { get; set; }
        [Column("idpersoanamod")] public int? ModifiedByPersonId { get; set; }
        [Column("numepersoanamod")] public string? ModifiedByPersonName { get; set; }
        [Column("operatie")] public string? Operation { get; set; }
        [Column("descriere")] public string? Description { get; set; }
        [Column("dataoperatie")] public DateTime? OperationAt { get; set; }
        [Column("actoperatie")] public string? OperationDocument { get; set; }

        [Column("idcontract")] public int? ContractId { get; set; }
        [Column("idfelcontract")] public int? ContractKindId { get; set; }
        [Column("denumirefelcontract")] public string? ContractKindName { get; set; }
        [Column("idtipcontractplatacuora")] public int? HourlyContractTypeId { get; set; }
        [Column("denumiretipcontractplatacuora")] public string? HourlyContractTypeName { get; set; }

        [Column("idpersoana")] public int? PersonId { get; set; }
        [Column("numeprenume")] public string? FullName { get; set; }
        [Column("cnp")] public string? NationalId { get; set; }

        [Column("numarcontract")] public string? ContractNumber { get; set; }
        [Column("idtipcontract")] public int? ContractTypeId { get; set; }
        [Column("denumiretipcontract")] public string? ContractTypeName { get; set; }

        [Column("idtipdurata")] public int? DurationTypeId { get; set; }
        [Column("denumiretipdurata")] public string? DurationTypeName { get; set; }
        [Column("idtemeiduratadeterminata")] public int? FixedTermGroundId { get; set; }
        [Column("denumiretemeiduratadeterminata")] public string? FixedTermGroundName { get; set; }
        [Column("articolduratadeterminata")] public string? FixedTermArticle { get; set; }

        [Column("idtipnorma")] public int? WorkNormTypeId { get; set; }
        [Column("denumiretipnorma")] public string? WorkNormTypeName { get; set; }

        [Column("datacontract")] public DateTime? ContractDate { get; set; }
        [Column("datainceput")] public DateTime? StartDate { get; set; }
        [Column("datasfarsit")] public DateTime? EndDate { get; set; }
        [Column("idexceptiedatasfarsit")] public int? EndDateExceptionId { get; set; }
        [Column("denumireexceptiedatasfarsit")] public string? EndDateExceptionName { get; set; }

        [Column("categorieprestari")] public string? ServiceCategory { get; set; }
        [Column("garantiebunaexecutie")] public bool? PerformanceGuarantee { get; set; }
        [Column("clauzadrepturiautor")] public bool? CopyrightClause { get; set; }
        [Column("functie_baza")] public bool? PrimaryPosition { get; set; }
        [Column("seacordaco")] public bool? AnnualLeaveGranted { get; set; }
        [Column("perioadaproba")] public int? ProbationPeriod { get; set; }

        [Column("detaliicontract")] public string? ContractDetails { get; set; }
        [Column("obiectcontract")] public string? ContractSubject { get; set; }

        [Column("idcodiban")] public int? BankCodeId { get; set; }
        [Column("denumirecodiban")] public string? BankCodeName { get; set; }

        [Column("idstarecontract")] public int? ContractStatusId { get; set; }
        [Column("denumirestarecontract")] public string? ContractStatusName { get; set; }

        [Column("idformaangajare")] public int? EmploymentFormId { get; set; }
        [Column("denumireformaangajare")] public string? EmploymentFormName { get; set; }

        [Column("numardecizie")] public string? DecisionNumber { get; set; }
        [Column("datadecizie")] public DateTime? DecisionDate { get; set; }
        [Column("temeidecizie")] public string? DecisionGround { get; set; }

        [Column("numarcerereangajare")] public string? HiringRequestNumber { get; set; }
        [Column("datacerereangajare")] public DateTime? HiringRequestDate { get; set; }

        [Column("codunitate")] public string? UnitCode { get; set; }
        [Column("denumireunitate")] public string? UnitName { get; set; }

        [Column("idanuniversitar")] public int? AcademicYearId { get; set; }
        [Column("denumireanuniversitar")] public string? AcademicYearName { get; set; }
        [Column("semestru")] public int? Semester { get; set; }

        [Column("idstatfunctii")] public int? FunctionStatId { get; set; }
        [Column("pozitie")] public string? Position { get; set; }

        [Column("declaratiedeaveresiinterese")] public bool? AssetAndInterestDeclaration { get; set; }
        [Column("datadepuneredadi")] public DateTime? DadiSubmissionDate { get; set; }

        [Column("idocupatie")] public int? OccupationId { get; set; }
        [Column("denumireocupatie")] public string? OccupationName { get; set; }

        [Column("idfunctieprestari")] public int? ServicesFunctionId { get; set; }
        [Column("denumirefunctieprestari")] public string? ServicesFunctionName { get; set; }

        [Column("adresacontract")] public string? ContractAddress { get; set; }

        [Column("idnorma")] public int? NormId { get; set; }
        [Column("denumirenorma")] public string? NormName { get; set; }
        [Column("codnorma")] public string? NormCode { get; set; }

        [Column("idrepartizaretimpmunca")] public int? WorkTimeAllocationId { get; set; }
        [Column("denumirerepartizaretimpmunca")] public string? WorkTimeAllocationName { get; set; }
        [Column("idintervaltimpmunca")] public int? WorkingTimeIntervalId { get; set; }
        [Column("denumireintervaltimpmunca")] public string? WorkingTimeIntervalName { get; set; }

        [Column("duratacontract")] public int? ContractDuration { get; set; }
        [Column("maximtimpmunca")] public int? MaxWorkingTime { get; set; }
        [Column("norma112")] public bool? Norm112 { get; set; }

        [Column("idtemeilegal")] public int? LegalGroundId { get; set; }
        [Column("denumiretemeilegal")] public string? LegalGroundName { get; set; }
        [Column("idtipcontracttemeilegal")] public int? LegalGroundContractTypeId { get; set; }
        [Column("denumiretipcontracttemeilegal")] public string? LegalGroundContractTypeName { get; set; }

        [Column("salariuldeincadrare")] public decimal? HiringBaseSalary { get; set; }
        [Column("salariuldebaza")] public decimal? BaseSalary { get; set; }
        [Column("tarifbrutorar")] public decimal? GrossHourlyRate { get; set; }
        [Column("salariulbrut")] public decimal? GrossSalary { get; set; }

        [Column("zileconcediu")] public int? VacationDays { get; set; }
        [Column("zileconcediusuplimentar")] public int? AdditionalVacationDays { get; set; }
        [Column("temeizileconcediusuplimentar")] public string? AdditionalVacationDaysGround { get; set; }

        [Column("existafisapost")] public bool? HasJobDescription { get; set; }
        [Column("datafisapost")] public DateTime? JobDescriptionDate { get; set; }
        [Column("locatiearhivarefisapost")] public string? JobDescriptionArchiveLocation { get; set; }
        [Column("anexafisapost")] public string? JobDescriptionAnnex { get; set; }

        [Column("scutitdeimpozit")] public bool? IncomeTaxExempt { get; set; }
        [Column("idtemeilegalimpozitpevenit")] public int? IncomeTaxLegalGroundId { get; set; }
        [Column("denumiretemeilegalimpozitpevenit")] public string? IncomeTaxLegalGroundName { get; set; }
        [Column("exceptietratamentfiscal")] public string? FiscalTreatmentException { get; set; }
        [Column("impozitpevenit")] public decimal? IncomeTax { get; set; }
        [Column("bazadecalculimpozitpevenit")] public decimal? IncomeTaxBase { get; set; }

        [Column("cas")] public decimal? PensionContribution { get; set; }
        [Column("bazadecalculcas")] public decimal? PensionBase { get; set; }
        [Column("cass")] public decimal? HealthContribution { get; set; }
        [Column("bazadecalculcass")] public decimal? HealthBase { get; set; }
        [Column("contribasigptmunca")] public decimal? LaborInsuranceContribution { get; set; }
        [Column("bazadecalculcam")] public decimal? LaborInsuranceBase { get; set; }

        [Column("gestionar")] public bool? Storekeeper { get; set; }
        [Column("inasteptare")] public bool? Pending { get; set; }
        [Column("datasfarsitsuspendare")] public DateTime? SuspensionEndDate { get; set; }
        [Column("temeilegalmodificare")] public string? ModificationLegalGround { get; set; }

        [Column("dataincetare")] public DateTime? TerminationDate { get; set; }
        [Column("idtemeilegalincetare")] public int? TerminationLegalGroundId { get; set; }
        [Column("denumiretemeilegalincetare")] public string? TerminationLegalGroundName { get; set; }
        [Column("idtemeilegalreactivare")] public int? ReactivationLegalGroundId { get; set; }
        [Column("denumiretemeilegalreactivare")] public string? ReactivationLegalGroundName { get; set; }
        [Column("documentreactivare")] public string? ReactivationDocument { get; set; }

        [Column("idtemeilegalincetareprestari")] public int? ServicesTerminationLegalGroundId { get; set; }
        [Column("denumiretemeilegalincetareprestari")] public string? ServicesTerminationLegalGroundName { get; set; }
        [Column("dataincetareconventie")] public DateTime? ConventionEndDate { get; set; }

        [Column("datamodificare")] public DateTime? ModificationDate { get; set; }
        [Column("datasfarsitmodificare")] public DateTime? ModificationEndDate { get; set; }
        [Column("obsdatasfarsitmodificare")] public string? ModificationEndDateNote { get; set; }
        [Column("idformamodificare")] public int? ModificationFormId { get; set; }
        [Column("denumireformamodificare")] public string? ModificationFormName { get; set; }
        [Column("numardeciziemodificare")] public string? ModificationDecisionNumber { get; set; }
        [Column("datadeciziemodificare")] public DateTime? ModificationDecisionDate { get; set; }

        [Column("actaditional")] public bool? AdditionalAct { get; set; }
        [Column("numaractaditional")] public string? AdditionalActNumber { get; set; }
        [Column("dataactaditional")] public DateTime? AdditionalActDate { get; set; }
        [Column("idtemeimodificare")] public int? ModificationGroundId { get; set; }
        [Column("denumiretemeimodificare")] public string? ModificationGroundName { get; set; }

        [Column("numardecizieincetare")] public string? TerminationDecisionNumber { get; set; }
        [Column("datadecizieincetare")] public DateTime? TerminationDecisionDate { get; set; }
        [Column("numarcerereincetare")] public string? TerminationRequestNumber { get; set; }
        [Column("datacerereincetare")] public DateTime? TerminationRequestDate { get; set; }
        [Column("idtemeiincetare")] public int? TerminationGroundId { get; set; }
        [Column("denumiretemeiincetare")] public string? TerminationGroundName { get; set; }

        [Column("datainceputgradatie")] public DateTime? StepStartDate { get; set; }
        [Column("clasa")] public string? Class { get; set; }
        [Column("preluat")] public bool? Imported { get; set; }
        [Column("dataconsemnare")] public DateTime? RecordingDate { get; set; }
        [Column("datatransmitererevisal")] public DateTime? RevisalSentAt { get; set; }

        [Column("denumireangajatorcedent")] public string? TransferorEmployerName { get; set; }
        [Column("cuiangajatorcedent")] public string? TransferorEmployerTaxId { get; set; }
        [Column("datatransfer")] public DateTime? TransferDate { get; set; }
        [Column("idtiptransfer")] public int? TransferTypeId { get; set; }
        [Column("temeilegaltransfer")] public string? TransferLegalGround { get; set; }
        [Column("denumiretiptransfer")] public string? TransferTypeName { get; set; }

        [Column("salariuldebaza_vechi")] public decimal? BaseSalaryOld { get; set; }
        [Column("salariulbrut_vechi")] public decimal? GrossSalaryOld { get; set; }
        [Column("idtipmodificaresalar")] public int? SalaryChangeTypeId { get; set; }
        [Column("denumiretipmodificaresalar")] public string? SalaryChangeTypeName { get; set; }
        [Column("anulat")] public bool? Cancelled { get; set; }
        [Column("salariucalculorenoapte")] public decimal? NightWorkPay { get; set; }
        [Column("sediu")] public string? Headquarters { get; set; }
        [Column("scutitdeimpozitit")] public bool? ITTaxExempt { get; set; }
        [Column("pilon2it")] public bool? Pillar2IT { get; set; }

        [Column("idcategorieprestari")] public int? ServicesCategoryId { get; set; }
        [Column("denumirecategorieprestari")] public string? ServicesCategoryName { get; set; }

        [Column("ora_inceput")] public TimeSpan? StartTime { get; set; }
        [Column("ora_sfarsit")] public TimeSpan? EndTime { get; set; }

        [Column("idrepartizaremunca")] public int? WorkDistributionId { get; set; }
        [Column("denumirerepartizaremunca")] public string? WorkDistributionName { get; set; }

        [Column("idtiptura")] public int? ShiftTypeId { get; set; }
        [Column("denumiretiptura")] public string? ShiftTypeName { get; set; }

        [Column("idtiplocmunca")] public int? WorkLocationTypeId { get; set; }
        [Column("denumiretiplocmunca")] public string? WorkLocationTypeName { get; set; }

        [Column("idjudet")] public int? CountyId { get; set; }
        [Column("numejudet")] public string? CountyName { get; set; }
        [Column("idlocalitate")] public int? CityId { get; set; }
        [Column("denumirelocalitate")] public string? CityName { get; set; }
    }
}