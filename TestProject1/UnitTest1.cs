using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Xunit;

namespace FunctionApp2.Tests
{
    public class Function1Tests
    {
        [Fact]
        public async Task Run_ReturnsOkResult()
        {
            // Arrange
            var request = new DefaultHttpContext().Request;
            var loggerMock = new Mock<ILogger>();
            var requestBody = new
            {
                recipientEmail = "test@example.com",
                name = "example.txt",
                blobUri = "https://example.blob.core.windows.net/container/example.txt"
            };
            var serializedBody = JsonConvert.SerializeObject(requestBody);
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.Write(serializedBody);
            writer.Flush();
            stream.Position = 0;
            request.Body = stream;

            // Act
            var result = await Function1.Run(request, loggerMock.Object);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SendEmail_SendsEmail()
        {
            // Arrange
            var recipientEmail = "test@example.com";
            var fileName = "example.txt";
            var blobUriWithSas = "https://example.blob.core.windows.net/container/example.txt?sastoken";
            var smtpClientMock = new Mock<SmtpClient>("smtp.gmail.com")
            {
                CallBase = true
            };
            smtpClientMock.SetupAllProperties();

            // Act
            await Function1.SendEmail(recipientEmail, fileName, blobUriWithSas);

            // Assert
            smtpClientMock.VerifySet(x => x.EnableSsl = true, Times.Once);
            smtpClientMock.VerifySet(x => x.Port = 587, Times.Once);
            smtpClientMock.VerifySet(x => x.Credentials = It.IsAny<System.Net.NetworkCredential>(), Times.Once);
            smtpClientMock.Verify(x => x.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
        }

        [Fact]
        public void GenerateSasToken_ReturnsValidSasToken()
        {
            // Arrange
            var blobUri = "https://example.blob.core.windows.net/container/example.txt";
            var expectedSasTokenPrefix = "?sv=";

            // Act
            var sasToken = Function1.GenerateSasToken(blobUri);

            // Assert
            Assert.StartsWith(expectedSasTokenPrefix, sasToken);
        }
    }
}
