using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("gradprofesional", Schema = "ru")]
    public class ProfessionalGrade
    {
        [Column("idgradprofesional")]
        public int ProfessionalGradeId { get; set; }          
        [Column("denumiregradprofesional")]
        public string? ProfessionalGradeName { get; set; }
        [Column("idgradprofesionalgrupat")]
        public int? ProfessionalGradeGroupId { get; set; } 
    }
}
