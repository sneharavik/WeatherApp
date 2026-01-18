namespace WeatherApp.Models
{
    
    public class WeatherApiResponse
    {
        public List<WeatherItem>? list { get; set; }
    }

    public class WeatherItem
    {
        public Main? main { get; set; }
    }

    public class Main
    {
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public int humidity { get; set; }
    }



}
