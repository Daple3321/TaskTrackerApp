using Newtonsoft.Json;

namespace TaskTrackerApp.DTO
{
    public class ContentDto
    {
        [JsonProperty("content")]
        public string content { get; set; }
    }
}
