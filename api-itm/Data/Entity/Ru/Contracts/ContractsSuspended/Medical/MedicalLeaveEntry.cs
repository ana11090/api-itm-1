using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.ContractsSuspended.Medical
{
    [Table("pontaj_concediimedicale", Schema = "ru")]
    public class MedicalLeaveEntry
    {
        [Key]
        [Column("idpontajconcediumedical")]
        public int MedicalLeaveEntryId { get; set; }                  // integer, NOT NULL

        [Column("idcontract")]
        public int? ContractId { get; set; }                          // integer, NULL

        [Column("idcontractsalarizare")]
        public int? ContractPayrollId { get; set; }                   // integer, NULL

        [Column("idpersoana")]
        public int? PersonId { get; set; }                            // integer, NULL

        [Column("tipconcediumedical")]
        public string? MedicalLeaveType { get; set; }                 // varchar, NULL (e.g., "I")

        //[Column("idconcediumedicalinitial")]
        //public int? InitialMedicalLeaveId { get; set; }               // integer, NULL

        //[Column("idconcediumedicalanterior")]
        //public int? PreviousMedicalLeaveId { get; set; }              // integer, NULL

        //[Column("codindemnizatie")]
        //public string? IndemnityCode { get; set; }                    // varchar, NULL (e.g., "03")

        //[Column("codbiumg")]
        //public string? BiuMgCode { get; set; }                        // varchar, NULL

        //[Column("serie")]
        //public string? Series { get; set; }                           // varchar, NULL (e.g., "ABC")

        //[Column("numar")]
        //public string? Number { get; set; }                           // varchar, NULL (e.g., "111")

        //[Column("coddiagnostic")]
        //public string? DiagnosticCode { get; set; }                   // varchar, NULL (e.g., "HS32")

        //[Column("unitatemedicalaemitenta")]
        //public string? IssuingMedicalUnit { get; set; }               // varchar, NULL

        //[Column("dataacordarii")]
        //public DateTime? GrantedDate { get; set; }                    // date, NULL

        [Column("datainceput")]
        public DateTime? StartDate { get; set; }                      // date, NULL

        [Column("datasfarsit")]
        public DateTime? EndDate { get; set; }                        // date, NULL

        //[Column("nravizmedicexpert")]
        //public string? ExpertDoctorApprovalNumber { get; set; }       // varchar, NULL

        //[Column("procentindemnizatie")]
        //public decimal? IndemnityPercent { get; set; }                // numeric, NULL (0–100)

        //[Column("mediazilnicabazacalcul")]
        //public decimal? DailyAverageBase { get; set; }                // numeric, NULL

        //[Column("nrtotalzilecm")]
        //public int? TotalDaysCM { get; set; }                         // integer, NULL

        //[Column("nrtotalzilelucratoarecm")]
        //public int? TotalWorkingDaysCM { get; set; }                  // integer, NULL

        //[Column("nrtotalzilecalculateangajator")]
        //public int? TotalDaysCalculatedEmployer { get; set; }         // integer, NULL

        //[Column("nrtotalzilecalculatefond")]
        //public int? TotalDaysCalculatedFund { get; set; }             // integer, NULL

        //[Column("nrtotalzileplatiteangajator")]
        //public int? TotalDaysPaidByEmployer { get; set; }             // integer, NULL

        //[Column("nrtotalzileplatitefond")]
        //public int? TotalDaysPaidByFund { get; set; }                 // integer, NULL

        //[Column("sumaplatitaangajator")]
        //public decimal? AmountPaidByEmployer { get; set; }            // numeric, NULL

        //[Column("sumaplatitafond")]
        //public decimal? AmountPaidByFund { get; set; }                // numeric, NULL

        //[Column("cnpcopil")]
        //public string? ChildCnp { get; set; }                         // varchar(13), NULL

        //[Column("exceptiecalculzile")]
        //public bool? DaysCalculationException { get; set; }           // boolean, NULL

        //[Column("bazacalcul")]
        //public decimal? CalculationBase { get; set; }                 // numeric, NULL

        //[Column("zilecalcul")]
        //public int? CalculationDays { get; set; }                     // integer, NULL

        //[Column("idoperator")]
        //public int? OperatorId { get; set; }                          // integer, NULL
    }
}