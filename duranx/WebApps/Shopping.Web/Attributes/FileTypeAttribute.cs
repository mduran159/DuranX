using System.ComponentModel.DataAnnotations;

namespace Shopping.Web.Attributes
{
    public class FileTypeAttribute : ValidationAttribute
    {
        private readonly string[] _validTypes;

        public FileTypeAttribute(params string[] validTypes)
        {
            _validTypes = validTypes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!_validTypes.Contains(fileExtension))
                {
                    return new ValidationResult($"Only the following file types are allowed: {string.Join(", ", _validTypes)}");
                }
            }

            return ValidationResult.Success;
        }
    }
}
