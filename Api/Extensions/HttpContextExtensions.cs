using System.Text;

namespace Api.Extensions
{
    public static class HttpContextExtensions
    {
        /******************************************************************************/
        /*ReadRequestJsonAsync extension method                                       */
        /*Read JsonString from body of a request                                      */
        /******************************************************************************/
        public static async Task<string?> ReadRequestJsonAsync(this HttpContext context)
        {
            if (context.Request.ContentType != null && !context.Request.ContentType.ToLower().Contains("application/json"))
                return null;

            if (!context.Request.Body.CanSeek)
            {
                context.Request.EnableBuffering();
            }
            context.Request.Body.Position = 0;
            var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            string body = await reader.ReadToEndAsync();
            return body;
        }
    }
}
