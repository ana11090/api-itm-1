using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace api_itm.Data.Entity.Ru.Lookups
    {
        [Table("functii_reges", Schema = "ru")]
        public class FunctieReges
        {
            // idfunctiereges	anexa	capitol	litera	denumirefunctiereges	idlegaturafunctiereges
            // codcategoriepost	idtransavechevechimeinv	idtransavechimeinv	codpost
            // idfunctieconducere	idgradprofesional	idnivelstudii	cod	idtipfunctie
            // an	idgradprofesionalgrupat	idpersoanamod	dataoperare	luna

            [Key]
            [Column("idfunctiereges")]
            public int FunctieRegesId { get; set; }

            [Column("anexa")]
            public int? Anexa { get; set; }

            // e.g., "1.0" → keep as string to avoid locale/decimal issues
            [Column("capitol")]
            public string? Capitol { get; set; }

            // e.g., "06" → keep as string
            [Column("litera")]
            public string? Litera { get; set; }

            [Column("denumirefunctiereges")]
            public string? DenumireFunctieReges { get; set; }

            [Column("idlegaturafunctiereges")]
            public int? IdLegaturaFunctieReges { get; set; }

            [Column("codcategoriepost")]
            public int? CodCategoriePost { get; set; }

            [Column("idtransavechevechimeinv")]
            public int? IdTransaVecheVechimeInv { get; set; }

            [Column("idtransavechimeinv")]
            public int? IdTransaVechimeInv { get; set; }

            [Column("codpost")]
            public int? CodPost { get; set; }

            [Column("idfunctieconducere")]
            public int? IdFunctieConducere { get; set; }

            [Column("idgradprofesional")]
            public int? IdGradProfesional { get; set; }

            [Column("idnivelstudii")]
            public int? IdNivelStudii { get; set; }

            // e.g., "11.006XX013.01.1"
            [Column("cod")]
            public string? Cod { get; set; }

            [Column("idtipfunctie")]
            public int? IdTipFunctie { get; set; }

            // looks like a year; keep int? (e.g., 2025)
            [Column("an")]
            public int? An { get; set; }

            [Column("idgradprofesionalgrupat")]
            public int? IdGradProfesionalGrupat { get; set; }

            [Column("idpersoanamod")]
            public int? IdPersoanaMod { get; set; }

            [Column("dataoperare")]
            public DateTime? DataOperare { get; set; }

            [Column("luna")]
            public int? Luna { get; set; }
        }
    }

}
