namespace Shopping.Web.Pages.Products
{
    public class CreateProductModel(IInventoryService inventoryService, ILogger<ProductsModel> logger, IDropboxService dropboxService) 
        : PageModel
    {
        [BindProperty]
        public ProductModel Product { get; set; } = new ProductModel();

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        //this is loading from a static list instead of a stored data in a database because I don't want to spend too much time in this example code
        public List<string> CategoryList { get; set; } = ProductModel.ProductCategory;

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ImageFile is not null && ImageFile.FileName.EndsWith(".png"))
            {
                var imageFilePath = await dropboxService.UploadFileAsync(ImageFile, isPublicFile: true, IsRawImage: true);

                Product.ImageFile = imageFilePath;
                await inventoryService.SaveProduct(new SaveProductRequest(Product));

               return RedirectToPage("Products");

            }
            else
            {
                ModelState.AddModelError("ImageFile", "Not file selected or format is not an png."); //revisar y poner key en dile
                return Page();
            }
        }
    }
}
