using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Shopping.Web.Models.Inventory;

public class ProductModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string Description { get; set; } = default!;

    [ValidateNever]
    public string ImageFile { get; set; } = default!;

    public decimal Price { get; set; }

    public static List<string> ProductCategory = new()
    {
        "Smart Phone",
        "Home Kitchen",
        "White Appliances",
        "Camera"
    };
}

//wrapper classes
public record GetProductsResponse(IEnumerable<ProductModel> Products);
public record GetProductByCategoryResponse(IEnumerable<ProductModel> Products);
public record GetProductByIdResponse(ProductModel Product);
public record SaveProductRequest(ProductModel Product);