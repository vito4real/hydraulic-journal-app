namespace HydraulicJournalApp.Models;

public class ProductDesignationCheckResult
{
    public bool IsAllowed { get; set; }

    public bool Exists { get; set; }

    public int ExistingProductId { get; set; }

    public string ExistingCustomerName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}