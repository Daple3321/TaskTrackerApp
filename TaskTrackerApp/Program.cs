using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine;
using SystemTask =  System.Threading.Tasks;
using static TaskTrackerApp.Utils.Utils;
using static TaskTrackerApp.TrackerOperations;
using System.Net;
using TaskTrackerApp.DTO;
using System.Threading;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

// ----------
// This is a reworked version designed to work with TaskTracker-API
// ----------
namespace TaskTrackerApp
{
    class Program
    {
        public static bool exitFlag = false;
        // Old url http://localhost:5133
        public static string apiUrl = "http://192.168.0.10:5133";
        public static HttpClientHandler handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
        };
        public static HttpClient sharedClient = new();
        public static string AuthKey = "";
        public static string currentUserId = "";
        public static DateTime tokenExpiration;
        public static string currentUserName = "";

        public static CancellationTokenSource cancellationTokenSrc = new();

        public static JsonWriter writer;
        public static string systemPath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData
                );
        public static string dirPath = Path.Combine(systemPath, "TaskTrackerApp");
        public static string filePath = Path.Combine(dirPath, "authData.json");

        public static void LoadAuthData()
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
                writer.Close();

                AuthData newAuthData = new AuthData();
                string authDataJson = JsonConvert.SerializeObject(newAuthData, Formatting.Indented);
                File.WriteAllText(filePath, authDataJson);
                //writer.WriteStartObject();
                //writer.WriteStartArray();
                //writer.WritePropertyName("Tasks");

                //writer.WriteEndArray();

            }
            else
            {
                string authJson = File.ReadAllText(filePath);
                AuthData authData = JsonConvert.DeserializeObject<AuthData>(authJson);
                AuthKey = authData.token;
                currentUserId = authData.userId;
                tokenExpiration = authData.expire;
                sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{AuthKey}");
            }

            //string authDataJson = JsonConvert.SerializeObject(AuthData, Formatting.Indented);
            //File.WriteAllText(filePath, authDataJson);

        }

        static async SystemTask.Task Main(string[] args)
        {
            LoadAuthData();

            sharedClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            sharedClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "PostmanRuntime/7.42.0");
            sharedClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            //sharedClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");
            //sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", $"Bearer {AccesKey}");

            var authTokenOption = new Option<string>("--authToken")
            {
                Description = "Auth JWT token.",
                IsRequired = false,
                AllowMultipleArgumentsPerToken = false
            };
            authTokenOption.SetDefaultValue(AuthKey);

            var tokenOption = new Option<CancellationToken>("--token")
            {
                Description = "Cancellation token",
                IsRequired = false
            };
            tokenOption.AddAlias("-t");
            tokenOption.SetDefaultValue(cancellationTokenSrc.Token);

            var numberOption = new Option<int>("--number")
            {
                Description = "Amount of",
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
            numberOption.AddAlias("-num");

            var taskIdOption = new Option<int>("--id")
            {
                Description = "Task ID",
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
            taskIdOption.AddAlias("-id");

            var nameOption = new Option<string[]>("--taskName")
            {
                Description = "Task name",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = true
            };
            nameOption.AddAlias("-n");

            var changeNameOption = new Option<string[]>("--changeName")
            {
                Description = "Task name",
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };
            changeNameOption.AddAlias("-n");

            var stateOption = new Option<Task.TaskState>("--state")
            {
                Description = "Task state",
                IsRequired = false
            };
            stateOption.AddAlias("-s");

            var emailOption = new Option<string>("--email")
            { 
                Description = "Your email.",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = false
            };
            emailOption.AddAlias("-e");

            var passwordOption = new Option<string>("--password")
            {
                Description = "Your password.",
                IsRequired = true,
                AllowMultipleArgumentsPerToken = false
            };
            passwordOption.AddAlias("-p");

            var roleOption = new Option<string>("--role")
            {
                Description = "Role you want to be assigned to.",
                IsRequired = false,
                AllowMultipleArgumentsPerToken = false
            };
            roleOption.AddAlias("-r");

            var expireOption = new Option<DateTime>("--expire")
            {
                Description = "Token expiration date.",
                IsRequired = false,
                AllowMultipleArgumentsPerToken = false
            };
            expireOption.SetDefaultValue(DateTime.UtcNow.AddDays(7));
            expireOption.AddAlias("-exp");

            var regiesterCommand = new Command("register", "Registers a new user");
            regiesterCommand.AddOption(emailOption);
            regiesterCommand.AddOption(passwordOption);
            regiesterCommand.AddOption(roleOption);
            regiesterCommand.AddOption(tokenOption);
            regiesterCommand.AddAlias("reg");
            regiesterCommand.SetHandler(Register, emailOption, passwordOption, roleOption, tokenOption);

            var loginCommand = new Command("login", "Logs in as user");
            loginCommand.AddOption(emailOption);
            loginCommand.AddOption(passwordOption);
            loginCommand.AddOption(expireOption);
            loginCommand.AddOption(tokenOption);
            loginCommand.AddAlias("log");
            loginCommand.SetHandler(Login, emailOption, passwordOption, expireOption, tokenOption);

            var createTask = new Command("create", "Creates a new task");
            createTask.AddOption(nameOption);
            createTask.AddAlias("c");
            createTask.SetHandler(CreateTask, nameOption, tokenOption);

            var deleteTask = new Command("delete", "Deletes task");
            deleteTask.AddOption(taskIdOption);
            deleteTask.AddAlias("del");
            deleteTask.SetHandler(DeleteTask, taskIdOption);

            var list = new Command("list", "Displays all tasks, you can filter tasks by state, use --state argument.");
            //list.AddOption(stateOption);
            list.AddOption(tokenOption);
            list.AddAlias("l");
            list.SetHandler(PrintTasks, tokenOption);

            var changeTask = new Command("change", "Changes the description of scpecified task");
            changeTask.AddOption(taskIdOption);
            changeTask.AddOption(changeNameOption);
            changeTask.AddOption(stateOption);
            changeTask.AddAlias("ch");
            changeTask.SetHandler(UpdateTask, taskIdOption, changeNameOption, stateOption);

            var markTask = new Command("mark", "Changes the state of scpecified task");
            markTask.AddOption(taskIdOption);
            markTask.AddOption(stateOption);
            markTask.AddAlias("m");
            markTask.SetHandler(ChangeTaskState, taskIdOption, stateOption);

            var generateTasks = new Command("generate", "Generates a set amount of tasks");
            generateTasks.AddAlias("gen");
            generateTasks.AddOption(numberOption);
            generateTasks.SetHandler(GenerateTasksAsync, numberOption);

            RootCommand rootCommand = new("Simple task tracker app");
            rootCommand.AddCommand(loginCommand);
            rootCommand.AddCommand(regiesterCommand);
            rootCommand.AddCommand(createTask);
            rootCommand.AddCommand(deleteTask);
            rootCommand.AddCommand(list);
            rootCommand.AddCommand(changeTask);
            rootCommand.AddCommand(markTask);
            rootCommand.AddCommand(generateTasks);
            rootCommand.SetHandler(MainCommand);

            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.AddMiddleware(async (context, next) =>
            {
                await next(context);
            });
            commandLineBuilder.UseDefaults();
            Parser parser = commandLineBuilder.Build();
            await parser.InvokeAsync(args);
        }
        
        public static async SystemTask.Task Register(string _email, string _password, string _role, CancellationToken token)
        {
            RegisterDto form = new RegisterDto { email = _email, password = _password, role = _role };

            string jsonForm = JsonConvert.SerializeObject(form, Formatting.Indented);

            string uri = $"{apiUrl}/accounts/register";

            HttpResponseMessage response = new();
            try
            {
                response = await sharedClient.PostAsync(uri, 
                    new StringContent(jsonForm, Encoding.UTF8, MediaTypeNames.Application.Json) ,
                    token).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine(e.Message);
                    cancellationTokenSrc.Cancel();
                }
            }
            //token.ThrowIfCancellationRequested();
            
            string jsonResponse = await response.Content.ReadAsStringAsync();
            //return jsonResponse;
            if(response.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("New account created! " + response.StatusCode);
                //return "New account created! " + response.StatusCode;
            }
            else
            {
                Console.WriteLine(response.StatusCode + "\n Couldn't create new account.");
                //return response.StatusCode + "\n Couldn't create new account.";
            }
        }
        
        public static async Task<LoginResponse> Login(string _email, string _password, DateTime _expire, CancellationToken token)
        {
            LoginDto form = new LoginDto { email = _email, password = _password, expire = _expire };

            string jsonForm = JsonConvert.SerializeObject(form, Formatting.Indented);

            string uri = $"{apiUrl}/accounts/login";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Content = new StringContent(
                    jsonForm,
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
            };


            HttpResponseMessage response = new();
            try
            {
                response = await sharedClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                cancellationTokenSrc.Cancel();
            }
            token.ThrowIfCancellationRequested();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            LoginResponse loginResponse = new LoginResponse();
            loginResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonResponse);

            AuthKey = loginResponse.token;
            currentUserId = loginResponse.userId;

            sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", $"Bearer {AuthKey}");

            AuthData newAuthData = new AuthData { token = AuthKey, expire = loginResponse.expire, userId = currentUserId};
            string authDataJson = JsonConvert.SerializeObject(newAuthData, Formatting.Indented);
            File.WriteAllText(filePath, authDataJson);

            Console.WriteLine("Token: " + AuthKey);
            Console.WriteLine("UserId: " + currentUserId);

            return loginResponse;
        }

        static async SystemTask.Task GenerateTasksAsync(int amount)
        {
            await SystemTask.Task.Run(() => GenerateTasks(amount));
        }
        public static void GenerateTasks(int amount)
        {
            
        }

        /*private static void LoadTasks()
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
        }*/

        public async static Task<string?> CreateTask(string[] desc, CancellationToken token)
        {
            LoadAuthData();
            string uri = $"{apiUrl}/task";

            string joinedString = "";
            foreach(string s in desc)
            {
                joinedString += s + " ";
            }

            Task newTask = new Task { 
                description = joinedString, 
                createdAt = DateTime.Now, 
                updatedAt = DateTime.Now, 
                userId = currentUserId, 
                State = Task.TaskState.InProgress };

            string jsonTask = JsonConvert.SerializeObject(newTask, Formatting.Indented);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Content = new StringContent(
                    jsonTask,
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
            };


            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };
            try
            {
                response = await sharedClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                cancellationTokenSrc.Cancel();
            }
            //token.ThrowIfCancellationRequested();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            //Console.WriteLine(jsonResponse);
            return jsonResponse;
        }

        public async static Task<string?> GetAllTasks(CancellationToken token)
        {
            LoadAuthData();
            string uri = $"{apiUrl}/task/getAll";

            ContentDto contentDto = new();
            contentDto.content = currentUserId;
            string jsonUserId = JsonConvert.SerializeObject(contentDto, Formatting.Indented);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Content = new StringContent(
                    jsonUserId,
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
            };


            HttpResponseMessage response = new();
            try
            {
                response = await sharedClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                cancellationTokenSrc.Cancel();
            }
            //token.ThrowIfCancellationRequested();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            //Console.WriteLine(jsonResponse);
            return jsonResponse;
        }

        public static async SystemTask.Task PrintTasks(CancellationToken token)
        {
            List<Task> tasks = new List<Task>();
            string tasksJson = await GetAllTasks(token).ConfigureAwait(false);

            tasks = JsonConvert.DeserializeObject<List<Task>>(tasksJson);

            foreach(Task task in tasks)
            {
                PrintTask(task);
            }
        }

        public static void PrintTask(Task t)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Task: ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(t.description + "\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("State: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(t.State + "\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Id: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(t.id + "\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Created at: ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(t.createdAt + "\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Updated at: ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(t.updatedAt + "\n");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("------------------------------" + "\n");
        }

        /*public static void ResetTaskIds()
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].id = i;
            }
        }*/

        public static void ChangeTaskState(int taskId, Task.TaskState state)
        {
            /*try
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
            }*/
        } 

        public static async Task<string?> UpdateTask(int taskId, string[] desc, Task.TaskState state)
        {
            LoadAuthData();
            string uri = $"{apiUrl}/task/update";

            string joinedString = "";
            foreach (string s in desc)
            {
                joinedString += s + " ";
            }

            UpdateTaskDto updatedTask = new();
            updatedTask.userId = currentUserId;
            updatedTask.Id = taskId;
            updatedTask.newTask = new Task { 
                userId = currentUserId, 
                updatedAt = DateTime.Now, 
                description = joinedString, 
                State = state 
            };
            string updatedTaskJson = JsonConvert.SerializeObject(updatedTask, Formatting.Indented);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(uri),
                Content = new StringContent(
                    updatedTaskJson,
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
            };


            HttpResponseMessage response = new();
            try
            {
                response = await sharedClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                cancellationTokenSrc.Cancel();
            }
            //token.ThrowIfCancellationRequested();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonResponse);
            return jsonResponse;
        }

        public static async Task<string?> DeleteTask(int taskId)
        {
            LoadAuthData();
            string uri = $"{apiUrl}/task/delete";

            DeleteTaskDto deleteDto = new();
            deleteDto.Id = taskId;
            deleteDto.userId = currentUserId;
            string jsonDeleteBody = JsonConvert.SerializeObject(deleteDto, Formatting.Indented);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(uri),
                Content = new StringContent(
                    jsonDeleteBody,
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json),
            };

            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };
            try
            {
                response = await sharedClient.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
                cancellationTokenSrc.Cancel();
            }
            //token.ThrowIfCancellationRequested();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine(jsonResponse);
            return jsonResponse;
        }

        /*public static void SaveTasks()
        {
            string serializedTasks = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            //Console.WriteLine(serializedTasks);
            File.WriteAllText(filePath, serializedTasks);
        }*/

        public static void List(Task.TaskState state = 0)
        {
            /*switch (state)
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
            }*/
        }

        /*public static void ClearTasks()
        {
            tasks.Clear();
            SaveTasks();
        }*/
    }
}