using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine;
using SystemTask =  System.Threading.Tasks;
using static TaskTrackerApp.Utils.Utils;
using static TaskTrackerApp.TrackerOperations;

namespace TaskTrackerApp
{
    class Program
    {
        public static List<Task> tasks = [];
        public static JsonWriter writer;
        public static bool exitFlag = false;

        public static string systemPath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData
                );
        public static string dirPath = Path.Combine(systemPath, "TaskTrackerApp");
        public static string filePath = Path.Combine(dirPath, "tasks.json");

        static async SystemTask.Task Main(string[] args)
        {
            //SetupConsole();
            LoadTasks();

            var taskIdOption = new Option<int>("--id")
            {
                Description = "Task ID",
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
            taskIdOption.AddAlias("-id");

            var nameOption = new Option<string[]>("--name")
            {
                Description = "Task name",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = true
            };
            nameOption.AddAlias("-n");

            var stateOption = new Option<Task.TaskState>("--state")
            {
                Description = "Task state",
                IsRequired = false
            };
            stateOption.AddAlias("-s");

            var createTask = new Command("create", "Creates a new task");
            createTask.AddOption(nameOption);
            createTask.AddAlias("c");
            createTask.SetHandler(CreateTask, nameOption);

            var deleteTask = new Command("delete", "Deletes task");
            deleteTask.AddOption(taskIdOption);
            deleteTask.AddAlias("del");
            deleteTask.SetHandler(DeleteTask, taskIdOption);

            var list = new Command("list", "Displays all tasks, you can filter tasks by state, use --state argument.");
            list.AddOption(stateOption);
            list.AddAlias("l");
            list.SetHandler(List, stateOption);

            var changeTask = new Command("change", "Changes the description of scpecified task");
            changeTask.AddOption(taskIdOption);
            changeTask.AddOption(nameOption);
            changeTask.AddAlias("chng");
            changeTask.SetHandler(UpdateTask, taskIdOption, nameOption);

            var markTask = new Command("mark", "Changes the state of scpecified task");
            markTask.AddOption(taskIdOption);
            markTask.AddOption(stateOption);
            markTask.AddAlias("m");
            markTask.SetHandler(ChangeTaskState, taskIdOption, stateOption);

            var clearTasks = new Command("clear", "Clears all tasks");
            clearTasks.SetHandler(ClearTasks);

            var dirCommand = new Command("directory", "Displays tasks.json directory");
            dirCommand.AddAlias("dir");
            dirCommand.SetHandler(() => Console.WriteLine("tasks.json directory: " + dirPath));

            RootCommand rootCommand = new("Simple task tracker app");
            rootCommand.AddCommand(createTask);
            rootCommand.AddCommand(deleteTask);
            rootCommand.Add(list);
            rootCommand.Add(changeTask);
            rootCommand.Add(markTask);
            rootCommand.Add(clearTasks);
            rootCommand.Add(dirCommand);
            rootCommand.SetHandler(MainCommand);

            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.AddMiddleware(async (context, next) =>
            {
                await next(context);
            });
            commandLineBuilder.UseDefaults();
            Parser parser = commandLineBuilder.Build();
            await parser.InvokeAsync(args);

            /*while (true)
            {
                await HandleCommands();

                if(exitFlag)
                {
                    break;
                }
            }*/
        }
        
        static async SystemTask.Task GenerateTasksAsync(int amount)
        {
            await SystemTask.Task.Run(() => GenerateTasks());
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

        public static void CreateTask(string[] desc)
        {
            string joinedString = "";
            foreach(string s in desc)
            {
                joinedString += " " + s;
            }

            Task newTask = new Task(0, joinedString);
            tasks.Add(newTask);
            ResetTaskIds();

            SaveTasks();
        }

        public static void ResetTaskIds()
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].id = i;
            }
        }

        public static void ChangeTaskState(int taskId, Task.TaskState state)
        {
            try
            {
                //Console.Write("Task Id: ");
                //taskId = Convert.ToInt32(Console.ReadLine());
                if (tasks.Exists(x=> x.id == taskId))
                {
                    tasks[taskId].State = state;
                    tasks[taskId].updatedAt = DateTime.Now;
                    SaveTasks();
                }
                else
                {
                    PrintError("No task found with that id.");
                }
            }
            catch
            {
                PrintError("Wrong ID input. Retry.");
                //ChangeTaskState(state);
            }
        } 

        public static void UpdateTask(int taskId, string[] desc)
        {
            string joinedString = "";
            foreach (string s in desc)
            {
                joinedString += " " + s;
            }
            try
            {
                //Console.Write("Task Id: ");
                //taskId = Convert.ToInt32(Console.ReadLine());
                if (tasks.Exists(x => x.id == taskId))
                {
                    //Console.Write("New description: ");
                    //desc = Console.ReadLine();
                    tasks[taskId].description = joinedString;
                    tasks[taskId].updatedAt = DateTime.Now;
                    SaveTasks();
                }
                else
                {
                    PrintError("No task found with that id.");
                }
            }
            catch
            {
                PrintError("Wrong ID input. Retry.");
                UpdateTask(taskId, desc);
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
                PrintError("No task found with id = " + taskId);
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

        public static void ClearTasks()
        {
            tasks.Clear();
            SaveTasks();
        }

        private static async SystemTask.Task HandleCommands()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            var command = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            switch (command)
            {
                case "create":
                    Console.Write("Description: ");
                    var descriptionToSet = Console.ReadLine();
                    //CreateTask(descriptionToSet);
                    break;
                case "update":
                    //UpdateTask();
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
                        PrintError("Wrong input. Write id of the task you want to delete.");
                    }
                    break;

                case "q":
                    PrintError("Exiting...");
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

                /*case "mark done":
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
                    break;*/

                case "generate":
                    await GenerateTasksAsync(50);
                    break;
                case "clear":
                    tasks.Clear();
                    SaveTasks();
                    break;
                case "dir":
                    Console.WriteLine("tasks.json directory: " + dirPath);
                    break;

                case "help":
                    PrintHelp();
                    break;


                default:
                    PrintError("Unknown command.");
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