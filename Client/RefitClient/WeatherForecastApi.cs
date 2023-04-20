using Refit;
using SmallEco.DTO;

namespace SmallEco.Client.RefitClient;

public interface IWeatherForecastApi
{
  [Get("/WeatherForecast")]
  Task<List<WeatherForecast>> WeatherForecastAsync();
}
