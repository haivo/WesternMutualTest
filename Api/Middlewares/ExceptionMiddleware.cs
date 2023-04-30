using Api.ApiResults;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        /******************************************************************************/
        /*ExceptionMiddleware                                                         */
        /*Catch and log exception                                                     */
        /******************************************************************************/
        public async Task Invoke(HttpContext context, ILogger<ExceptionMiddleware> log)
        {
            try
            {
                await _next(context);
            }
            catch (JsonReaderException jex)
            {
                log.LogError(message: "Cannot deserialize request body", exception: jex);
                var apiResult = ApiResult.InvalidJson("Invalid JSON formatted.");
                await context.Response.WriteAsJsonAsync(apiResult);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "error during executing {Context}", context.Request.Path.Value);
                var response = context.Response;
                response.StatusCode = 500;
                var apiResult = ApiResult.Error();
                await response.WriteAsJsonAsync(apiResult);
            }
        }
    }
}
