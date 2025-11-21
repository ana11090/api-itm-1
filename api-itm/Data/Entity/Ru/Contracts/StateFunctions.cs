using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("functii_stare", Schema = "ru")]
    public class StateFunctions
    {
        [Key]
        [Column("idstatfunctii")]
        public int IdFunctionState { get; set; }
        [Column("codunitate")]
        public int? UnitCode { get; set; }

        [Column("idtipfunctie")]
        public int? IdTypeFunction { get; set; }
        [Column("idanuniversitar")]
        public int? IdAcademicYear { get; set; }
        [Column("idfunctieconducere")]
        public int? IdFunctionLeadership { get; set; }

        [Column("codpost")]
        public string? PostCode { get; set; }
        [Column("idfunctie")]
        public int? IdFunction { get; set; }
        [Column("idtipcalificare")]
        public int? IdQualificationType { get; set; }
        [Column("idnivelstudii")]
        public int? IdEducationLevel { get; set; }
        [Column("idgradprofesional")]
        public int? IdProfessionalGrade { get; set; }
        [Column("perioada")]
        public string? Period { get; set; }
        [Column("datacreare")]
        public DateTime? CreatedAt { get; set; }
        [Column("datadesfiintare")]
        public DateTime? AbolishedAt { get; set; }
        [Column("actinfiintare")]
        public string? EstablishmentAct { get; set; }
        [Column("incarcaturadidactica")]    
        public int? DidacticLoad { get; set; }
        [Column("pozitie")]
        public int? Position { get; set; }
        [Column("stare")]
        public string? Status { get; set; }
        [Column("observatii")]
        public string? Notes { get; set; }
        [Column("proceduradeconcurs")]
        public bool? CompetitionProcedure { get; set; }
        [Column("da_di")]
        public bool? DaDi { get; set; }
        [Column("iddocument")]
        public int? DocumentId { get; set; }

        [Column("datatransfer")]
        public DateTime? TransferDate { get; set; }
        [Column("acttransfer")]
        public string? TransferAct { get; set; }
        [Column("datastergere")]
        public DateTime? DeletionDate { get; set; }
        [Column("actdesfiintare")]
        public string? AbolishmentAct { get; set; }
        [Column("datatransformare")]
        public DateTime? TransformationDate { get; set; }
        [Column("acttransformare")]
        public string? TransformationAct { get; set; } 
    }
}
