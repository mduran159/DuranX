using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Shopping.Web.Services
{
    public interface IGoogleDriveService
    {
        Task<string> UploadImageFileAsync(IFormFile imageFile, bool IsProductImage = false);
    }

    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _publicUrlBase;
        private readonly string _productsFolderId;

        public GoogleDriveService(IConfiguration configuration)
        {
            var credential = GoogleCredential.FromFile(configuration["GoogleDrive:JsonKeyPath"])
                .CreateScoped(DriveService.Scope.Drive);

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = configuration["GoogleDrive:ApplicationName"],
            });

            _publicUrlBase = configuration["GoogleDrive:PublicUrlBase"]!;

            _productsFolderId = configuration["GoogleDrive:ProductsFolderId"]!;
        }

        public async Task<string> UploadImageFileAsync(IFormFile imageFile, bool IsProductImage = false)
        {
            // Verifica que el archivo no sea nulo
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("El archivo proporcionado no es válido.");
            }

            // Obtén el tipo MIME del archivo
            var mimeType = imageFile.ContentType;
            var productImageId = Guid.NewGuid().ToString();

            // Crea metadatos para el archivo en Google Drive
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = productImageId,
                MimeType = mimeType,
            };

            if(IsProductImage)
                fileMetadata.Parents = new[] { _productsFolderId };

            // Inicializa la solicitud de carga de archivo
            string fileId;
            using (var stream = imageFile.OpenReadStream())
            {
                // Crea la solicitud para subir el archivo
                var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id";

                var uploadedFile = await request.UploadAsync();
                if (uploadedFile.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    throw new Exception("Error subiendo el archivo a Google Drive");
                }

                fileId = request.ResponseBody.Id;
            }

            // Configura el archivo para que sea accesible públicamente
            await _driveService.Permissions.Create(new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            }, fileId).ExecuteAsync();

            // Genera la URL pública del archivo
            return string.Format(_publicUrlBase, fileId);
        }
    }
}
