using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Public
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace api_itm.Data.Entity.Public // pick any namespace that avoids clashes
    {
        [Table("persoana", Schema = "public")]
        public class PublicPerson
        {
            [Key]
            [Column("idpersoana")]
            public int PersonId { get; set; }                      // NOT NULL

            [Column("nume")]
            public string? LastName { get; set; }                  // NULL

            [Column("prenume")]
            public string? FirstName { get; set; }                 // NULL

            [Column("codunitate")]
            public int? UnitCode { get; set; }                     // NULL

            [Column("datanasterii")]
            public DateTime BirthDate { get; set; }                // NOT NULL (date)

            [Column("loculnasterii")]
            public string? BirthPlace { get; set; }                // NULL

            [Column("domeniu")]
            public string? Domain { get; set; }                    // NULL

            [Column("sex")]
            public string? Sex { get; set; }                       // NULL (character)

            [Column("email")]
            public string? Email { get; set; }                     // NULL

            [Column("adresa")]
            public string? Address { get; set; }                   // NULL

            [Column("telefon")]
            public string? Phone { get; set; }                     // NULL

            [Column("cnp")]
            public string? NationalId { get; set; }                // NULL (character)

            [Column("titlu")]
            public string? Title { get; set; }                     // NULL

            [Column("marcapersoana")]
            public string? EmployeeCode { get; set; }              // NULL

            [Column("codmfin")]
            public string? MfinCode { get; set; }                  // NULL

            [Column("profesie")]
            public string? Occupation { get; set; }                // NULL

            [Column("nationalitate")]
            public string Nationality { get; set; } = "";          // NOT NULL (character)

            [Column("seriaactidentitate")]
            public string? IdentitySeries { get; set; }            // NULL (character)

            [Column("numaractidentitate")]
            public string? IdentityNumber { get; set; }            // NULL (character)

            [Column("valabilitateactidentitate")]
            public DateTime IdentityExpiryDate { get; set; }       // NOT NULL (date)

            [Column("dataangajare")]
            public DateTime HireDate { get; set; }                 // NOT NULL (date)

            [Column("marcabaza")]
            public string? BaseMark { get; set; }                  // NULL (character)

            [Column("stare")]
            public string Status { get; set; } = "";               // NOT NULL (varchar)

            [Column("desincronizat")]
            public int Desynchronized { get; set; }                // NOT NULL (integer)

            [Column("numecasatorie")]
            public string? MarriageName { get; set; }              // NULL

            [Column("initiala")]
            public string? Initial { get; set; }                   // NULL

            [Column("datasfcontract")]
            public DateTime ContractEndDate { get; set; }          // NOT NULL (date)

            [Column("observatii")]
            public string? Notes { get; set; }                     // NULL

            [Column("utilizator_office365")]
            public string? Office365User { get; set; }             // NULL

            [Column("email_personal")]
            public string? PersonalEmail { get; set; }             // NULL
        }
    }
}
