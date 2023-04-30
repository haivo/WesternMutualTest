using Api.ApiResults;
using Api.Core.Entities;
using Api.Extensions;
using Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<GuidContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.UseSerilog((context, logger) =>
{
    var appPath = AppDomain.CurrentDomain.BaseDirectory;
    var binIndex = appPath.IndexOf("bin");
    if (binIndex != -1)
        appPath = appPath.Substring(0, binIndex);

    var logFile = Path.Combine(appPath, "log", "log-.txt");
    logger.WriteTo.File(logFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31);
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// Handle global exception
app.UseMiddleware<ExceptionMiddleware>();


/*                                                                      
Create new GUID                                                 
*/

app.MapPost("/guid/{guid?}", async (string? guid, HttpContext context, GuidContext dbContext) =>
{
    //Hexa, Length = 32 and Uppercase
    Regex regEx = new Regex("^[0-9A-F]{32}$");
    if (!string.IsNullOrEmpty(guid) && !regEx.IsMatch(guid))
    {
        var apiResult = ApiResult.ValidationError("GUID is invalid format.");
        return Results.BadRequest(apiResult);
    }

    // Validate empty body
    var requestBody = await context.ReadRequestJsonAsync();
    if (string.IsNullOrEmpty(requestBody))
    {
        var apiResult = ApiResult.ValidationError("Metadata is required.");
        return Results.BadRequest(apiResult);
    }
    // Validate Invalid Json
    // Will catch JsonReaderException in ExceptionMiddleware
    var jsonData = JObject.Parse(requestBody);
    if (jsonData == null)
    {
        var apiResult = ApiResult.InvalidJson("Invalid JSON formatted.");
        return Results.BadRequest(apiResult);
    }
    // Require metadata
    var hasMetadata = jsonData?.Properties().Any(p => p.Name != "expire");
    if (hasMetadata != true)
    {
        var apiResult = ApiResult.ValidationError("Metadata is required.");
        return Results.BadRequest(apiResult);
    }

    // Validate Expire is datetime or just a number

    //Validate the expiration date of Guid
    long expire = 0;
    if (jsonData != null && jsonData.ContainsKey("expire"))
    {
        if (!long.TryParse(jsonData["expire"]!.ToString(), out expire))
        {
            var apiResult = ApiResult.ValidationError("Field 'expire' is not a valid date.");
            return Results.BadRequest(apiResult);
        }
        long currentDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (expire - currentDateTime < 0)
        {
            var apiResult = ApiResult.ValidationError("Field 'expire' was expired.");
            return Results.BadRequest(apiResult);
        }
    }

    //Check duplicate
    if (!string.IsNullOrWhiteSpace(guid))
    {
        var uuid = Guid.ParseExact(guid, "N");
        var guidMetadataFromDb = await dbContext.GetGuid(uuid);
        if (guidMetadataFromDb != null)
        {
            var apiResult = ApiResult.ValidationError("Cannot insert duplicate GUID.");
            return Results.BadRequest(apiResult);
        }
    }


    var uniqueId = string.IsNullOrEmpty(guid) ? Guid.NewGuid() : Guid.ParseExact(guid, "N");

    var guidMetadata = new GuidMetadata();
    //Prepare for response for Adding

    guidMetadata.Guid = uniqueId;
    guidMetadata.CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    guidMetadata.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    // Minify Json
    guidMetadata.Metadata = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
    guidMetadata.ExpiredAt = expire > 0 ? expire : DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();
    dbContext.GuidMedata.Add(guidMetadata);
    await dbContext.SaveChangesAsync();
    return Results.Content(guidMetadata.ToJsonString(), "application/json", Encoding.UTF8);
});
/*                                                                      
Get GUID                                                 
*/

app.MapGet("/guid/{guid}", async (string guid, GuidContext context) =>
{
    //Hexa, Length = 32 and Uppercase
    Regex regEx = new Regex("^[0-9A-F]{32}$");
    if (!regEx.IsMatch(guid))
    {
        var apiResult = ApiResult.ValidationError("GUID is invalid format.");
        return Results.BadRequest(apiResult);
    }
    var uniqueId = Guid.ParseExact(guid, "N");
    // Check existing GUID
    var guidMetadata = await context.GetGuid(uniqueId);
    if (guidMetadata == null)
    {
        var apiResult = ApiResult.ValidationError("This GUID cannot be found.");
        return Results.NotFound(apiResult);
    }
    return Results.Content(guidMetadata.ToJsonString(), "application/json", Encoding.UTF8);
});

/*                                                                      
Update GUID Metadata                                                 
*/

app.MapPut("/guid/{guid}", async (string guid, HttpContext context, GuidContext dbContext) =>
{
    //Hexa, Length = 32 and Uppercase
    Regex regEx = new Regex("^[0-9A-F]{32}$");
    if (string.IsNullOrWhiteSpace(guid) || !regEx.IsMatch(guid))
    {
        var apiResult = ApiResult.ValidationError("GUID is invalid format.");
        return Results.BadRequest(apiResult);
    }
    // Validate empty body
    var requestBody = await context.ReadRequestJsonAsync();
    if (string.IsNullOrEmpty(requestBody))
    {
        var apiResult = ApiResult.ValidationError("Metadata is required.");
        return Results.BadRequest(apiResult);
    }
    // Validate Invalid Json
    // Will catch JsonReaderException in ExceptionMiddleware
    var jsonData = JObject.Parse(requestBody);

    //Validate the expiration date of GUID
    long expire = 0;
    if (jsonData != null && jsonData.ContainsKey("expire"))
    {
        if (!long.TryParse(jsonData["expire"]!.ToString(), out expire))
        {
            var apiResult = ApiResult.ValidationError("Field 'expire' is not a valid date.");
            return Results.BadRequest(apiResult);
        }
        long currentDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (expire - currentDateTime < 0)
        {
            var apiResult = ApiResult.ValidationError("Field 'expire' was expired.");
            return Results.BadRequest(apiResult);
        }
    }
    var uniqueId = Guid.ParseExact(guid, "N");

    // Check existing GUID
    var guidMetadataFromDb = await dbContext.GetGuid(uniqueId);
    if (guidMetadataFromDb == null)
    {
        var apiResult = ApiResult.NotFound("This GUID cannot be found.");
        return Results.NotFound(apiResult);
    }
    // Don't allow update if GUID was expired.
    else if (guidMetadataFromDb.WasExpired())
    {
        var apiResult = ApiResult.ValidationError("This GUID was expired. You cannot update it.");
        return Results.BadRequest(apiResult);
    }

    if (guidMetadataFromDb != null)
    {
        guidMetadataFromDb.Metadata = jsonData?.ToString(Newtonsoft.Json.Formatting.None);
        guidMetadataFromDb.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        guidMetadataFromDb.ExpiredAt = expire;
        await dbContext.SaveChangesAsync();
    }
    return Results.Content(guidMetadataFromDb?.ToJsonString(), "application/json", Encoding.UTF8);
});


/*                                                                      
Delete GUID                                                 
*/
app.MapDelete("/guid/{guid}", async (string guid, GuidContext context) =>
{
    //Hexa, Length = 32 and Uppercase
    Regex regEx = new Regex("^[0-9A-F]{32}$");
    if (!regEx.IsMatch(guid))
    {
        var apiResult = ApiResult.ValidationError("GUID is invalid format.");
        return Results.BadRequest(apiResult);
    }
    var uniqueId = Guid.ParseExact(guid, "N");

    // Check existing GUID
    var guidMetadata = await context.GetGuid(uniqueId);
    if (guidMetadata == null)
    {
        var apiResult = ApiResult.ValidationError("This GUID cannot be found.");
        return Results.NotFound(apiResult);
    }
    context.GuidMedata.Remove(guidMetadata);
    await context.SaveChangesAsync();

    return Results.Ok();
});

await app.RunAsync();



