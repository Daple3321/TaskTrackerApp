using Newtonsoft.Json;

namespace TaskTrackerApp.DTO
{
    public class DeleteTaskDto
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("userId")]
        public string userId { get; set; }
    }
}
