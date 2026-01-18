namespace WeatherApp.Models
{
    public class SaveWeatherDto
    {
        public string City { get; set; } = string.Empty;
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public int Humidity { get; set; }
    }

}
