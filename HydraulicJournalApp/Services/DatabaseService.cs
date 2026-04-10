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
    }

    // -----------------------------
    // Developers
    // -----------------------------
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

        return await _db.InsertAsync(new Developer
        {
            FullName = fullName
        });
    }

    // -----------------------------
    // Customers
    // -----------------------------
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

        return await _db.InsertAsync(new Customer
        {
            Name = name
        });
    }

    // -----------------------------
    // Products
    // -----------------------------
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

    public async Task<ProductDesignationCheckResult> CheckDesignationAsync(string designation, int customerId)
    {
        designation = (designation ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(designation))
        {
            return new ProductDesignationCheckResult
            {
                IsAllowed = false,
                Exists = false,
                Message = "Designation is required."
            };
        }

        var existingProduct = await _db.Table<Product>()
            .FirstOrDefaultAsync(x => x.Designation == designation);

        if (existingProduct == null)
        {
            return new ProductDesignationCheckResult
            {
                IsAllowed = true,
                Exists = false,
                Message = string.Empty
            };
        }

        var existingCustomer = await _db.Table<Customer>()
            .FirstOrDefaultAsync(x => x.Id == existingProduct.CustomerId);

        var existingCustomerName = existingCustomer?.Name ?? "Unknown customer";

        if (existingProduct.CustomerId != customerId)
        {
            return new ProductDesignationCheckResult
            {
                IsAllowed = false,
                Exists = true,
                ExistingProductId = existingProduct.Id,
                ExistingCustomerName = existingCustomerName,
                Message = $"Designation \"{designation}\" is already used for customer: {existingCustomerName}."
            };
        }

        return new ProductDesignationCheckResult
        {
            IsAllowed = false,
            Exists = true,
            ExistingProductId = existingProduct.Id,
            ExistingCustomerName = existingCustomerName,
            Message = $"Designation \"{designation}\" already exists for this customer."
        };
    }

    public async Task<int> AddProductAsync(string designation, string name, int customerId)
    {
        designation = (designation ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(designation))
            throw new Exception("Designation is required.");

        if (customerId <= 0)
            throw new Exception("Customer must be selected.");

        var check = await CheckDesignationAsync(designation, customerId);

        if (!check.IsAllowed)
            throw new Exception(check.Message);

        var product = new Product
        {
            Designation = designation,
            Name = name,
            CustomerId = customerId
        };

        await _db.InsertAsync(product);
        return product.Id;
    }

    // -----------------------------
    // Journal
    // -----------------------------
    public async Task<int> AddJournalEntryAsync(int productId, int developerId, DateTime issueDate, KitType kitType)
    {
        if (productId <= 0)
            throw new Exception("Product must be selected.");

        if (developerId <= 0)
            throw new Exception("Developer must be selected.");

        var entry = new JournalEntry
        {
            ProductId = productId,
            DeveloperId = developerId,
            IssueDate = issueDate,
            KitType = kitType
        };

        await _db.InsertAsync(entry);
        return entry.Id;
    }

    public async Task<List<JournalEntryListItem>> GetJournalEntriesAsync()
    {
        var entries = await _db.Table<JournalEntry>()
            .OrderByDescending(x => x.IssueDate)
            .ToListAsync();

        var products = await _db.Table<Product>().ToListAsync();
        var developers = await _db.Table<Developer>().ToListAsync();
        var customers = await _db.Table<Customer>().ToListAsync();

        var result = entries
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
                    KitType = entry.KitType,
                    CustomerName = customer?.Name ?? string.Empty
                };
            })
            .ToList();

        return result;
    }

    public async Task<int> AddJournalEntryWithProductAsync(
    string designation,
    string productName,
    int customerId,
    int developerId,
    DateTime issueDate,
    KitType kitType)
    {
        designation = (designation ?? string.Empty).Trim();
        productName = (productName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(designation))
            throw new Exception("Обозначение изделия обязательно.");

        if (customerId <= 0)
            throw new Exception("Клиент должен быть выбран.");

        if (developerId <= 0)
            throw new Exception("Разработчик должен быть выбран.");

        var existingProduct = await _db.Table<Product>()
            .FirstOrDefaultAsync(x => x.Designation == designation);

        if (existingProduct == null)
        {
            var newProduct = new Product
            {
                Designation = designation,
                Name = productName,
                CustomerId = customerId
            };

            await _db.InsertAsync(newProduct);

            var newEntry = new JournalEntry
            {
                ProductId = newProduct.Id,
                DeveloperId = developerId,
                IssueDate = issueDate,
                KitType = kitType
            };

            await _db.InsertAsync(newEntry);

            return newEntry.Id;
        }

        if (existingProduct.CustomerId != customerId)
        {
            var existingCustomer = await _db.Table<Customer>()
                .FirstOrDefaultAsync(x => x.Id == existingProduct.CustomerId);

            var existingCustomerName = existingCustomer?.Name ?? "Неизвестный клиент";

            throw new Exception(
                $"Обозначение \"{designation}\" уже занято для клиента: {existingCustomerName}.");
        }

        var existingJournalEntry = await _db.Table<JournalEntry>()
            .FirstOrDefaultAsync(x => x.ProductId == existingProduct.Id);

        if (existingJournalEntry != null)
        {
            throw new Exception(
                $"Запись в журнале для обозначения \"{designation}\" уже существует от {existingJournalEntry.IssueDate:dd.MM.yyyy}.");
        }

        var entry = new JournalEntry
        {
            ProductId = existingProduct.Id,
            DeveloperId = developerId,
            IssueDate = issueDate,
            KitType = kitType
        };

        await _db.InsertAsync(entry);

        return entry.Id;
    }

    public Task<Product?> GetProductByDesignationAsync(string designation)
    {
        designation = (designation ?? string.Empty).Trim();

        return _db.Table<Product>()
            .FirstOrDefaultAsync(x => x.Designation == designation);
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

        var result = entries
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

        return result;
    }
}

public class JournalEntryListItem
{
    public int Id { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string DeveloperName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public KitType KitType { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public string KitTypeDisplay =>
        KitType == KitType.Experimental ? "Опытный" : "Контрольный";

    public string IssueDateDisplay => IssueDate.ToString("dd.MM.yyyy");
}

public class DeveloperProductListItem
{
    public int ProductId { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }

    public string IssueDateDisplay => IssueDate.ToString("dd.MM.yyyy");
}