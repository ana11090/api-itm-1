// api_itm.Data.Entity.Ru.RegesSync
public class RegesSync
{
    public int Id { get; set; }

    // int, because you want 1,2,3… (and because your local Person is int)
    public int? PersonId { get; set; }
    public int? UserId { get; set; }

    // store the GUIDs you get from API
    public Guid? MessageResponseId { get; set; }   // maps to id_raspuns_mesaj (sync.responseId)
    public Guid? MessageResultId { get; set; }     // maps to id_rezultat_mesaj (queue result messageId)

    public Guid? AuthorId { get; set; } 
    public Guid? RegesEmployeeId { get; set; } 

    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
