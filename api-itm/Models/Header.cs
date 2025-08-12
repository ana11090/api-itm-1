using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Models
{
    [Table("Headers")]
    public class Header
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }                    // se setează singur (IDENTITY)

        [Required, MaxLength(50)]
        public string MessageId { get; set; }          // generat automat

        [Required, MaxLength(50)]
        public string ClientApplication { get; set; } = "SAP"; // fix

        [Required, MaxLength(10)]
        public string Version { get; set; } = "1";     // fix

        [MaxLength(100)]
        public string Operation { get; set; }          // setată la trimitere

        [MaxLength(100)]
        public string AuthorId { get; set; }           // setată la trimitere

        [MaxLength(100)]
        public string SessionId { get; set; }          // GUID la trimitere (per sesiune)

        [MaxLength(200)]
        public string User { get; set; }               // din DB

        [MaxLength(100)]
        public string UserId { get; set; }             // din DB

        public DateTime? Timestamp { get; set; }       // setată la trimitere (UTC)

        public Header()
        {
            MessageId = Guid.NewGuid().ToString("D");  // GUID nou per înregistrare
            // Timestamp-ul se va pune la trimitere
        }
    }
}