using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Shopping.Web.Models.Page;

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
}

//wrapper classes
public record GetProductsResponse(PaginatedResult<ProductModel> Products);
public record GetProductByCategoryResponse(PaginatedResult<ProductModel> Products);
public record GetProductByIdResponse(ProductModel Product);
public record SaveProductRequest(ProductModel Product);
public record UpdateProductRequest(ProductModel Product);