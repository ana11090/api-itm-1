using api_itm.Data.Entity.Ru.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_itm.Data.Configurations
{
    public class ContractsRuConfiguration : IEntityTypeConfiguration<ContractsRu>
    {
        public void Configure(EntityTypeBuilder<ContractsRu> builder)
        {
            builder.ToTable("contracte_ru");

            // PK
            builder.HasKey(c => c.IdContract);
            builder.Property(c => c.IdContract).HasColumnName("idcontract");

            builder.Property(c => c.ContractKindId).HasColumnName("idfelcontract");
            builder.Property(c => c.HourlyPayContractTypeId).HasColumnName("idtipcontractplatacuora");
            builder.Property(c => c.PersonId).HasColumnName("idpersoana");
            builder.Property(c => c.ContractNumber).HasColumnName("numarcontract");
            builder.Property(c => c.ContractTypeId).HasColumnName("idtipcontract");
            builder.Property(c => c.DurationTypeId).HasColumnName("idtipdurata");
            builder.Property(c => c.FixedTermReasonId).HasColumnName("idtemeiduratadeterminata");
            builder.Property(c => c.WorkNormTypeId).HasColumnName("idtipnorma");

            builder.Property(c => c.ContractDate).HasColumnName("datacontract").HasColumnType("date");
            builder.Property(c => c.StartDate).HasColumnName("datainceput").HasColumnType("date");
            builder.Property(c => c.EndDate).HasColumnName("datasfarsit").HasColumnType("date");

            builder.Property(c => c.EndDateExceptionId).HasColumnName("idexceptiedatasfarsit");
            builder.Property(c => c.ServiceCategory).HasColumnName("categorieprestari");
            builder.Property(c => c.PerformanceGuarantee).HasColumnName("garantiebunaexecutie");
            builder.Property(c => c.AuthorsRightsClause).HasColumnName("clauzadrepturiautor");
            builder.Property(c => c.IsBaseFunction).HasColumnName("functie_baza");
            builder.Property(c => c.IsVacationGranted).HasColumnName("seacordaco");
            builder.Property(c => c.TrialPeriodDays).HasColumnName("perioadaproba");
            builder.Property(c => c.ContractDetails).HasColumnName("detaliicontract");
            builder.Property(c => c.ContractObject).HasColumnName("obiectcontract");
            builder.Property(c => c.IBANCodeId).HasColumnName("idcodiban");
            builder.Property(c => c.ContractStatusId).HasColumnName("idstarecontract");
            builder.Property(c => c.EmploymentFormId).HasColumnName("idformaangajare");
            builder.Property(c => c.DecisionNumber).HasColumnName("numardecizie");
            builder.Property(c => c.DecisionDate).HasColumnName("datadecizie").HasColumnType("date");
            builder.Property(c => c.DecisionBasis).HasColumnName("temeidecizie");
            builder.Property(c => c.EmploymentRequestNumber).HasColumnName("numarcerereangajare");
            builder.Property(c => c.EmploymentRequestDate).HasColumnName("datacerereangajare").HasColumnType("date");
            builder.Property(c => c.UnitCode).HasColumnName("codunitate");
            builder.Property(c => c.AcademicYearId).HasColumnName("idanuniversitar");
            builder.Property(c => c.Semester).HasColumnName("semestru");
            builder.Property(c => c.FunctionStatId).HasColumnName("idstatfunctii"); // FunctionStatId  
            builder.Property(c => c.PositionNumber).HasColumnName("pozitie");
            builder.Property(c => c.AssetAndInterestDeclaration).HasColumnName("declaratiedeaveresiinterese");
            builder.Property(c => c.AssetAndInterestDeclarationDate).HasColumnName("datadepuneredadi").HasColumnType("date");
            builder.Property(c => c.OccupationCode).HasColumnName("idocupatie");
            builder.Property(c => c.ServiceFunctionId).HasColumnName("idfunctieprestari");
            builder.Property(c => c.ContractAddress).HasColumnName("adresacontract");
            builder.Property(c => c.NormId).HasColumnName("idnorma");
            builder.Property(c => c.WorkTimeAllocationId).HasColumnName("idrepartizaretimpmunca");
            builder.Property(c => c.WorkTimeIntervalId).HasColumnName("idintervaltimpmunca");

            builder.Property(c => c.ContractDuration).HasColumnName("duratacontract").HasColumnType("numeric");
            builder.Property(c => c.Norm112).HasColumnName("norma112").HasColumnType("numeric");

            builder.Property(c => c.IsMaxWorkTime).HasColumnName("maximtimpmunca");
            builder.Property(c => c.LegalBasisId).HasColumnName("idtemeilegal");
            builder.Property(c => c.ContractLegalTypeId).HasColumnName("idtipcontracttemeilegal");

            builder.Property(c => c.EmploymentSalary).HasColumnName("salariuldeincadrare").HasColumnType("numeric");
            builder.Property(c => c.BaseSalary).HasColumnName("salariuldebaza").HasColumnType("numeric");
            builder.Property(c => c.GrossHourlyRate).HasColumnName("tarifbrutorar").HasColumnType("numeric");
            builder.Property(c => c.GrossSalary).HasColumnName("salariulbrut").HasColumnType("numeric");

            builder.Property(c => c.VacationDays).HasColumnName("zileconcediu");
            builder.Property(c => c.SupplementalVacationDays).HasColumnName("zileconcediusuplimentar");
            builder.Property(c => c.SupplementalVacationDaysBasis).HasColumnName("temeizileconcediusuplimentar");

            builder.Property(c => c.HasJobDescription).HasColumnName("existafisapost");
            builder.Property(c => c.JobDescriptionDate).HasColumnName("datafisapost").HasColumnType("date");
            builder.Property(c => c.JobDescriptionArchiveLocation).HasColumnName("locatiearhivarefisapost");
            builder.Property(c => c.JobDescriptionAnnex).HasColumnName("anexafisapost");

            builder.Property(c => c.IncomeTaxExempt).HasColumnName("scutitdeimpozit");
            builder.Property(c => c.IncomeTaxLegalBasisId).HasColumnName("idtemeilegalimpozitpevenit");
            builder.Property(c => c.TaxTreatmentException).HasColumnName("exceptietratamentfiscal");
            builder.Property(c => c.IncomeTax).HasColumnName("impozitpevenit");
            builder.Property(c => c.IncomeTaxBase).HasColumnName("bazadecalculimpozitpevenit").HasColumnType("numeric");

            builder.Property(c => c.SocialSecurity).HasColumnName("cas");
            builder.Property(c => c.SocialSecurityBase).HasColumnName("bazadecalculcas").HasColumnType("numeric");
            builder.Property(c => c.HealthInsurance).HasColumnName("cass");
            builder.Property(c => c.HealthInsuranceBase).HasColumnName("bazadecalculcass").HasColumnType("numeric");
            builder.Property(c => c.WorkInsuranceContribution).HasColumnName("contribasigptmunca");
            builder.Property(c => c.WorkInsuranceBase).HasColumnName("bazadecalculcam").HasColumnType("numeric");

            builder.Property(c => c.IsCustodian).HasColumnName("gestionar");
            builder.Property(c => c.IsPending).HasColumnName("inasteptare");

            builder.Property(c => c.ModificationDate).HasColumnName("datamodificare").HasColumnType("date");
            builder.Property(c => c.ModificationEndDate).HasColumnName("datasfarsitmodificare").HasColumnType("date");
            builder.Property(c => c.ModificationEndObservation).HasColumnName("obsdatasfarsitmodificare");
            builder.Property(c => c.ModificationFormId).HasColumnName("idformamodificare");
            builder.Property(c => c.ModificationDecisionNumber).HasColumnName("numardeciziemodificare");
            builder.Property(c => c.ModificationDecisionDate).HasColumnName("datadeciziemodificare").HasColumnType("date");
            builder.Property(c => c.ModificationLegalBasis).HasColumnName("temeilegalmodificare");

            builder.Property(c => c.AdditionalAct).HasColumnName("actaditional");
            builder.Property(c => c.AdditionalActNumber).HasColumnName("numaractaditional");
            builder.Property(c => c.AdditionalActDate).HasColumnName("dataactaditional").HasColumnType("date");
            builder.Property(c => c.ModificationReasonId).HasColumnName("idtemeimodificare");

            builder.Property(c => c.TerminationDate).HasColumnName("dataincetare").HasColumnType("date");
            builder.Property(c => c.TerminationAgreementDate).HasColumnName("dataincetareconventie").HasColumnType("date");
            builder.Property(c => c.TerminationLegalBasisId).HasColumnName("idtemeilegalincetare");
            builder.Property(c => c.TerminationLegalBasisServiceId).HasColumnName("idtemeilegalincetareprestari");
            builder.Property(c => c.TerminationDecisionNumber).HasColumnName("numardecizieincetare");
            builder.Property(c => c.TerminationDecisionDate).HasColumnName("datadecizieincetare").HasColumnType("date");
            builder.Property(c => c.TerminationRequestNumber).HasColumnName("numarcerereincetare");
            builder.Property(c => c.TerminationRequestDate).HasColumnName("datacerereincetare").HasColumnType("date");
            builder.Property(c => c.TerminationReasonId).HasColumnName("idtemeiincetare");

            builder.Property(c => c.ReactivationLegalBasisId).HasColumnName("idtemeilegalreactivare");
            builder.Property(c => c.ReactivationDocument).HasColumnName("documentreactivare");

            builder.Property(c => c.JobClass).HasColumnName("clasa");
            builder.Property(c => c.GradeStartDate).HasColumnName("datainceputgradatie").HasColumnType("date");
            builder.Property(c => c.IsImported).HasColumnName("preluat");

            builder.Property(c => c.RecordDate).HasColumnName("dataconsemnare").HasColumnType("date");
            builder.Property(c => c.RevisalTransmitDate).HasColumnName("datatransmitererevisal").HasColumnType("date");

            builder.Property(c => c.TransferEmployerName).HasColumnName("denumireangajatorcedent");
            builder.Property(c => c.TransferEmployerCUI).HasColumnName("cuiangajatorcedent");
            builder.Property(c => c.TransferDate).HasColumnName("datatransfer").HasColumnType("date");
            builder.Property(c => c.TransferTypeId).HasColumnName("idtiptransfer");
            builder.Property(c => c.TransferLegalBasis).HasColumnName("temeilegaltransfer");

            builder.Property(c => c.SalaryModificationTypeId).HasColumnName("idtipmodificaresalar");
            builder.Property(c => c.NightWorkSalaryCalculation).HasColumnName("salariucalculorenoapte").HasColumnType("numeric");
            builder.Property(c => c.Headquarters).HasColumnName("sediu");
            builder.Property(c => c.ITIncomeTaxExempt).HasColumnName("scutitdeimpozitit");
            builder.Property(c => c.ITPillar2).HasColumnName("pilon2it");
            builder.Property(c => c.ServiceCategoryId).HasColumnName("idcategorieprestari");

            builder.Property(c => c.StartHour).HasColumnName("ora_inceput").HasColumnType("time without time zone");
            builder.Property(c => c.EndHour).HasColumnName("ora_sfarsit").HasColumnType("time without time zone");

            builder.Property(c => c.WorkDistributionId).HasColumnName("idrepartizaremunca");
            builder.Property(c => c.ShiftTypeId).HasColumnName("idtiptura");

        }
    }
}
