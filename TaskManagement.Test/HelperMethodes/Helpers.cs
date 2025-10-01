using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
namespace TaskManagement.Tests.HelperMethodes
{
    public static class Helpers
    {

        public static Mock<IFormFile> CreateMockFile
            (string fileName = "task.jpg", string fileContent = "test content",
            long fileLength = 1024, string contentType = "image/gpeg" , string path = @"this\path")
        {
            var mockFile = new Mock<IFormFile>();
            var contentBytes = Encoding.UTF8.GetBytes(fileContent);
            var ms = new MemoryStream(contentBytes);

            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.ContentType).Returns(contentType);
            mockFile.Setup(_ => _.Length).Returns(fileLength);
            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken token) =>
                {
                    ms.Position = 0;
                    return ms.CopyToAsync(target, 81920, token);
                });

            return mockFile;
        }
    }
}
