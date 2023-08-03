using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add this line to enable OpenAPI (Swagger) support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather API", Version = "v1" });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient(); // Add this line to register IHttpClientFactory

var app = builder.Build();

app.UseHttpsRedirection();

// Add this line to enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API V1");
    c.RoutePrefix = "swagger";
});

app.MapPost("/api/weather/weatheralerts", async context =>
{
    WeatherData? data = await context.Request.ReadFromJsonAsync<WeatherData>();
    if (data is not null)
    {
        double latitude = data.Latitude ?? 0.0;
        double longitude = data.Longitude ?? 0.0;

        string apiUrl = $"https://api.weather.gov/alerts?point={latitude},{longitude}";

        using HttpClient httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
HttpResponseMessage? response = await httpClient.GetAsync(apiUrl);
if (response is not null && response.IsSuccessStatusCode)
{
    WeatherAlerts? weatherAlerts = await response.Content.ReadFromJsonAsync<WeatherAlerts>();
    if (weatherAlerts is not null)
    {
        await context.Response.WriteAsJsonAsync(weatherAlerts);
    }
    else
    {
        // Handle the case when the JSON deserialization fails or the data is null
        // For example, return a bad request response
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Error while fetching weather alerts.");
    }
}
else
{
    // Handle the case when the HTTP request fails
    context.Response.StatusCode = 400;
    await context.Response.WriteAsync("Error while fetching weather alerts.");
}
    }
});

app.Run();

public class WeatherData
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class WeatherAlerts
{
    // Define properties for weather alerts data here
    // For example: public string Title { get; set; }
    // You should map the JSON properties from the API to this class.
}
