using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api_itm.Data.Entity
{
    [Table("salarizare_contracte_ru")]
    public class SalaryContractRu
    {
        [Key]
        [Column("idcontractsalarizare")]
        public int SalaryContractId { get; set; }

        [Column("idcontract")]
        public int? ContractId { get; set; }

        [Column("luna")]
        public int? Month { get; set; }

        [Column("anul")]
        public int? Year { get; set; }

        [Column("datai")]
        public DateTime? EntryDate { get; set; }

        [Column("datas")]
        public DateTime? ExitDate { get; set; }

        [Column("numarcontract")]
        public string? ContractNumber { get; set; }

        [Column("datacontract")]
        public DateTime? ContractDate { get; set; }

        [Column("idpersoana")]
        public int? PersonId { get; set; }

        [Column("idtipcontract")]
        public int? ContractTypeId { get; set; }

        [Column("idstatfunctii")]
        public int? FunctionStatId { get; set; }

        [Column("datainceput")]
        public DateTime? StartDate { get; set; }

        [Column("datasfarsit")]
        public DateTime? EndDate { get; set; }

        [Column("salariuldeincadrare")]
        public decimal? EmploymentSalary { get; set; }

        [Column("perioadaproba")]
        public int? ProbationPeriod { get; set; }

        [Column("zileconcediu")]
        public int? VacationDays { get; set; }

        [Column("zileconcediusuplimentar")]
        public int? AdditionalVacationDays { get; set; }

        [Column("idocupatie")]
        public int? OccupationId { get; set; }

        [Column("functie_baza")]
        public bool? IsBaseFunction { get; set; }

        [Column("idtipnorma")]
        public int? NormTypeId { get; set; }

        [Column("idfelcontract")]
        public int? ContractKindId { get; set; }

        [Column("idnorma")]
        public int? NormId { get; set; }

        [Column("idrepartizaretimpmunca")]
        public int? WorkDistributionId { get; set; }

        [Column("idintervaltimpmunca")]
        public int? WorkTimeIntervalId { get; set; }

        [Column("duratacontract")]
        public int? ContractDuration { get; set; }

        [Column("norma112")]
        public decimal? Norm112 { get; set; }

        [Column("salariuldebaza")]
        public decimal? BaseSalary { get; set; }

        [Column("tarifbrutorar")]
        public decimal? GrossHourlyRate { get; set; }

        [Column("salariulbrut")]
        public decimal? GrossSalary { get; set; }

        [Column("codunitate")]
        public string? UnitCode { get; set; }

        [Column("dataincetare")]
        public DateTime? TerminationDate { get; set; }

        [Column("dataincetareconventie")]
        public DateTime? ConventionTerminationDate { get; set; }

        [Column("obiectcontract")]
        public string? ContractObject { get; set; }

        [Column("maximtimpmunca")]
        public int? MaxWorkTime { get; set; }

        [Column("salariucalculorenoapte")]
        public decimal? NightWorkSalaryCalculation { get; set; }

        [Column("idtipcontractplatacuora")]
        public int? HourlyPaidContractTypeId { get; set; }

        [Column("idtemeilegal")]
        public int? LegalBasisId { get; set; }
    }
}
