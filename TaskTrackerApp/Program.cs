using Newtonsoft.Json;

namespace TaskTrackerApp
{
    class Program
    {
        public static List<Task> tasks = [];
        public static int taskCount;
        public static bool exitFlag = false;
        public static JsonWriter writer;
        public static string jsonTasks = "";

        static void Main(string[] args)
        {
            SetupConsole();
            SetupJsonWriter();



            while (true)
            {
                HandleCommands();

                if(exitFlag)
                {
                    break;
                }
            }
        }

        private static Dictionary<string, string> ParseArguments(string[] args)
        {
            var arguments= new Dictionary<string, string>();

            foreach (var arg in args)
            {
                string[] parts = arg.Split('=');

                if (parts.Length == 2)
                {
                    arguments[parts[0]] = parts[1];
                }
                else
                {
                    arguments[arg] = null;
                }

            }

            return arguments;
        }


        private static void LoadTasks()
        {
            
            JsonConvert.DeserializeObject(jsonTasks);
        }

        public static void CreateTask(string desc)
        {
            int newTaskId = taskCount++;
            Task newTask = new Task(newTaskId, desc);
            tasks.Add(newTask);

            string serializedTask = JsonConvert.SerializeObject(newTask);
            Console.WriteLine(serializedTask);

            writer.WriteStartObject();

            writer.WritePropertyName("Description");
            writer.WriteValue(desc);

            writer.WritePropertyName("State");
            writer.WriteValue(Task.TaskState.InProgress);

            writer.WritePropertyName("CreateDate");
            writer.WriteValue(newTask.createdAt);

            writer.WritePropertyName("UpdateDate");
            writer.WriteValue(newTask.updatedAt);

            writer.WritePropertyName("id");
            writer.WriteValue(newTask.id);

            writer.WriteEndObject();

            taskCount++;
        }

        public static void DeleteTask(int taskId)
        {

        }

        private static void HandleCommands()
        {
            string command = Console.ReadLine();
            switch (command)
            {
                case "create":
                    string descriptionToSet = Console.ReadLine();
                    CreateTask(descriptionToSet);
                    break;

                case "q":
                    Console.WriteLine("Exiting...");
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                    writer.Close();
                    exitFlag = true;
                    break;
                case "p":
                    if(tasks != null)
                    {
                        foreach(Task task in tasks)
                        {
                            task.Print();
                        }
                    }
                    break;
                case "help":
                    PrintHelp();
                    break;


                default:
                    Console.WriteLine("Unknown command.");
                break;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("p - print all tasks" + "\n" +
                "create - create new task" + "\n" +
                "q - exit" + "\n");
        }
        
        private static void SetupJsonWriter()
        {
            StreamWriter tasksFile = File.CreateText("tasks.json");
            tasksFile.AutoFlush = true;
            writer = new JsonTextWriter(tasksFile);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
            writer.WritePropertyName("Tasks");
            writer.WriteStartArray();
        }

        private static void SetupConsole()
        {
            Console.Title = "TASK TRACKER";
            Console.WindowHeight = 500;
            Console.WindowWidth = 500;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("---- TASK TRACKER APP ----");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            PrintHelp();
        }
    }

}