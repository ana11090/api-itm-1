using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("post", Schema = "ru")]
    public class Post
    {
        [Key]
        [Column("codpost")]
        public int PostCode { get; set; }                       // e.g., 111

        [Column("denumirepost")]
        public string? PostName { get; set; }                   // e.g., "supraveghetor noapte"

        [Column("codcategoriepost")]
        public int? CategoryPostCode { get; set; }              // FK → categoriepost.codcategoriepost

        [Column("desincronizat")]
        public bool? Desynchronized { get; set; }               // 0/1 in DB → map to bool?

        [Column("idtipfunctie")]
        public int? FunctionTypeId { get; set; }                // 1=conducere, 2=executie, 3=coordonare (după codurile tale)

        [Column("dataoperare")]
        public DateTime? OperationDate { get; set; }

        [Column("idpersoanamod")]
        public int? ModifiedByPersonId { get; set; }

        [Column("idfunctiegrila")]
        public int? GridFunctionId { get; set; }

        [Column("idlegaturafunctiereges")]
        public int? RegesFunctionLinkId { get; set; }

        // Navigation (optional)
        public CategoryPost? CategoryPost { get; set; }
    }
}