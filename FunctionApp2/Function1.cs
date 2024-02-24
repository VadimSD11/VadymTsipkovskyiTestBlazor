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

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string recipientEmail = data?.recipientEmail;
            string fileName = data?.name;
            string blobUri = data?.blobUri;

            // Send email notification
            await SendEmail(recipientEmail, fileName, blobUri);

            return new OkResult();
        }
        public static async Task SendEmail(string recipientEmail, string fileName, string blobUri)
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
            var body = $"The file {fileName} is successfully uploaded {blobUri}";
            var message = new MailMessage(from, to) { Subject = subject, Body = body };

            // Send the email
            await smtpClient.SendMailAsync(message);
        }
    }
}
