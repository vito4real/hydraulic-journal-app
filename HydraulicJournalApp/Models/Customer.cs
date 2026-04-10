using SQLite;

namespace HydraulicJournalApp.Models;

public class Customer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(200), NotNull]
    public string Name { get; set; } = string.Empty;
}