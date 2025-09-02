using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Data.Entity.Ru.Contracts
{
    public class ContractState
    {
        public int ContractStateId { get; set; }          // idstarecontract
        public string? ContractStateName { get; set; }    // denumirestarecontract
        public string? ContractStateCode { get; set; }    // codstarecontract
    }
}