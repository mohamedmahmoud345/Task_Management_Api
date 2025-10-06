using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace TaskManagement.Api.Exceptions
{
    /// <summary>
    /// Middleware to handle global exceptions in the application.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;
        private readonly IProblemDetailsService problemDetailsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger instance to log errors.</param>
        /// <param name="problemDetailsService">The service for writing Problem Details responses.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger , IProblemDetailsService problemDetailsService)
        {
            this.logger = logger; 
            this.problemDetailsService = problemDetailsService;
        }
        /// <summary>
        /// Handles the specified exception by logging it and writing a Problem Details response.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="exception">The unhandled exception.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> indicating if the exception was handled.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occured");

            httpContext.Response.StatusCode = (int)StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var errorResponse = new
            {
            StatusCode = httpContext.Response.StatusCode,
            Message = "Internal Server Error",
            Details = exception.Message
            };
            var json = JsonSerializer.Serialize(errorResponse);

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "an error occured",
                    Detail = exception.Message
                }
            });
        }
    }
}
