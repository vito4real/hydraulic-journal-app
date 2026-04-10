using SQLite;

namespace HydraulicJournalApp.Models;

public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100), NotNull]
    public string Designation { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [Indexed]
    public int CustomerId { get; set; }
}