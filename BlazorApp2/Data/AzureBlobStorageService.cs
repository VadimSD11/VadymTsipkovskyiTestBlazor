using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace BlazorApp2.Data
{
    public class AzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
            await blobClient.UploadAsync(fileStream, blobHttpHeaders);

            return blobClient.Uri.ToString();
        }
    }
}
