using System;
using api_itm.Data;
using api_itm.Data.Entity.Ru;
using api_itm.Models.Employee;

namespace api_itm.Infrastructure.Mappers
{
    public static class EmployeeMapper
    {
        public static EmployeeInformation FromPerson(
          Person p,
          string nationalityName,
          string domicileCountryName,
          string IdentityDocumentCode,
          string? handicapTypeCode,
          string? handicapGradeCode,
          string? invalidityGradeCode)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));

            // Only include handicap-related fields if HandicapTypeId is between 1 and 10
            bool includeHandicap = p.HandicapTypeId >= 1 && p.HandicapTypeId <= 10;

            return new EmployeeInformation
            {
                Localitate = new Localitate { CodSiruta = p.SirutaCode ?? 0 },
                Adresa = p.Address ?? "",
                Cnp = p.NationalId ?? "",
                Nume = p.LastName ?? "",
                Prenume = p.FirstName ?? "",
                DataNastere = p.BirthDate ?? DateTime.MinValue,

                Nationalitate = new NamedEntity { Nume = nationalityName ?? "" },
                TaraDomiciliu = new NamedEntity { Nume = domicileCountryName ?? "" },
                TipActIdentitate = IdentityDocumentCode ?? "",

                //DetaliiSalariatStrain = new DetaliiSalariatStrain
                //{
                //    DataInceputAutorizatie = p.WorkPermitStartDate ?? DateTime.MinValue,
                //    DataSfarsitAutorizatie = p.WorkPermitEndDate ?? DateTime.MinValue,
                //    TipAutorizatie = "Exceptie",
                //    TipAutorizatieExceptie = "Art32LiteraK",
                //    NumarAutorizatie = "1234K"
                //},

                TipHandicap = includeHandicap ? handicapTypeCode ?? "" : null,
                GradHandicap = includeHandicap ? handicapGradeCode ?? "" : null,
                DataCertificatHandicap = includeHandicap ? p.HandicapCertificateDate : null,
                NumarCertificatHandicap = includeHandicap ? p.HandicapCertificateNumber ?? "" : null,
                DataValabilitateCertificatHandicap = includeHandicap ? p.DisabilityReviewDate : null,
                GradInvaliditate = includeHandicap ? invalidityGradeCode ?? "" : null,

                Mentiuni = "",
                MotivRadiere = ""
            };
        }

    }
}
