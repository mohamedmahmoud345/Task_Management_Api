
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManagement.Api.Exceptions;

namespace TaskManagement.Tests.ExceptionHandlers
{
    public class GlobalExceptionHandlerTest
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> logger;
        private readonly Mock<IProblemDetailsService> problemDetailsService;
        private readonly Mock<HttpContext> httpContext;
        private readonly Mock<HttpResponse> response;
        private readonly GlobalExceptionHandler handler;

        public GlobalExceptionHandlerTest()
        {
            this.logger = new Mock<ILogger<GlobalExceptionHandler>>();
            this.problemDetailsService = new Mock<IProblemDetailsService>();
            this.httpContext = new Mock<HttpContext>();
            this.response = new Mock<HttpResponse>();

            handler = new GlobalExceptionHandler(logger.Object, problemDetailsService.Object);
        }

        [Fact]
        public async Task TryHandleAsync_LogsException()
        {
            var exception = new InvalidOperationException();

            problemDetailsService.Setup(x => x.TryWriteAsync(It.IsAny<ProblemDetailsContext>())).ReturnsAsync(true);

            httpContext.Setup(x => x.Response.StatusCode).Returns(It.IsAny<int>());
            response.Setup(x => x.ContentType).Returns(It.IsAny<string>());
            await handler.TryHandleAsync(httpContext.Object , exception, default);

            logger.Verify(logger => 
            logger.Log(LogLevel.Error,
            It.IsAny<EventId>() ,
            It.Is<It.IsAnyType>((v, t) => true), exception , 
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }
    }
}
