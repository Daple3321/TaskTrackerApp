using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrackerApp.Utils;

namespace TaskTrackerApp
{
    class Program
    {
        public static List<Task> tasks = [];
        public static bool exitFlag = false;
        public static JsonWriter writer;

        public static string systemPath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData
                );
        public static string dirPath = Path.Combine(systemPath, "TaskTrackerApp");
        public static string filePath = Path.Combine(dirPath, "tasks.json");

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            //Console.WriteLine("Dir path: " + dirPath);
            SetupConsole();
            LoadTasks();

            while (true)
            {
                await HandleCommands();

                if(exitFlag)
                {
                    break;
                }
            }
        }
        
        static async System.Threading.Tasks.Task GenerateTasksAsync(int amount)
        {
            await System.Threading.Tasks.Task.Run(() => GenerateTasks());
        }
        public static void GenerateTasks()
        {
            for (int i = 0; i < 50; i++)
            {
                tasks.Add(new Task(0, "GeneratedTask"));
            }
            ResetTaskIds();
            SaveTasks();
        }

        private static void LoadTasks()
        {
            if (!Directory.Exists(dirPath)) // If directory doesn't exist
            {
                Console.WriteLine("New folder created at: " + dirPath);
                Directory.CreateDirectory(dirPath);
            }
            if (!Path.Exists(filePath)) // If .json file doesn't exist
            {
                // Create a new json file
                StreamWriter tasksFile = File.CreateText(filePath);
                tasksFile.AutoFlush = true;
                writer = new JsonTextWriter(tasksFile);
                writer.Formatting = Formatting.Indented;

                //writer.WriteStartObject();
                writer.WriteStartArray();
                //writer.WritePropertyName("Tasks");

                writer.WriteEndArray();
                writer.Close();

            }
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                tasks = JsonConvert.DeserializeObject<List<Task>>(json);
            }
        }

        public static void CreateTask(string desc)
        {
            Task newTask = new Task(0, desc);
            tasks.Add(newTask);
            ResetTaskIds();

            SaveTasks();

            /*writer.WriteStartObject();
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
            writer.WriteEndObject();*/

        }

        public static void ResetTaskIds()
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].id = i;
            }
        }

        public static void ChangeTaskState(Task.TaskState state)
        {
            int taskId = 0;
            try
            {
                Console.Write("Task Id: ");
                taskId = Convert.ToInt32(Console.ReadLine());
                if (tasks.Exists(x=> x.id == taskId))
                {
                    tasks[taskId].State = state;
                    tasks[taskId].updatedAt = DateTime.Now;
                    SaveTasks();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No task found with that id.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong ID input. Retry.");
                Console.ForegroundColor = ConsoleColor.White;
                ChangeTaskState(state);
            }
        } 

        public static void UpdateTask()
        {
            int taskId = 0;
            try
            {
                Console.Write("Task Id: ");
                taskId = Convert.ToInt32(Console.ReadLine());
                if (tasks.Exists(x => x.id == taskId))
                {
                    Console.Write("New description: ");
                    string desc = Console.ReadLine();
                    tasks[taskId].description = desc;
                    tasks[taskId].updatedAt = DateTime.Now;
                    SaveTasks();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No task found with that id.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong ID input. Retry.");
                Console.ForegroundColor = ConsoleColor.White;
                UpdateTask();
            }
        }

        public static void DeleteTask(int taskId)
        {
            if (tasks.Exists(task => task.id == taskId))
            {
                tasks.RemoveAt(taskId);
                ResetTaskIds();

                string serializedTasks = JsonConvert.SerializeObject(tasks, Formatting.Indented);
                File.WriteAllText(filePath, serializedTasks);
            }
            else
            {
                Console.WriteLine("No task found with id = " + taskId);
                return;
            }
        }

        public static void SaveTasks()
        {
            string serializedTasks = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            //Console.WriteLine(serializedTasks);
            File.WriteAllText(filePath, serializedTasks);
        }

        public static void List(Task.TaskState state = 0)
        {
            switch (state)
            {
                case 0:
                    if (tasks.Count > 0)
                    {
                        foreach (Task task in tasks)
                        {
                            task.Print();
                        }
                    }
                    else
                    {
                        Console.WriteLine("No tasks. Type 'create' to create a new task");
                    }
                break;
                case Task.TaskState.Done:
                    if (tasks.Exists(x=> x.State == Task.TaskState.Done))
                    {
                        foreach (Task task in tasks)
                        {
                            if(task.State == Task.TaskState.Done)
                            {
                                task.Print();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No DONE tasks found");
                    }
                break;
                case Task.TaskState.InProgress:
                    if (tasks.Exists(x => x.State == Task.TaskState.InProgress))
                    {
                        foreach (Task task in tasks)
                        {
                            if (task.State == Task.TaskState.InProgress)
                            {
                                task.Print();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No IN PROGRESS tasks found.");
                    }
                    break;
                case Task.TaskState.InDesign:
                    if (tasks.Exists(x => x.State == Task.TaskState.InDesign))
                    {
                        foreach (Task task in tasks)
                        {
                            if (task.State == Task.TaskState.InDesign)
                            {
                                task.Print();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No IN DESIGN tasks found.");
                    }
                    break;
                case Task.TaskState.ToDo:
                    if (tasks.Exists(x => x.State == Task.TaskState.ToDo))
                    {
                        foreach (Task task in tasks)
                        {
                            if (task.State == Task.TaskState.ToDo)
                            {
                                task.Print();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No To-Do tasks found.");
                    }
                    break;
            }
        }

        private static async System.Threading.Tasks.Task HandleCommands()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            string command = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            switch (command)
            {
                case "create":
                    Console.Write("Description: ");
                    string descriptionToSet = Console.ReadLine();
                    CreateTask(descriptionToSet);
                    break;
                case "update":
                    UpdateTask();
                    break;
                case "delete":
                    Console.Write("Task id: ");
                    try
                    {
                        int taskId = Convert.ToInt32(Console.ReadLine());
                        DeleteTask(taskId);
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong input. Write id of the task you want to delete.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;

                case "q":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exiting...");
                    Console.ForegroundColor = ConsoleColor.White;
                    exitFlag = true;
                    break;
                case "list":
                    List();
                    break;
                case "list done":
                    List(Task.TaskState.Done);
                    break;
                case "list in-progress":
                    List(Task.TaskState.InProgress);
                    break;
                case "list in-design":
                    List(Task.TaskState.InDesign);
                    break;
                case "list to-do":
                    List(Task.TaskState.ToDo);
                    break;

                case "mark done":
                    ChangeTaskState(Task.TaskState.Done);
                    break;
                case "mark in-progress":
                    ChangeTaskState(Task.TaskState.InProgress);
                    break;
                case "mark in-design":
                    ChangeTaskState(Task.TaskState.InDesign);
                    break;
                case "mark to-do":
                    ChangeTaskState(Task.TaskState.ToDo);
                    break;

                case "generate":
                    await GenerateTasksAsync(50);
                    break;
                case "clear":
                    tasks.Clear();
                    SaveTasks();
                    break;

                case "help":
                    PrintHelp();
                    break;


                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown command.");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }

        private static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("list - prints all tasks" + "\n" +
                "list done - prints all done tasks" + "\n" +
                "list in-progress - prints all in-progress tasks" + "\n" +
                "list in-design - prints all in-design tasks" + "\n" +
                "list to-do - prints all to-do tasks" + "\n\n" +

                "mark done - marks task as done" + "\n" +
                "mark in-progress - marks task as in-progress" + "\n" +
                "mark in-design - marks task as in-design" + "\n" +
                "mark to-do - marks task as to-do" + "\n\n" +

                "create - create new task" + "\n" +
                "delete - delete task with specified ID" + "\n" +
                "update - changes task description" + "\n" +
                "help - show this message" + "\n" +
                "q - exit" + "\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void SetupConsole()
        {
            Console.Title = "TASK TRACKER";
            //Console.WindowHeight = 500;
            //Console.WindowWidth = 500;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("---- TASK TRACKER APP ----");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            PrintHelp();
        }
    }

}