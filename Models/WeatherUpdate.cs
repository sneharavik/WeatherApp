namespace WeatherApp.Models
{
    public class WeatherUpdateDto
    {
        public int RecordId { get; set; }
        public string City { get; set; }
        public decimal MinTemp { get; set; }
        public decimal MaxTemp { get; set; }
        public int Humidity { get; set; }
    }
}
