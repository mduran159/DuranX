namespace Shopping.Web.Pages.Products
{
    [Authorize(Roles = Roles.Admin)]
    public class CreateProductModel(IInventoryService inventoryService, IGoogleDriveService googleDriveService) : PageModel
    {
        [BindProperty]
        public ProductFormModel FormModel { get; set; } = new ProductFormModel()
        {
            IsEditMode = false,
            ButtonText = "Create",
        };

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Upload file into drive
            var imageFilePath = await googleDriveService.UploadImageFileAsync(FormModel.ImageFile!, true);
            FormModel.Product.ImageFile = imageFilePath;

            //Save into database
            await inventoryService.SaveProduct(new SaveProductRequest(FormModel.Product));

            return RedirectToPage("Products");
        }
    }
}
