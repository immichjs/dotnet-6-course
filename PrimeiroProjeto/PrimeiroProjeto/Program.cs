using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/products", (Product product) =>
{
    ProductRepository.Add(product);
    return Results.Created($"/products/{product.Code}", product);
});

app.MapGet("/products", () =>
{
    var products = ProductRepository.Products;

    if (products == null)
        return Results.NotFound();

    return Results.Ok(products);
});

app.MapGet("/products/{code}", ([FromRoute] int code) =>
{
    var product = ProductRepository.GetBy(code);

    if (product == null)
        return Results.NotFound();

    return Results.Ok(product);
});


app.MapPut("/products/{code}", ([FromRoute] int code, Product product) =>
{
    var productSaved = ProductRepository.GetBy(code);

    if (productSaved == null)
        return Results.NotFound();

    productSaved.Name = product.Name;

    return Results.Ok(productSaved);
});

app.MapDelete("/products/{code}", ([FromRoute] int code) =>
{
    var product = ProductRepository.GetBy(code);

    if (product == null)
        return Results.NotFound();

    ProductRepository.Remove(product);
    return Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration configuration) =>
{
    int PORT = Convert.ToInt32(configuration["database:port"]);
    string CONNECTION = configuration["database:connection"];

    return Results.Ok($"{CONNECTION}/{PORT}");
});


app.Run();

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ProductId { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public List<Tag> Tags { get; set; } 
}

public static class ProductRepository
{
    public static List<Product> Products { get; set; }

    public static void Init(IConfiguration configuration)
    {
        var products = configuration.GetSection("Products").Get<List<Product>>();

        Products = products;
    }

    public static void Add(Product product)
    {
        if (Products == null)
        {
            Products = new List<Product>();
        }

        Products.Add(product);
    }

    public static Product GetBy(int code)
    {
        var product = Products.FirstOrDefault(product => product.Code == code);

        return product;
    }

    public static void Remove(Product product)
    {
        Products.Remove(product);
    }
}

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(500).IsRequired(false);

        model.Entity<Product>()
            .Property(p => p.Name).HasMaxLength(120).IsRequired();

        model.Entity<Product>()
            .Property(p => p.Code).IsRequired();

        model.Entity<Category>()
            .Property(p => p.Name).HasMaxLength(30).IsRequired();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(@"Server=localhost;Database=Products;User Id=sa;Password=Sql@2022;MultipleActiveResultSets=true;Encrypt=YES;TrustServerCertificate=YES");
    }
}