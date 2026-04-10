using SQLite;

namespace HydraulicJournalApp.Models;

public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ProductId { get; set; }

    [Indexed]
    public int DeveloperId { get; set; }

    public DateTime IssueDate { get; set; }

    public KitType KitType { get; set; }
}