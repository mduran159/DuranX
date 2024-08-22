using Dropbox.Api;

namespace Shopping.Web.Services
{
    public interface IDropboxService
    {
        Task<string> UploadFileAsync(IFormFile imageFile, bool isPublicFile = false, bool IsRawImage = false);
        Task DownloadFileAsync(string dropboxPath, string localFilePath);
    }

    public class DropboxService : IDropboxService
    {
        private readonly string _accessToken;

        public DropboxService(string accessToken)
        {
            _accessToken = accessToken;
        }

        public async Task<string> UploadFileAsync(IFormFile imageFile, bool isPublicFile = false, bool IsRawImage = false)
        {
            var fileIdName = Guid.NewGuid().ToString();
            var localFilePath = Path.Combine(Path.GetTempPath(), imageFile.FileName);
            
            using (var stream = new FileStream(localFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            string dropboxPath = $"/Products/{fileIdName}.png";

            using (var dbx = new DropboxClient(_accessToken))
            {
                using (var fileStream = File.Open(localFilePath, FileMode.Open))
                {
                    var updated = await dbx.Files.UploadAsync(
                        dropboxPath,
                        Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                        body: fileStream);

                    Console.WriteLine($"Uploaded file: {updated.Name}");
                }

                File.Delete(localFilePath);

                if (isPublicFile)
                {
                    var sharedLinkMetadata = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(dropboxPath);
                    return IsRawImage ? sharedLinkMetadata.Url.Replace("dl=0", "raw=1") : sharedLinkMetadata.Url; 
                }
                else {
                    return dropboxPath;
                }
            }
        }

        public async Task DownloadFileAsync(string dropboxPath, string localFilePath)
        {
            using (var dbx = new DropboxClient(_accessToken))
            {
                var response = await dbx.Files.DownloadAsync(dropboxPath);

                using (var fileStream = File.Create(localFilePath))
                {
                    (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                }

                Console.WriteLine($"Downloaded file: {localFilePath}");
            }
        }
    }
}
