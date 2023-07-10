using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/products", (Product product) =>
{
    ProductRepository.Add(product);
    return Results.Created($"/products/{product.Code}", product);
});

app.MapGet("/products", ([FromQuery] string dateStart, [FromQuery] string dateEnd) =>
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


app.Run();

public class Product
{
    public int Code { get; set; }
    public string Name { get; set; }
}

public static class ProductRepository
{
    public static List<Product> Products { get; set; }

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