using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.api_itm.Domain.Employees
{
    [Table("postpersoana", Schema = "public")]
    public class PostPerson
    {
        [Key]
        [Column("idpersoana")]
        public int PersonId { get; set; }                          // NOT NULL

        [Column("nume")]
        public string? LastName { get; set; }

        [Column("prenume")]
        public string? FirstName { get; set; }

        [Column("codunitate")]
        public int? UnitCode { get; set; }

        [Column("datanasterii")]
        public DateTime BirthDate { get; set; }                    // NOT NULL (date)

        [Column("loculnasterii")]
        public string? BirthPlace { get; set; }

        [Column("domeniu")]
        public string? Domain { get; set; }

        [Column("sex")]
        public string? Gender { get; set; }                        // bpchar/char -> string

        [Column("email")]
        public string? Email { get; set; }

        [Column("adresa")]
        public string? Address { get; set; }

        [Column("telefon")]
        public string? Phone { get; set; }

        [Column("cnp")]
        public string? NationalId { get; set; }                    // character -> string

        [Column("titlu")]
        public string? Title { get; set; }

        [Column("marcapersoana")]
        public string? EmployeeCode { get; set; }

        [Column("codmfin")]
        public string? MfinCode { get; set; }

        [Column("profesie")]
        public string? Occupation { get; set; }

        [Column("nationalitate")]
        public string Nationality { get; set; } = "";              // NOT NULL (character)

        [Column("seriaactidentitate")]
        public string? IdentitySeries { get; set; }

        [Column("numaractidentitate")]
        public string? IdentityNumber { get; set; }

        [Column("valabilitateactidentitate")]
        public DateTime IdentityExpiryDate { get; set; }           // NOT NULL (date)

        [Column("dataangajare")]
        public DateTime HireDate { get; set; }                     // NOT NULL (date)

        [Column("marcabaza")]
        public string? BaseMark { get; set; }

        [Column("stare")]
        public string Status { get; set; } = "";                   // NOT NULL (varchar)

        [Column("desincronizat")]
        public int Desynchronized { get; set; }                    // NOT NULL (integer)

        [Column("numecasatorie")]
        public string? MarriageName { get; set; }

        [Column("initiala")]
        public string? Initial { get; set; }

        [Column("datasfcontract")]
        public DateTime ContractEndDate { get; set; }              // NOT NULL (date)

        [Column("observatii")]
        public string? Notes { get; set; }

        [Column("utilizator_office365")]
        public string? Office365User { get; set; }

        [Column("email_personal")]
        public string? PersonalEmail { get; set; }
    }
}