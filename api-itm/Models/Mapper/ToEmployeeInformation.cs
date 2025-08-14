using api_itm.Data.Entity.api_itm.Domain.Employees;
using api_itm.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models.Mapper
{
    //public static EmployeeInformation ToEmployeeInformation(Person person, string tipHandicap, string gradHandicap)
    //{
        //return new EmployeeInformation
        //{
        //    Localitate = new Localitate { CodSiruta = person.CodSiruta ?? 0 },
        //    Adresa = person.Adresa,
        //    Cnp = person.Cnp,
        //    Nume = person.Nume,
        //    Prenume = person.Prenume,
        //    DataNastere = person.DataNasterii ?? DateTime.MinValue,
        //    Nationalitate = new NamedEntity { Nume = person.Cetatenie },
        //    TaraDomiciliu = new NamedEntity { Nume = person.TaraDomiciliu?.CountryName ?? "" }, // assuming navigation property
        //    TipActIdentitate = person.TipAct?.Denumire ?? "",
        //    Apatrid = person.TipApatrid?.Denumire ?? "",
        //    DetaliiSalariatStrain = new DetaliiSalariatStrain
        //    {
        //        DataInceputAutorizatie = person.DataInceputAutorizatie ?? DateTime.MinValue,
        //        DataSfarsitAutorizatie = person.DataSfarsitAutorizatie ?? DateTime.MinValue,
        //        TipAutorizatie = "Exceptie", // or get from another table
        //        TipAutorizatieExceptie = "Art32LiteraK",
        //        NumarAutorizatie = "1234K"
        //    },
        //    TipHandicap = tipHandicap,
        //    GradHandicap = gradHandicap,
        //    DataCertificatHandicap = person.DataCertificatHandicap,
        //    NumarCertificatHandicap = person.NrCertificatHandicap,
        //    DataValabilitateCertificatHandicap = DateTime.Now.AddYears(5), // for example
        //    Mentiuni = "Mentiuni salariat",
        //    MotivRadiere = "Adaugat din greseala"
        //};
    //}
}