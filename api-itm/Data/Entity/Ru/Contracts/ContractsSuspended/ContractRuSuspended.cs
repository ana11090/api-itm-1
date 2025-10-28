using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.ContractsSuspended
{

    [Table("contracte_ru_suspendate", Schema = "ru")]
    public class ContractRuSuspended
    {
        [Key]
        [Column("idcontractsuspendat")]
        public int ContractSuspensionId { get; set; }

        [Column("idcontract")]
        public int? ContractId { get; set; }

        [Column("idtemeilegalsuspendare")]
        public int? SuspensionLegalGroundId { get; set; }

        [Column("idtemeisuspendarelegeaeducatiei")]
        public int? EducationLawSuspensionGroundId { get; set; }

        [Column("idtemeisuspendare")]
        public int? SuspensionGroundId { get; set; }

        [Column("datainceputsuspendare")]
        public DateTime? SuspensionStartDate { get; set; }

        [Column("datasfarsitsuspendare")]
        public DateTime? SuspensionEndDate { get; set; }

        [Column("dataincetaresuspendare")]
        public DateTime? SuspensionTerminationDate { get; set; }

        [Column("numardeciziesuspendare")]
        public string? SuspensionDecisionNumber { get; set; }

        [Column("datadeciziesuspendare")]
        public DateTime? SuspensionDecisionDate { get; set; }

        [Column("numarcereresuspendare")]
        public string? SuspensionRequestNumber { get; set; }

        [Column("datacereresuspendare")]
        public DateTime? SuspensionRequestDate { get; set; }

        [Column("institutie")]
        public string? Institution { get; set; }

        [Column("functieautoritate")]
        public string? AuthorityRole { get; set; }

        [Column("numardecizieprelungire")]
        public string? ExtensionDecisionNumber { get; set; }

        [Column("datadecizieprelungire")]
        public DateTime? ExtensionDecisionDate { get; set; }

        [Column("numarcerereprelungire")]
        public string? ExtensionRequestNumber { get; set; }

        [Column("datacerereprelungire")]
        public DateTime? ExtensionRequestDate { get; set; }

        [Column("numardecizierevenire")]
        public string? ReturnDecisionNumber { get; set; }

        [Column("datadecizierevenire")]
        public DateTime? ReturnDecisionDate { get; set; }

        [Column("numarcerererevenire")]
        public string? ReturnRequestNumber { get; set; }

        [Column("datacerererevenire")]
        public DateTime? ReturnRequestDate { get; set; }

        [Column("datainceputprelungire")]
        public DateTime? ExtensionStartDate { get; set; }

        [Column("reges_sincronizare")]
        public int? RegesSyncVariable { get; set; }
    }
}



 