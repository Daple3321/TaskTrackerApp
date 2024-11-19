using Newtonsoft.Json;

namespace TaskTrackerApp.DTO
{
    public class LoginResponse
    {
        [JsonProperty("token")]
        public string token { get; set; }

        [JsonProperty("expire")]
        public DateTime expire { get; set; }

        [JsonProperty("userId")]
        public string userId { get; set; }
    }
}
