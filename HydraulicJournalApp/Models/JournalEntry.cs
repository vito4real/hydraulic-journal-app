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

    // Дата выдачи ТЗ в работу
    public DateTime IssueDate { get; set; }

    // Дата выдачи комплекта КД
    public DateTime? DocumentationIssuedDate { get; set; }
}