using Microsoft.Data.SqlClient;
using System.Data;
using WeatherApp.Models;


namespace WeatherApp.Services
{
    public class DbService
    {
        private readonly string _connectionString;

        public DbService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        public async Task<int?> sp_AuthenticateUser1(string username, string hex)
        {
            // Convert hex string (0xABC...) to byte[]
            byte[] passwordHash = ConvertHexStringToByteArray(hex);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand("sp_AuthenticateUser1", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary).Value = passwordHash;

            var result = await cmd.ExecuteScalarAsync();
            return result == null ? (int?)null : Convert.ToInt32(result);
        }
        private static byte[] ConvertHexStringToByteArray(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);

            int length = hex.Length;
            byte[] bytes = new byte[length / 2];

            for (int i = 0; i < length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }


        //public async Task<int?> sp_AuthenticateUser1(string username, string hex)
        //{
        //    using var conn = new SqlConnection(_connectionString);
        //    await conn.OpenAsync();
        //    using var cmd = new SqlCommand("sp_AuthenticateUser1", conn);
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@Username", username);
        //    cmd.Parameters.AddWithValue("@PasswordHash", hex);

        //    var result = await cmd.ExecuteScalarAsync();
        //    return result == null ? (int?)null : Convert.ToInt32(result);
        //}
        //private async Task<int?> AuthenticateUser(string username, byte[] passwordHash)
        //{
        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    using (SqlCommand cmd = new SqlCommand("sp_AuthenticateUser", con))
        //    {
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@Username", username);
        //        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

        //        con.Open();
        //        var result = cmd.ExecuteScalar(); // Returns first column of first row (UserId)
        //        if (result != null)
        //            return Convert.ToInt32(result);
        //        else
        //            return null;
        //    }
        //}
        public async Task SaveWeatherRecord(int userId, string city, decimal min, decimal max, int humidity)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_SaveWeatherRecord", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@City", city);
            cmd.Parameters.AddWithValue("@MinTemp", min);
            cmd.Parameters.AddWithValue("@MaxTemp", max);
            cmd.Parameters.AddWithValue("@Humidity", humidity);
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<List<WeatherRecord>> GetAllWeatherRecords()
        {
            var records = new List<WeatherRecord>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_GetAllWeatherRecords", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(new WeatherRecord
                {
                RecordId = reader.GetInt32("RecordId"),
                Username = reader.GetString("Username"),
                City = reader.GetString("City"),
                MinTemp = reader.GetDecimal("MinTemp"),
                MaxTemp = reader.GetDecimal("MaxTemp"),
                Humidity = reader.GetInt32("Humidity"),
                SearchDate = reader.GetDateTime("SearchDate")
            });
        }
        return records;
    }

    public async Task UpdateWeatherRecord(int recordId, string city, decimal min, decimal max, int humidity)
{
    using var conn = new SqlConnection(_connectionString);
    await conn.OpenAsync();
    using var cmd = new SqlCommand("sp_UpdateWeatherRecord", conn);
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.Parameters.AddWithValue("@RecordId", recordId);
    cmd.Parameters.AddWithValue("@City", city);
    cmd.Parameters.AddWithValue("@MinTemp", min);
    cmd.Parameters.AddWithValue("@MaxTemp", max);
    cmd.Parameters.AddWithValue("@Humidity", humidity);
    await cmd.ExecuteNonQueryAsync();
}
        public async Task<bool> UpdateWeatherRecord(WeatherUpdateDto model)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_UpdateWeatherRecord", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecordId", model.RecordId);
            cmd.Parameters.AddWithValue("@City", model.City);
            cmd.Parameters.AddWithValue("@MinTemp", model.MinTemp);
            cmd.Parameters.AddWithValue("@MaxTemp", model.MaxTemp);
            cmd.Parameters.AddWithValue("@Humidity", model.Humidity);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task LogAuditChange(int recordId, int userId, string description)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("sp_LogAuditChange", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@RecordId", recordId);
            cmd.Parameters.AddWithValue("@ChangedByUserId", userId);
            cmd.Parameters.AddWithValue("@ChangeDescription", description);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
