using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Shopping.Web.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Shopping.Web.Models.Inventory
{
    public class ProductFormModel
    {
        public ProductModel Product { get; set; } = new ProductModel();

        [Required(ErrorMessage = "Please upload a .png image file.")]
        [FileType(".png", ErrorMessage = "Only .png files are allowed.")]
        public IFormFile? ImageFile { get; set; }

        public List<string> CategoryList { get; } = ProductCategory;

        [ValidateNever]
        public required bool IsEditMode { get; set; }

        [ValidateNever]
        public required string ButtonText { get; set; }

        public static List<string> ProductCategory = new()
        {
            "Smart Phone",
            "Home Kitchen",
            "White Appliances",
            "Camera"
        };
    }
}
