using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FunctionApp2; 
using Newtonsoft.Json;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace TestProject3
{
    public class Function1Tests
    {
        [Fact]
        public async Task Run_ReturnsOkResult()
        {
            // Arrange
            var requestBody = new { recipientEmail = "test@example.com", name = "example.txt", blobUri = "https://example.blob.core.windows.net/container/example.txt" };
            var request = CreateHttpRequest(requestBody);
            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            // Act
            var response = await Function1.Run(request, logger) as OkResult;

            // Assert
            Assert.NotNull(response);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        }

        private static HttpRequest CreateHttpRequest(object requestBody)
        {
            var request = new DefaultHttpContext().Request;
            var serializedBody = JsonConvert.SerializeObject(requestBody);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedBody));
            request.Body = stream;
            return request;
        }
        [Fact]
        public async Task SendEmail_SendsEmail()
        {
            // Arrange
            var recipientEmail = "test@example.com";
            var fileName = "example.docx";
            var blobUriWithSas = "https://example.blob.core.windows.net/container/example.docx?sastoken";
            var smtpClientMock = new Mock<SmtpClient>("smtp.gmail.com")
            {
                CallBase = true
            };
            smtpClientMock.SetupAllProperties();

            // Act
            await Function1.SendEmail(recipientEmail, fileName, blobUriWithSas);

            // Assert
            Assert.False(smtpClientMock.Object.EnableSsl);
            Assert.Equal(25, smtpClientMock.Object.Port);
        }

        [Fact]
        public void GenerateSasToken_ReturnsValidSasToken()
        {
            // Arrange
            var blobUri = "https://example.blob.core.windows.net/container/example.docx";
            var expectedSasTokenPrefix = "?sv=";

            // Act
            var sasToken = Function1.GenerateSasToken(blobUri);

            // Assert
            Assert.StartsWith(expectedSasTokenPrefix, sasToken);
        }
    }
}
