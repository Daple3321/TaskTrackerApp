using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTrackerApp
{
    internal class Task
    {
        [JsonProperty("Description")]
        public string description = "No Description.";


        [JsonProperty("State")]
        public TaskState State { get; set; }
        [Flags]
        public enum TaskState
        {
            Done = 1,
            InProgress = 2,
            InDesign = 3,
            ToDo = 4,
            None = 0,
        }

        [JsonProperty("CreateDate")]
        public DateTime createdAt;
        [JsonProperty("UpdateDate")]
        public DateTime updatedAt;
        [JsonProperty("id")]
        public int id;

        public virtual void Print()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Task: " + description + "\n" +
                "State: " + State.ToString() + "\n" +
                "Id: " + id + "\n" +
                "Created at: " + createdAt + "\n" +
                "Updated at: " + updatedAt + "\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public Task(int assignedId, string desc = "")
        { 
            description = desc;
            State = TaskState.InProgress;

            createdAt = DateTime.Now;
            updatedAt = createdAt;

            id = assignedId;
        }
    }
}
