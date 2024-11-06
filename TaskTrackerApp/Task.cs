using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTrackerApp
{
    internal class Task
    {
        public string description = "No Description.";

        public enum TaskState
        {
            Done,
            InProgress,
            InDesign,
            None,
        }
        public TaskState State { get; set; }

        public int id;
        public TimeSpan createdAt;
        public TimeSpan updatedAt;

        public virtual void Print()
        {
            Console.WriteLine("Task: " + description + "\n" +
                "State: " + State.ToString() + "\n" +
                "Id: " + id + "\n" +
                "Created at: " + createdAt.Days + "\n" +
                "Updated at: " + updatedAt.Days + "\n");
        }

        public Task(int assignedId, string desc = null)
        { 
            description = desc;
            State = TaskState.InProgress;

            id = assignedId;
        }
    }
}
