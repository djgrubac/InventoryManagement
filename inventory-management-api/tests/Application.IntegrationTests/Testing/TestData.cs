namespace Inventory_Management.Application.IntegrationTests.Testing;

/// <summary>Builders for valid, unsaved entity graphs. Navigation properties are set so a single Add saves the graph.</summary>
public static class TestData
{
    public static readonly DateTimeOffset BaseTime = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static Category Category(string caption = "Tools") => new() { Caption = caption };

    public static Product Product(Category category, string name = "Hammer", decimal price = 10m) => new()
    {
        Name = name,
        Price = price,
        Category = category,
        StockQuantity = 0,
        Description = $"{name} description"
    };

    public static Warehouse Warehouse(string name = "Main", string? company = "Acme") => new()
    {
        Name = name,
        Company = company,
        Address = $"{name} street 1",
        ContactPerson = "Jane"
    };

    public static Stock Stock(Product product, Warehouse warehouse, int quantity) => new()
    {
        Product = product,
        Warehouse = warehouse,
        Quantity = quantity
    };

    public static Order Order(
        Warehouse warehouse,
        OrderType type = OrderType.Incoming,
        OrderStatus status = OrderStatus.Pending,
        DateTimeOffset? created = null,
        params (Product Product, int Quantity)[] items)
    {
        var order = new Order
        {
            Warehouse = warehouse,
            Type = type,
            Status = status,
            Created = created ?? BaseTime
        };

        foreach (var (product, quantity) in items)
            order.Items.Add(new OrderItem { Product = product, Quantity = quantity, Price = product.Price });

        order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity);
        return order;
    }
}
