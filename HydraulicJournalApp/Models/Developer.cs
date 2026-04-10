using SQLite;

namespace HydraulicJournalApp.Models;

public class Developer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(200), NotNull]
    public string FullName { get; set; } = string.Empty;
}