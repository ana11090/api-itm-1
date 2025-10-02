using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts.ContractsSuspended
{
    [Table("temeilegalsuspendare", Schema = "ru")]
    public class SuspensionLegalGround
    {
        [Key]
        [Column("idtemeilegalsuspendare")]
        public int SuspensionLegalGroundId { get; set; }

        [Column("denumiretemeilegalsuspendare")]
        [Required, StringLength(200)]
        public string SuspensionLegalGroundName { get; set; } = string.Empty;

        [Column("codtemeilegalsuspendare")]
        [Required, StringLength(100)]
        public string SuspensionLegalGroundCode { get; set; } = string.Empty;

        // Forma deciziei de suspendare (e.g., type/format of decision document)
        [Column("formadeciziesuspendare")]
        public string? SuspensionDecisionForm { get; set; }

        // Formulă/wording/template for the suspension decision
        [Column("formuladeciziesuspendare")]
        public string? SuspensionDecisionFormula { get; set; }
    }
}