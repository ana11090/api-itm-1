using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    public class ContractsRu
    {
        // PK
        /// <summary>idcontract (integer, NOT NULL)</summary>
        public int IdContract { get; set; }

        /// <summary>idfelcontract (integer)</summary>
        public int? ContractKindId { get; set; }

        /// <summary>idtipcontractplatacuora (integer)</summary>
        public int? HourlyPayContractTypeId { get; set; }

        /// <summary>idpersoana (integer)</summary>
        public int? PersonId { get; set; }

        /// <summary>numarcontract (varchar)</summary>
        public string? ContractNumber { get; set; }

        /// <summary>idtipcontract (integer)</summary>
        public int? ContractTypeId { get; set; }

        /// <summary>idtipdurata (integer)</summary>
        public int? DurationTypeId { get; set; }

        /// <summary>idtemeiduratadeterminata (integer)</summary>
        public int? FixedTermReasonId { get; set; }

        /// <summary>idtipnorma (integer)</summary>
        public int? WorkNormTypeId { get; set; }

        /// <summary>datacontract (date)</summary>
        public DateTime? ContractDate { get; set; }

        /// <summary>datainceput (date)</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>datasfarsit (date)</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>idexceptiedatasfarsit (integer)</summary>
        public int? EndDateExceptionId { get; set; }

        /// <summary>categorieprestari (varchar)</summary>
        public string? ServiceCategory { get; set; }

        /// <summary>garantiebunaexecutie (boolean)</summary>
        public bool? PerformanceGuarantee { get; set; }

        /// <summary>clauzadrepturiautor (boolean)</summary>
        public bool? AuthorsRightsClause { get; set; }

        /// <summary>functie_baza (boolean)</summary>
        public bool? IsBaseFunction { get; set; }

        /// <summary>seacordaco (boolean)</summary>
        public bool? IsVacationGranted { get; set; }

        /// <summary>perioadaproba (integer)</summary>
        public int? TrialPeriodDays { get; set; }

        /// <summary>detaliicontract (varchar)</summary>
        public string? ContractDetails { get; set; }

        /// <summary>obiectcontract (varchar)</summary>
        public string? ContractObject { get; set; }

        /// <summary>idcodiban (integer)</summary>
        public int? IBANCodeId { get; set; }

        /// <summary>idstarecontract (integer)</summary>
        public int? ContractStatusId { get; set; }

        /// <summary>idformaangajare (integer)</summary>
        public int? EmploymentFormId { get; set; }

        /// <summary>numardecizie (varchar)</summary>
        public string? DecisionNumber { get; set; }

        /// <summary>datadecizie (date)</summary>
        public DateTime? DecisionDate { get; set; }

        /// <summary>temeidecizie (varchar)</summary>
        public string? DecisionBasis { get; set; }

        /// <summary>numarcerereangajare (varchar)</summary>
        public string? EmploymentRequestNumber { get; set; }

        /// <summary>datacerereangajare (date)</summary>
        public DateTime? EmploymentRequestDate { get; set; }

        /// <summary>codunitate (integer)</summary>
        public int? UnitCode { get; set; }

        /// <summary>idanuniversitar (integer)</summary>
        public int? AcademicYearId { get; set; }

        /// <summary>semestru (varchar)</summary>
        public string? Semester { get; set; }

        /// <summary>idstatfunctii (integer)</summary>
        public int? FunctionStatId { get; set; }

        /// <summary>pozitie (integer)</summary>
        public int? PositionNumber { get; set; }

        /// <summary>declaratiedeaveresiinterese (boolean)</summary>
        public bool? AssetAndInterestDeclaration { get; set; }

        /// <summary>datadepuneredadi (date)</summary>
        public DateTime? AssetAndInterestDeclarationDate { get; set; }

        /// <summary>idocupatie (varchar)</summary>
        public string? OccupationCode { get; set; }

        /// <summary>idfunctieprestari (integer)</summary>
        public int? ServiceFunctionId { get; set; }

        /// <summary>adresacontract (varchar)</summary>
        public string? ContractAddress { get; set; }

        /// <summary>idnorma (integer)</summary>
        public int? NormId { get; set; }

        /// <summary>idrepartizaretimpmunca (integer)</summary>
        public int? WorkTimeAllocationId { get; set; }

        /// <summary>idintervaltimpmunca (integer)</summary>
        public int? WorkTimeIntervalId { get; set; }

        /// <summary>duratacontract (numeric)</summary>
        public decimal? ContractDuration { get; set; }

        /// <summary>norma112 (numeric)</summary>
        public decimal? Norm112 { get; set; }

        /// <summary>maximtimpmunca (boolean)</summary>
        public bool? IsMaxWorkTime { get; set; }

        /// <summary>idtemeilegal (integer)</summary>
        public int? LegalBasisId { get; set; }

        /// <summary>idtipcontracttemeilegal (integer)</summary>
        public int? ContractLegalTypeId { get; set; }

        /// <summary>salariuldeincadrare (numeric)</summary>
        public decimal? EmploymentSalary { get; set; }

        /// <summary>salariuldebaza (numeric)</summary>
        public decimal? BaseSalary { get; set; }

        /// <summary>tarifbrutorar (numeric)</summary>
        public decimal? GrossHourlyRate { get; set; }

        /// <summary>salariulbrut (numeric)</summary>
        public decimal? GrossSalary { get; set; }

        /// <summary>zileconcediu (integer)</summary>
        public int? VacationDays { get; set; }

        /// <summary>zileconcediusuplimentar (integer)</summary>
        public int? SupplementalVacationDays { get; set; }

        /// <summary>temeizileconcediusuplimentar (varchar)</summary>
        public string? SupplementalVacationDaysBasis { get; set; }

        /// <summary>existafisapost (boolean)</summary>
        public bool? HasJobDescription { get; set; }

        /// <summary>datafisapost (date)</summary>
        public DateTime? JobDescriptionDate { get; set; }

        /// <summary>locatiearhivarefisapost (varchar)</summary>
        public string? JobDescriptionArchiveLocation { get; set; }

        /// <summary>anexafisapost (boolean)</summary>
        public bool? JobDescriptionAnnex { get; set; }

        /// <summary>scutitdeimpozit (boolean)</summary>
        public bool? IncomeTaxExempt { get; set; }

        /// <summary>idtemeilegalimpozitpevenit (integer)</summary>
        public int? IncomeTaxLegalBasisId { get; set; }

        /// <summary>exceptietratamentfiscal (boolean)</summary>
        public bool? TaxTreatmentException { get; set; }

        /// <summary>impozitpevenit (boolean)</summary>
        public bool? IncomeTax { get; set; }

        /// <summary>bazadecalculimpozitpevenit (numeric)</summary>
        public decimal? IncomeTaxBase { get; set; }

        /// <summary>cas (boolean)</summary>
        public bool? SocialSecurity { get; set; }

        /// <summary>bazadecalculcas (numeric)</summary>
        public decimal? SocialSecurityBase { get; set; }

        /// <summary>cass (boolean)</summary>
        public bool? HealthInsurance { get; set; }

        /// <summary>bazadecalculcass (numeric)</summary>
        public decimal? HealthInsuranceBase { get; set; }

        /// <summary>contribasigptmunca (boolean)</summary>
        public bool? WorkInsuranceContribution { get; set; }

        /// <summary>bazadecalculcam (numeric)</summary>
        public decimal? WorkInsuranceBase { get; set; }

        /// <summary>gestionar (boolean)</summary>
        public bool? IsCustodian { get; set; }

        /// <summary>inasteptare (boolean)</summary>
        public bool? IsPending { get; set; }

        /// <summary>datamodificare (date)</summary>
        public DateTime? ModificationDate { get; set; }

        /// <summary>datasfarsitmodificare (date)</summary>
        public DateTime? ModificationEndDate { get; set; }

        /// <summary>obsdatasfarsitmodificare (varchar)</summary>
        public string? ModificationEndObservation { get; set; }

        /// <summary>idformamodificare (integer)</summary>
        public int? ModificationFormId { get; set; }

        /// <summary>numardeciziemodificare (varchar)</summary>
        public string? ModificationDecisionNumber { get; set; }

        /// <summary>datadeciziemodificare (date)</summary>
        public DateTime? ModificationDecisionDate { get; set; }

        /// <summary>temeilegalmodificare (varchar)</summary>
        public string? ModificationLegalBasis { get; set; }

        /// <summary>actaditional (boolean)</summary>
        public bool? AdditionalAct { get; set; }

        /// <summary>numaractaditional (integer)</summary>
        public int? AdditionalActNumber { get; set; }

        /// <summary>dataactaditional (date)</summary>
        public DateTime? AdditionalActDate { get; set; }

        /// <summary>idtemeimodificare (integer)</summary>
        public int? ModificationReasonId { get; set; }

        /// <summary>dataincetare (date)</summary>
        public DateTime? TerminationDate { get; set; }

        /// <summary>dataincetareconventie (date)</summary>
        public DateTime? TerminationAgreementDate { get; set; }

        /// <summary>idtemeilegalincetare (integer)</summary>
        public int? TerminationLegalBasisId { get; set; }

        /// <summary>idtemeilegalincetareprestari (integer)</summary>
        public int? TerminationLegalBasisServiceId { get; set; }

        /// <summary>numardecizieincetare (varchar)</summary>
        public string? TerminationDecisionNumber { get; set; }

        /// <summary>datadecizieincetare (date)</summary>
        public DateTime? TerminationDecisionDate { get; set; }

        /// <summary>numarcerereincetare (varchar)</summary>
        public string? TerminationRequestNumber { get; set; }

        /// <summary>datacerereincetare (date)</summary>
        public DateTime? TerminationRequestDate { get; set; }

        /// <summary>idtemeiincetare (integer)</summary>
        public int? TerminationReasonId { get; set; }

        /// <summary>idtemeilegalreactivare (integer)</summary>
        public int? ReactivationLegalBasisId { get; set; }

        /// <summary>documentreactivare (varchar)</summary>
        public string? ReactivationDocument { get; set; }

        /// <summary>clasa (integer)</summary>
        public int? JobClass { get; set; }

        /// <summary>datainceputgradatie (date)</summary>
        public DateTime? GradeStartDate { get; set; }

        /// <summary>preluat (boolean)</summary>
        public bool? IsImported { get; set; }

        /// <summary>dataconsemnare (date)</summary>
        public DateTime? RecordDate { get; set; }

        /// <summary>datatransmitererevisal (date)</summary>
        public DateTime? RevisalTransmitDate { get; set; }

        /// <summary>denumireangajatorcedent (varchar)</summary>
        public string? TransferEmployerName { get; set; }

        /// <summary>cuiangajatorcedent (varchar)</summary>
        public string? TransferEmployerCUI { get; set; }

        /// <summary>datatransfer (date)</summary>
        public DateTime? TransferDate { get; set; }

        /// <summary>idtiptransfer (integer)</summary>
        public int? TransferTypeId { get; set; }

        /// <summary>temeilegaltransfer (varchar)</summary>
        public string? TransferLegalBasis { get; set; }

        /// <summary>idtipmodificaresalar (integer)</summary>
        public int? SalaryModificationTypeId { get; set; }

        /// <summary>salariucalculorenoapte (numeric)</summary>
        public decimal? NightWorkSalaryCalculation { get; set; }

        /// <summary>sediu (varchar)</summary>
        public string? Headquarters { get; set; }

        /// <summary>scutitdeimpozitit (boolean)</summary>
        public bool? ITIncomeTaxExempt { get; set; }

        /// <summary>pilon2it (boolean)</summary>
        public bool? ITPillar2 { get; set; }

        /// <summary>idcategorieprestari (integer)</summary>
        public int? ServiceCategoryId { get; set; }

        /// <summary>ora_inceput (time)</summary>
        public TimeSpan? StartHour { get; set; }

        /// <summary>ora_sfarsit (time)</summary>
        public TimeSpan? EndHour { get; set; }

        /// <summary>idrepartizaremunca (integer)</summary>
        public int? WorkDistributionId { get; set; }

        public int? ShiftTypeId { get; set; } 
    }
}