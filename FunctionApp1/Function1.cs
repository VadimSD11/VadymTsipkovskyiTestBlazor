using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid;
using Microsoft.Azure.WebJobs;
using SendGrid.Helpers.Mail;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task Run(
        [BlobTrigger("vadymtsipkovskyinettest/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,
        string name,
        ILogger log)
        {
            // Use the recipient email obtained from the web app
            string recipientEmail = RecipientEmail;
            log.LogInformation($"Blob trigger function processed blob\n Name:{name} \n  BlobSize: {myBlob.Length} Bytes");

            // Replace with your logic to obtain recipientEmail
            string recipientEmail = "vadym.tsipkovskyi@nure.ua"; // For demonstration purposes

            // Retrieve your SendGrid API key from environment variables
            var sendGridApiKey = "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";

            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                log.LogError("SendGrid API key is missing. Check your Azure Function configuration.");
                return;
            }

            var sendGridClient = new SendGridClient(sendGridApiKey);

            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress("VadimSD11@gmail.com", "Your Name"));
            message.AddTo(recipientEmail, "Recipient Name");
            message.SetSubject("BLOB Uploaded Notification");

            var blobUriWithSas = GenerateSasTokenForBlob(name);
            message.AddContent(MimeType.Html, $"Your BLOB has been uploaded. Here is the link to download: {blobUriWithSas}");

            var response = await sendGridClient.SendEmailAsync(message);
            log.LogInformation($"Email sent. Status code: {response.StatusCode}");
        }

        private static string GenerateSasTokenForBlob(string blobName)
        {
            // Create a BlobServiceClient using your storage account connection string
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Define the container name
            var containerName = "vadymtsipkovskyinettest";

            // Define the permissions for the SAS token (e.g., read)
            var blobSasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1), // Set the expiry to 1 hour from now
            };

            // Set the permissions you want for the SAS token
            blobSasBuilder.SetPermissions(BlobSasPermissions.Read); // Adjust as needed

            // Generate the SAS token
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var sasToken = blobClient.GenerateSasUri(blobSasBuilder);

            return sasToken.ToString();
        }
    }
}
