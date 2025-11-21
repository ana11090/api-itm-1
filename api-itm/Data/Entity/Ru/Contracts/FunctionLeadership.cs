using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("functie_conducere", Schema = "ru")]
    public class FunctionLeadership
    {
        [Key]
        [Column("idfunctieconducere")]
        public int FunctionLeadershipId { get; set; }

        [Column("denumirefunctieconducere")]
        public string? FunctionLeadershipName { get; set; }

        [Column("codcategoriepost")]
        public int? CategoryPostCode { get; set; }  // 1 didactic, 2 auxiliar, 3 cercetare, 4 nedidactic (dacă e cazul)

        [Column("idtipfunctie")]
        public int? FunctionTypeId { get; set; }    // 1=conducere, 2=executie, 3=coordonare (după codurile voastre)

        [Column("neremunerat")]
        public bool? IsUnpaid { get; set; }

        [Column("idfunctiegrila")]
        public int? GridFunctionId { get; set; }

        [Column("idlegaturafunctiereges")]
        public int? RegesFunctionLinkId { get; set; }

        [Column("dataoperare")]
        public DateTime? OperationDate { get; set; }

        [Column("idpersoanamod")]
        public int? ModifiedByPersonId { get; set; }
    }
}