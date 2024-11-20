using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApp.DTO
{
    public class UpdateTaskDto
    {
        [JsonProperty("newTask")]
        public Task newTask { get; set; }

        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("userId")]
        public string userId { get; set; }
    }
}
