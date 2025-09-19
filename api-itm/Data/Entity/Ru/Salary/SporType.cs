using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Salary
{
    /// <summary>
    /// Maps table: tipspor
    /// Columns:
    ///  - idspor               -> SporTypeId (PK)
    ///  - denumirespor         -> SporName
    ///  - codspor              -> SporCode
    ///  - versiunetipspor      -> SporTypeVersion
    ///  - codtipspor           -> SporTypeCode
    /// </summary>
    public class SporType
    {
        /// <summary>PK. DB: idspor</summary>
        public int SporTypeId { get; set; }

        /// <summary>DB: denumirespor</summary>
        public string? SporName { get; set; }

        /// <summary>DB: codspor (string here; switch to int if your schema is numeric)</summary>
        public string? SporCode { get; set; }

        /// <summary>DB: versiunetipspor (int here; switch to string if needed)</summary>
        public int? SporTypeVersion { get; set; }

        /// <summary>DB: codtipspor (string here; switch to int if your schema is numeric)</summary>
        public string? SporTypeCode { get; set; }
        public string? RegesId { get; set; }
    }
}
