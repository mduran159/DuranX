namespace Shopping.Web.Pages.Products
{
    [Authorize(Roles = Roles.Admin)]
    public class EditProductModel(IInventoryService inventoryService, IGoogleDriveService googleDriveService) : PageModel
    {
        [BindProperty]
        public ProductFormModel FormModel { get; set; } = new ProductFormModel()
        {
            IsEditMode = true,
            ButtonText = "Edit",
        };

        public async Task<IActionResult> OnGet(Guid id)
        {
            var productResponse = await inventoryService.GetProduct(id);
            if (productResponse == null)
            {
                return NotFound();
            }

            FormModel.Product = productResponse.Product;
            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Save product into drive
            var imageFilePath = await googleDriveService.UploadImageFileAsync(FormModel.ImageFile!, true);
            FormModel.Product.ImageFile = imageFilePath;

            //Update database
            await inventoryService.UpdateProduct(new UpdateProductRequest(FormModel.Product));

            return RedirectToPage("Products");
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            await inventoryService.DeleteProduct(FormModel.Product.Id);
            return RedirectToPage("Products");
        }
    }
}
