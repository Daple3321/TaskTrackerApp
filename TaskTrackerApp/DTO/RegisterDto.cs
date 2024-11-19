using Newtonsoft.Json;

namespace TaskTrackerApp.DTO
{
    public class RegisterDto
    {
        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("role")]
        public string role { get; set; }
    }
}
