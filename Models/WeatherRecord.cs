namespace WeatherApp.Models
{
    public class WeatherRecord
    {
        public int RecordId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty; // For display
        public string City { get; set; } = string.Empty;
        public decimal MinTemp { get; set; }
        public decimal MaxTemp { get; set; }
        public int Humidity { get; set; }
        public DateTime SearchDate { get; set; }
    }

   


}
