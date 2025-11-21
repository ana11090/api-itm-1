using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    public class FunctionStat
    {
        [Key]
        public int FunctionStatId { get; set; }          // idstatfunctii
        public string? UnitCode { get; set; }            // codunitate
        public int? FunctionTypeId { get; set; }         // idtipfunctie
        public int? AcademicYearId { get; set; }         // idanuniversitar
        public int? LeadershipFunctionId { get; set; }   // idfunctieconducere
        public string? PostCode { get; set; }            // codpost
        public int? FunctionId { get; set; }             // idfunctie
        public int? QualificationTypeId { get; set; }    // idtipcalificare
        public int? EducationLevelId { get; set; }       // idnivelstudii
        public int? ProfessionalGradeId { get; set; }    // idgradprofesional
        public string? Period { get; set; }              // perioada
        public DateTime? CreatedAt { get; set; }         // datacreare
        public DateTime? AbolishedAt { get; set; }       // datadesfiintare
        public string? EstablishmentAct { get; set; }    // actinfiintare
        public decimal? DidacticLoad { get; set; }       // incarcaturadidactica
        public int? Position { get; set; }               // pozitie
        public string? Status { get; set; }              // stare
        public string? Notes { get; set; }               // observatii
        public string? CompetitionProcedure { get; set; }// proceduradeconcurs
        public string? DaDi { get; set; }                // da_di
        public int? DocumentId { get; set; }             // iddocument
        public DateTime? TransferDate { get; set; }      // datatransfer
        public string? TransferAct { get; set; }         // acttransfer
        public DateTime? DeletionDate { get; set; }      // datastergere
        public string? AbolishmentAct { get; set; }      // actdesfiintare
        public DateTime? TransformationDate { get; set; }// datatransformare
        public string? TransformationAct { get; set; }   // acttransformare
    }
}