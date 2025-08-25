using api_itm.Data.Entity.Ru;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Reges
{
    public class PersonRegesIds
    {
        public int PersonRegesIdsId { get; set; }   // PK
        public int PersonId { get; set; }           // FK -> People.PersonId (unique)
        public string? RegesSalariatId { get; set; } // GUID string returned by REGES
        public string? LastReceiptId { get; set; }   // last sync responseId (recipisă) that produced this mapping
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Person Person { get; set; }         
    }
}