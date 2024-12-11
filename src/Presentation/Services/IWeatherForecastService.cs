using Presentation.Models;
using Refit;

namespace Presentation.Services;

public interface IWeatherForecastService
{
    [Get("/api/weatherforecasts")]
    Task<List<WeatherForecastModel>> GetAllWeatherForecastsAsync();
}
