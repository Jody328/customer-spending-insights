using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

// -----------------------
// CONFIG
// -----------------------
const int count = 500;
var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
var end = new DateTime(2024, 9, 16, 23, 59, 59, DateTimeKind.Utc);

// Output should land here (relative to tools/SeedGenerator/)
var outputPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
    "..", "..", "..", "..", "..",  // go from bin/... -> SeedGenerator -> tools -> api
    "src", "seed", "transactions.json"));

// deterMinistic randomness
var rng = new Random(12345);

// -----------------------
// CATEGORY METADATA (match your filters.json)
// -----------------------
var categories = new[]
{
    new CategoryMeta("Groceries", "#FF6B6B", "shopping-cart", new[]
    {
        "Pick n Pay", "Checkers", "Shoprite", "Woolworths Food", "Spar"
    }, Min: 30, Max: 650),

    new CategoryMeta("Entertainment", "#4ECDC4", "film", new[]
    {
        "Netflix", "Spotify", "ShowMax", "Steam", "Ster-Kinekor"
    }, Min: 50, Max: 500),

    new CategoryMeta("Transportation", "#45B7D1", "car", new[]
    {
        "Engen", "Shell", "BP", "Uber", "Bolt"
    }, Min: 40, Max: 1200),

    new CategoryMeta("Dining", "#F7DC6F", "utensils", new[]
    {
        "KFC", "McDonald's", "Nando's", "Ocean Basket", "Mugg & Bean"
    }, Min: 60, Max: 900),

    new CategoryMeta("Shopping", "#BB8FCE", "shopping-bag", new[]
    {
        "Takealot", "Mr Price", "H&M", "Game", "Cape Union Mart"
    }, Min: 80, Max: 2500),

    new CategoryMeta("Utilities", "#85C1E9", "zap", new[]
    {
        "Eskom", "City of Cape Town", "Vodacom", "MTN", "Telkom"
    }, Min: 100, Max: 2200),
};

var paymentMethods = new[]
{
    "Credit Card",
    "Debit Card",
    "Debit Order",
    "EFT",
    "Cash"
};

var descriptions = new Dictionary<string, string[]>
{
    ["Groceries"] = new[] { "Weekly groceries", "Top-up essentials", "Household items", "Monthly stock-up" },
    ["Entertainment"] = new[] { "Monthly subscription", "Movie tickets", "Game purchase", "StreaMing subscription" },
    ["Transportation"] = new[] { "Fuel refill", "Ride share", "Parking", "Car wash" },
    ["Dining"] = new[] { "Lunch", "Dinner", "Takeaway", "Coffee & snacks" },
    ["Shopping"] = new[] { "Clothing", "Online purchase", "Electronics accessory", "General shopping" },
    ["Utilities"] = new[] { "Electricity", "Mobile bill", "Internet bill", "Municipal services" },
};

// Weighted distribution (more groceries + transport)
var weightedCategories = new (string Name, int Weight)[]
{
    ("Groceries", 28),
    ("Transportation", 18),
    ("Dining", 18),
    ("Shopping", 14),
    ("Utilities", 12),
    ("Entertainment", 10),
};

// -----------------------
// GENERATE
// -----------------------
var txns = new List<TransactionDto>(capacity: count);

for (int i = 0; i < count; i++)
{
    var categoryName = PickWeighted(weightedCategories, rng);
    var meta = categories.Single(c => c.Name == categoryName);

    // random date across range, with time-of-day bias
    var dt = RandomDateTime(start, end, rng);
    dt = BiasTimeOfDay(dt, rng);

    // amount distribution: mostly smaller, sometimes big spikes
    var amount = RandomAmount(meta.Min, meta.Max, rng);

    // occasional outliers for realism
    if (meta.Name is "Shopping" or "Utilities" && rng.NextDouble() < 0.06)
        amount *= (decimal)(1.3 + rng.NextDouble() * 1.8);

    // keep amounts sane
    amount = decimal.Round(Math.Clamp(amount, 10m, 5000m), 2);

    var merchant = meta.Merchants[rng.Next(meta.Merchants.Length)];
    var desc = descriptions[meta.Name][rng.Next(descriptions[meta.Name].Length)];
    var pay = paymentMethods[rng.Next(paymentMethods.Length)];

    txns.Add(new TransactionDto
    {
        Id = $"txn_{dt:yyyyMMdd}_{i:D4}",
        Date = dt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
        Merchant = merchant,
        Category = meta.Name,
        Amount = amount,
        Description = desc,
        PaymentMethod = pay,
        Icon = meta.Icon,
        CategoryColor = meta.Color
    });
}

// Sort newest first (nice for default date_desc)
txns = txns.OrderByDescending(t => t.Date).ToList();

// Write JSON
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

File.WriteAllText(outputPath, JsonSerializer.Serialize(txns, jsonOptions));

Console.WriteLine($"✅ Generated {txns.Count} transactions -> {outputPath}");

// -----------------------
// HELPERS
// -----------------------
static string PickWeighted((string Name, int Weight)[] items, Random rng)
{
    var total = items.Sum(i => i.Weight);
    var roll = rng.Next(1, total + 1);
    var cumulative = 0;

    foreach (var item in items)
    {
        cumulative += item.Weight;
        if (roll <= cumulative) return item.Name;
    }

    return items[0].Name;
}

static DateTime RandomDateTime(DateTime start, DateTime end, Random rng)
{
    var range = (end - start).TotalSeconds;
    var offset = rng.NextDouble() * range;
    return start.AddSeconds(offset);
}

static DateTime BiasTimeOfDay(DateTime dt, Random rng)
{
    // Bias toward typical spending times: morning, lunch, evening
    var roll = rng.NextDouble();

    int hour = roll switch
    {
        < 0.25 => rng.Next(7, 11),   // morning
        < 0.55 => rng.Next(11, 15),  // lunch
        < 0.90 => rng.Next(16, 21),  // evening
        _ => rng.Next(0, 24),
    };

    var Minute = rng.Next(0, 60);
    var second = rng.Next(0, 60);

    return new DateTime(dt.Year, dt.Month, dt.Day, hour, Minute, second, DateTimeKind.Utc);
}

static decimal RandomAmount(decimal Min, decimal Max, Random rng)
{
    // Skew towards smaller amounts: square the random
    var t = (decimal)rng.NextDouble();
    t = t * t;
    return Min + (Max - Min) * t;
}

sealed record CategoryMeta(string Name, string Color, string Icon, string[] Merchants, decimal Min, decimal Max);

sealed class TransactionDto
{
    public string Id { get; set; } = "";
    public string Date { get; set; } = "";
    public string Merchant { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string PaymentMethod { get; set; } = "";
    public string Icon { get; set; } = "";
    public string CategoryColor { get; set; } = "";
}
