

namespace TaskTrackerApp
{
    class Program
    {
        public static List<Task> tasks = new List<Task>();


        static void Main(string[] args)
        {
            Task test = new Task(1, "Some task...");

            tasks.Add(test);

            foreach (var task in tasks)
            {
                task.Print();
            }
        }
    }

}