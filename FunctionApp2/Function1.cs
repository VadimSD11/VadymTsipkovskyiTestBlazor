using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string recipientEmail = data?.recipientEmail;
            string fileName = data?.name;
            string blobUri = data?.blobUri;

            // Generate SAS token
            var sasToken = GenerateSasToken(blobUri);

            // Append SAS token to blob URI
            string blobUriWithSas = blobUri + sasToken;

            // Send email notification
            await SendEmail(recipientEmail, fileName, blobUriWithSas);

            return new OkResult();
        }

        public static async Task SendEmail(string recipientEmail, string fileName,  string blobUriWithSas)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("VadimSD11@gmail.com", "lfxecrlwpddquykl"),
                EnableSsl = true,
            };

            // Compose the email message
            var from = new MailAddress("VadimSD11@gmail.com", "Your Name");
            var to = new MailAddress(recipientEmail);
            var subject = "Blob Storage Update Notification";
            var body = $"The file {fileName} is successfully uploaded. Blob URI With SAS: {blobUriWithSas}";
            var message = new MailMessage(from, to) { Subject = subject, Body = body };

            // Send the email
            await smtpClient.SendMailAsync(message);
        }

        private static string GenerateSasToken(string blobUri)
        {
            // Parse the blob URI
            var blobUriBuilder = new UriBuilder(blobUri);
            var blobClient = new BlobClient(blobUriBuilder.Uri);

            // Define the SAS token permissions and validity period
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.Https
            };

            // Set the SAS token permissions (adjust as needed)
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the SAS token
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("vadymtsipkovskyinettest", "n0HHuJITXNc+nm1qYIBUUie8car67fEY62hpHsIQeYNy3uaOHATh5JKzRM9NNksjhk/RAQaa9+Ro+AStQxI5GA==")).ToString();

            return "?" + sasToken;
        }
    }
}
