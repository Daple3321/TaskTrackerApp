using Newtonsoft.Json;

namespace TaskTrackerApp.DTO
{
    public class LoginDto
    {
        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("expire")]
        public DateTime expire { get; set; }
    }
}
