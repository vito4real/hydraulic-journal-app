using SQLite;
using HydraulicJournalApp.Models;

namespace HydraulicJournalApp.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _db;

    public DatabaseService(string dbPath)
    {
        _db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitAsync()
    {
        await _db.CreateTableAsync<Developer>();
        await _db.CreateTableAsync<Customer>();
        await _db.CreateTableAsync<Product>();
        await _db.CreateTableAsync<JournalEntry>();

        await EnsureJournalEntryDocumentationIssuedDateColumnAsync();
    }

    private async Task EnsureJournalEntryDocumentationIssuedDateColumnAsync()
    {
        var columns = await _db.QueryAsync<TableInfo>("PRAGMA table_info(JournalEntry)");

        if (!columns.Any(x => x.name == nameof(JournalEntry.DocumentationIssuedDate)))
        {
            await _db.ExecuteAsync("ALTER TABLE JournalEntry ADD COLUMN DocumentationIssuedDate TEXT");
        }
    }

    public Task<List<Developer>> GetDevelopersAsync()
    {
        return _db.Table<Developer>()
            .OrderBy(x => x.FullName)
            .ToListAsync();
    }

    public async Task<int> AddDeveloperAsync(string fullName)
    {
        fullName = (fullName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new Exception("Developer name is required.");

        var existing = await _db.Table<Developer>()
            .FirstOrDefaultAsync(x => x.FullName == fullName);

        if (existing != null)
            return existing.Id;

        var developer = new Developer
        {
            FullName = fullName
        };

        await _db.InsertAsync(developer);
        return developer.Id;
    }

    public Task<List<Customer>> GetCustomersAsync()
    {
        return _db.Table<Customer>()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<int> AddCustomerAsync(string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("Customer name is required.");

        var existing = await _db.Table<Customer>()
            .FirstOrDefaultAsync(x => x.Name == name);

        if (existing != null)
            return existing.Id;

        var customer = new Customer
        {
            Name = name
        };

        await _db.InsertAsync(customer);
        return customer.Id;
    }

    public Task<List<Product>> GetProductsAsync()
    {
        return _db.Table<Product>()
            .OrderBy(x => x.Designation)
            .ToListAsync();
    }

    public Task<Product?> GetProductByIdAsync(int productId)
    {
        return _db.Table<Product>()
            .FirstOrDefaultAsync(x => x.Id == productId);
    }

    public Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        return _db.Table<Customer>()
            .FirstOrDefaultAsync(x => x.Id == customerId);
    }

    public Task<Product?> GetProductByDesignationAndCustomerAsync(string designation, int customerId)
    {
        designation = (designation ?? string.Empty).Trim();

        return _db.Table<Product>()
            .FirstOrDefaultAsync(x => x.Designation == designation && x.CustomerId == customerId);
    }

    public Task<Product?> GetProductByDesignationAsync(string designation)
    {
        designation = (designation ?? string.Empty).Trim();

        return _db.Table<Product>()
            .FirstOrDefaultAsync(x => x.Designation == designation);
    }

    public async Task<List<Product>> GetProductsByDesignationAsync(string designation)
    {
        designation = (designation ?? string.Empty).Trim();

        return await _db.Table<Product>()
            .Where(x => x.Designation == designation)
            .ToListAsync();
    }

    public async Task<int> AddProductAsync(string designation, string name, int customerId)
    {
        designation = (designation ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(designation))
            throw new Exception("Designation is required.");

        if (customerId <= 0)
            throw new Exception("Customer must be selected.");

        var existing = await GetProductByDesignationAndCustomerAsync(designation, customerId);

        if (existing != null)
            throw new Exception($"Designation \"{designation}\" already exists for this customer.");

        var product = new Product
        {
            Designation = designation,
            Name = name,
            CustomerId = customerId
        };

        await _db.InsertAsync(product);
        return product.Id;
    }

    public async Task<List<JournalEntryListItem>> GetJournalEntriesAsync()
    {
        var entries = await _db.Table<JournalEntry>().ToListAsync();
        var products = await _db.Table<Product>().ToListAsync();
        var developers = await _db.Table<Developer>().ToListAsync();
        var customers = await _db.Table<Customer>().ToListAsync();

        return entries
            .Select(entry =>
            {
                var product = products.FirstOrDefault(x => x.Id == entry.ProductId);
                var developer = developers.FirstOrDefault(x => x.Id == entry.DeveloperId);
                var customer = product == null
                    ? null
                    : customers.FirstOrDefault(x => x.Id == product.CustomerId);

                return new JournalEntryListItem
                {
                    Id = entry.Id,
                    Designation = product?.Designation ?? string.Empty,
                    ProductName = product?.Name ?? string.Empty,
                    DeveloperName = developer?.FullName ?? string.Empty,
                    IssueDate = entry.IssueDate,
                    DocumentationIssuedDate = entry.DocumentationIssuedDate,
                    CustomerName = customer?.Name ?? string.Empty
                };
            })
            .ToList();
    }

    public async Task<int> AddJournalEntryWithProductAsync(
        string designation,
        string productName,
        int customerId,
        int developerId,
        DateTime issueDate)
    {
        designation = (designation ?? string.Empty).Trim();
        productName = (productName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(designation))
            throw new Exception("Обозначение изделия обязательно.");

        if (customerId <= 0)
            throw new Exception("Клиент должен быть выбран.");

        if (developerId <= 0)
            throw new Exception("Разработчик должен быть выбран.");

        var product = await GetProductByDesignationAndCustomerAsync(designation, customerId);

        if (product == null)
        {
            product = new Product
            {
                Designation = designation,
                Name = productName,
                CustomerId = customerId
            };

            await _db.InsertAsync(product);
        }

        var existingJournalEntry = await _db.Table<JournalEntry>()
            .FirstOrDefaultAsync(x => x.ProductId == product.Id);

        if (existingJournalEntry != null)
        {
            throw new Exception(
                $"Запись в журнале для обозначения \"{designation}\" уже существует от {existingJournalEntry.IssueDate:dd.MM.yyyy}.");
        }

        var entry = new JournalEntry
        {
            ProductId = product.Id,
            DeveloperId = developerId,
            IssueDate = issueDate,
            DocumentationIssuedDate = null
        };

        await _db.InsertAsync(entry);

        return entry.Id;
    }

    public async Task SetDocumentationIssuedDateAsync(int journalEntryId, DateTime date)
    {
        var entry = await _db.Table<JournalEntry>()
            .FirstOrDefaultAsync(x => x.Id == journalEntryId);

        if (entry == null)
            throw new Exception("Запись журнала не найдена.");

        entry.DocumentationIssuedDate = date;

        await _db.UpdateAsync(entry);
    }

    public Task<Developer?> GetDeveloperByIdAsync(int developerId)
    {
        return _db.Table<Developer>()
            .FirstOrDefaultAsync(x => x.Id == developerId);
    }

    public async Task<List<Product>> GetProductsByCustomerAsync(int customerId)
    {
        return await _db.Table<Product>()
            .Where(x => x.CustomerId == customerId)
            .OrderBy(x => x.Designation)
            .ToListAsync();
    }

    public async Task<List<DeveloperProductListItem>> GetProductsByDeveloperAsync(int developerId)
    {
        var entries = await _db.Table<JournalEntry>()
            .Where(x => x.DeveloperId == developerId)
            .OrderByDescending(x => x.IssueDate)
            .ToListAsync();

        var products = await _db.Table<Product>().ToListAsync();

        return entries
            .Select(entry =>
            {
                var product = products.FirstOrDefault(x => x.Id == entry.ProductId);

                return new DeveloperProductListItem
                {
                    ProductId = entry.ProductId,
                    Designation = product?.Designation ?? string.Empty,
                    ProductName = product?.Name ?? string.Empty,
                    IssueDate = entry.IssueDate
                };
            })
            .GroupBy(x => x.ProductId)
            .Select(g => g.First())
            .OrderBy(x => x.Designation)
            .ToList();
    }
}

public class JournalEntryListItem
{
    public int Id { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? DocumentationIssuedDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public string IssueDateDisplay => IssueDate.ToString("dd.MM.yyyy");

    public string DocumentationIssuedDateDisplay =>
        DocumentationIssuedDate.HasValue
            ? DocumentationIssuedDate.Value.ToString("dd.MM.yyyy")
            : "—";

    public string DocumentationButtonText =>
        DocumentationIssuedDate.HasValue
            ? "Изменить дату КД"
            : "Указать дату КД";
}

public class DeveloperProductListItem
{
    public int ProductId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }

    public string IssueDateDisplay => IssueDate.ToString("dd.MM.yyyy");
}

public class TableInfo
{
    public int cid { get; set; }
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public int notnull { get; set; }
    public string? dflt_value { get; set; }
    public int pk { get; set; }
}