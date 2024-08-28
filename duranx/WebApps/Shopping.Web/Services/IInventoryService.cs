﻿namespace Shopping.Web.Services;

public interface IInventoryService
{
    [Get("/inventory-service/products?pageNumber={pageNumber}&pageSize={pageSize}")]
    Task<GetProductsResponse> GetProducts(int? pageNumber = 1, int? pageSize = 10);
    
    [Get("/inventory-service/products/{id}")]
    Task<GetProductByIdResponse> GetProduct(Guid id);
    
    [Get("/inventory-service/products/category/{category}")]
    Task<GetProductByCategoryResponse> GetProductsByCategory(string category);

    [Post("/inventory-service/products")]
    Task SaveProduct(SaveProductRequest request);

    [Put("/inventory-service/products")]
    Task UpdateProduct(UpdateProductRequest request);

    [Delete("/inventory-service/products/{id}")]
    Task DeleteProduct(Guid id);
}
