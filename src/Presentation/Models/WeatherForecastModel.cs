namespace Presentation.Models;

public class WeatherForecastModel
{
    public Guid guid { get; set; }

    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

    public int TemperatureF { get; set; }
}
