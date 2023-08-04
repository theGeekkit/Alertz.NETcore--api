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

// Define the API endpoint for fetching weather alerts using HTTP GET
app.MapGet("/api/weather/weatheralerts", async (double? latitude, double? longitude) =>
{
    if (latitude == null || longitude == null)
    {
        return Results.BadRequest("Latitude and longitude must be provided.");
    }

    string pointsUrl = $"https://api.weather.gov/points/{latitude},{longitude}";

    using HttpClient httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
    HttpResponseMessage pointsResponse = await httpClient.GetAsync(pointsUrl);

    if (pointsResponse.IsSuccessStatusCode)
    {
        // Read the points response to get the forecastGridData link
        var pointsData = await pointsResponse.Content.ReadFromJsonAsync<PointsData>();
        if (pointsData != null)
        {
            string gridpointUrl = pointsData.properties.forecastGridData;

            HttpResponseMessage gridpointResponse = await httpClient.GetAsync(gridpointUrl);

            if (gridpointResponse.IsSuccessStatusCode)
            {
                // Read the gridpoint response to get weather alerts
                WeatherAlerts? weatherAlerts = await gridpointResponse.Content.ReadFromJsonAsync<WeatherAlerts>();
                if (weatherAlerts != null)
                {
                    return Results.Ok(weatherAlerts);
                }
                else
                {
                    return Results.BadRequest("Error while fetching weather alerts.");
                }
            }
            else
            {
                return Results.BadRequest("Error while fetching weather alerts gridpoint data.");
            }
        }
        else
        {
            return Results.BadRequest("Error while fetching weather alerts metadata.");
        }
    }
    else
    {
        return Results.BadRequest("Error while fetching weather alerts points data.");
    }
});

app.Run();

public class WeatherData
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class PointsData
{
    public PointProperties properties { get; set; }
}

public class PointProperties
{
    public string forecastGridData { get; set; }
}

public class WeatherAlerts
{
    // Define properties for weather alerts data here
    // For example: public string Title { get; set; }
    // You should map the JSON properties from the API to this class.
}
