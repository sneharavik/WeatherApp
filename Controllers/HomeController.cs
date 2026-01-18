using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WeatherApp.Helpers;
using WeatherApp.Models;
using WeatherApp.Services;
using System.Security.Cryptography;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbService _db;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _weatherApiKey = "5796abbde9106b7da4febfae8c44c232";
        private readonly HttpClient _httpClient;
        

        public HomeController(DbService db, IHttpContextAccessor httpContext, HttpClient httpClient, IConfiguration configuration)
        {
            _db = db;
            _httpContext = httpContext;
            _httpClient = httpClient;
            _weatherApiKey = configuration["WeatherApiKey"];
        
        }

        public IActionResult Index() => View(); // Login page
       
        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Username and password are required.";
                return View();
            }

            // Hash password using SHA256 (matches DB storage)
            byte[] passwordHash;//= HashPasswordSHA256(password);
            using (SHA256 sha = SHA256.Create())
            {
                passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            string hex = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
           // Console.WriteLine(hex);
            // Call SP to authenticate
            int? userId = await _db.sp_AuthenticateUser1(username, hex);

            if (userId.HasValue)
            {
                // Successful login
                HttpContext.Session.SetInt32("UserId", userId.Value);
                return RedirectToAction("Weather", "Home");
              
            }
            else
            {
                ViewBag.Message = "Invalid username or password.";
                return View();
            }
        }

        private byte[] HashPasswordSHA256(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }


        public async Task<IActionResult> Weather1()
        {
           // if (_httpContext.HttpContext!.Session.GetInt32("UserId") == null)
                return RedirectToAction("History");

           // return View();
        }
        public async Task<IActionResult> Weather()
        {
          if (_httpContext.HttpContext!.Session.GetInt32("UserId") == null)
                return RedirectToAction("Weather");

            return View();
        }

        [HttpGet("Home/GetWeather")]
        public async Task<IActionResult> GetWeather(string city, string id)
        {
           string _weatherApiKey = "5796abbde9106b7da4febfae8c44c232";

           var url = $"https://api.openweathermap.org/data/2.5/find" +$"?q={city}&appid={_weatherApiKey}&units=metric";
                      
           var response = await _httpClient.GetAsync(url);
           

            if (!response.IsSuccessStatusCode)
                return NotFound(new { error = "City not found" });

            
            var json = await response.Content.ReadAsStringAsync();

         
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<WeatherApiResponse>(json, options);

            var weather = data?.list?.FirstOrDefault()?.main;

            if (weather == null)
                return StatusCode(500, "Invalid weather data");

            return Ok(new
            {
                humidity = weather.humidity,
                temp_min = weather.temp_min,
                temp_max = weather.temp_max
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveWeather([FromBody] SaveWeatherDto model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return BadRequest("Session expired");

            await _db.SaveWeatherRecord(
                userId.Value,
                model.City,
                model.Min,
                model.Max,
                model.Humidity
            );

            return Json(new { success = true });
        }

        //public IActionResult WeatherHistory()
        //{
        //    var records = _weatherService.GetWeatherHistory(); // fetch from DB
        //    return View(records);
        //}
        public async Task<IActionResult> History()
        {
            if (_httpContext.HttpContext!.Session.GetInt32("UserId") == null)
                return RedirectToAction("Index");

            var records = await _db.GetAllWeatherRecords();
            return View(records);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRecords([FromBody] List<WeatherRecord> RecordId)
        {
            var userId = _httpContext.HttpContext!.Session.GetInt32("UserId");
            if (userId == null || RecordId == null) return BadRequest();

            // Get original records for comparison
            var originals = (await _db.GetAllWeatherRecords()).ToDictionary(r => r.RecordId);

            foreach (var updated in RecordId)
            {
                if (originals.TryGetValue(updated.RecordId, out var original))
                {
                    var changes = new List<string>();
                    if (original.City != updated.City) changes.Add($"City: {original.City} → {updated.City}");
                    if (original.MinTemp != updated.MinTemp) changes.Add($"MinTemp: {original.MinTemp} → {updated.MinTemp}");
                    if (original.MaxTemp != updated.MaxTemp) changes.Add($"MaxTemp: {original.MaxTemp} → {updated.MaxTemp}");
                    if (original.Humidity != updated.Humidity) changes.Add($"Humidity: {original.Humidity} → {updated.Humidity}");

                    if (changes.Any())
                    {
                        await _db.UpdateWeatherRecord(updated.RecordId, updated.City, updated.MinTemp, updated.MaxTemp, updated.Humidity);
                        await _db.LogAuditChange(updated.RecordId, userId.Value, string.Join("; ", changes));
                    }
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSingleRecord([FromBody] WeatherUpdateDto model)
        {
            bool updated = await _db.UpdateWeatherRecord(model);
            return Json(new { success = updated });
        }
    }
}
