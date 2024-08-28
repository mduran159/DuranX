namespace Shopping.Web.Models.Page.Products
{
    public interface IProductItemPartialView
    {
        Task<IActionResult> OnPostAddToCartAsync(Guid productId);
    }
}
