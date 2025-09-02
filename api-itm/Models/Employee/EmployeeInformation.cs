using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_itm.Models.Employee
{
    public class EmployeeInformation
    {
        [JsonPropertyName("localitate")]
        public Localitate Localitate { get; set; }

        [JsonPropertyName("adresa")]
        public string Adresa { get; set; }

        [JsonPropertyName("cnp")]
        public string Cnp { get; set; }

        [JsonPropertyName("nume")]
        public string Nume { get; set; }

        [JsonPropertyName("prenume")]
        public string Prenume { get; set; }

        [JsonPropertyName("dataNastere")]
        public DateTime DataNastere { get; set; }

        [JsonPropertyName("nationalitate")]
        public NamedEntity Nationalitate { get; set; }

        [JsonPropertyName("taraDomiciliu")]
        public NamedEntity TaraDomiciliu { get; set; }

        [JsonPropertyName("tipActIdentitate")]
        public string TipActIdentitate { get; set; }

        [JsonPropertyName("apatrid")]
        public string Apatrid { get; set; }

        [JsonPropertyName("detaliiSalariatStrain")]
        public DetaliiSalariatStrain DetaliiSalariatStrain { get; set; }

        [JsonPropertyName("tipHandicap")]
        public string TipHandicap { get; set; }

        [JsonPropertyName("gradHandicap")]
        public string GradHandicap { get; set; }

        [JsonPropertyName("dataCertificatHandicap")]
        public DateTime? DataCertificatHandicap { get; set; }

        [JsonPropertyName("numarCertificatHandicap")]
        public string NumarCertificatHandicap { get; set; }

        [JsonPropertyName("dataValabilitateCertificatHandicap")]
        public DateTime? DataValabilitateCertificatHandicap { get; set; }

        [JsonPropertyName("gradInvaliditate")]
        public string GradInvaliditate { get; set; }

        [JsonPropertyName("mentiuni")]
        public string Mentiuni { get; set; }

        [JsonPropertyName("motivRadiere")]
        public string MotivRadiere { get; set; }

       
    }

    public enum TipAutorizatie // used in sending the api in controler employee view
    {
        Standard,
        Exceptie
    }

    public class Localitate
    {
        [JsonPropertyName("codSiruta")]
        public int CodSiruta { get; set; }
    }

    public class NamedEntity
    {
        [JsonPropertyName("nume")]
        public string Nume { get; set; }
    }

    public class DetaliiSalariatStrain
    {
        [JsonPropertyName("dataInceputAutorizatie")]
        public DateTime? DataInceputAutorizatie { get; set; }

        [JsonPropertyName("dataSfarsitAutorizatie")]
        public DateTime? DataSfarsitAutorizatie { get; set; }

        [JsonPropertyName("tipAutorizatie")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TipAutorizatie { get; set; }

        [JsonPropertyName("tipAutorizatieExceptie")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TipAutorizatieExceptie { get; set; }

        [JsonPropertyName("numarAutorizatie")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NumarAutorizatie { get; set; }
    }
}