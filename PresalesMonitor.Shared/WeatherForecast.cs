namespace PresalesMonitor.Shared
{
	partial class WeatherForecast
	{
		public DateTime Date => DateTimeStamp.ToDateTime();
		public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
	}
}