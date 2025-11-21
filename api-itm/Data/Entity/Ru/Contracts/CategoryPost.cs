using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    [Table("categoriepost", Schema = "ru")]
    public class CategoryPost
    {
        [Key]
        [Column("codcategoriepost")]
        public int CategoryPostCode { get; set; }          // e.g., 1 = didactic, 2 = auxiliar, 3 = cercetare, 4 = nedidactic

        [Column("denumirecategoriepost")]
        public string? CategoryPostName { get; set; }
    }
}