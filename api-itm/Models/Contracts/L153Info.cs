using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Contracts
{
    public sealed class L153Info
    {
        public int? Anexa { get; init; }
        public string? Capitol { get; init; }
        public string? Litera { get; init; }
        public string? Cod { get; init; }
        public string? Functie { get; init; }                  // denumire functie REGES (denumirefunctiereges)
        public string? FunctieLegatura { get; init; }          //  

         public string? Clasif { get; init; }
        public string? SpecFunctie { get; init; }
        public string? Structura { get; init; }
        public string? SpecStructura { get; init; }
        public string? GradProf { get; init; }
        public string? Gradatie { get; init; }
        public string? AltaFunctieName { get; init; }
        public string? ExplicatieFunctie { get; init; }
        public string? AltGradProfText { get; init; }
    }
}
