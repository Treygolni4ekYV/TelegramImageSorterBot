using TelegramImageSorterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using TelegramImageSorterBot.Models.Log;
using TelegramImageSorterBot.db;
using TelegramImageSorterBot.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace TelegramImageSorterBot
{
    internal class Program
    {
        static readonly string saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images");
        static readonly string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");

        static async Task Main(string[] args)
        {
            Startup.Init(args);
            var logService = Startup.ServiceProvider.GetRequiredService<ILogService>();
            var botService = Startup.ServiceProvider.GetRequiredService<ITelegramBotService>();
            var db = Startup.ServiceProvider.GetRequiredService<BotContext>();

            logService.Log("Bot configuring");

            //creating required directories(if don't exist)
            if (!Directory.GetDirectories(Directory.GetCurrentDirectory()).Contains("Images"))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            if (!Directory.GetDirectories(Directory.GetCurrentDirectory()).Contains("Logs"))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            //enable logging
            logService.StartLog();

            //starting bot
            if (!botService.Start(saveDirectory))
            {
                Console.WriteLine("Bot stopped", LogMessageType.notification);
                return;
            }
            
            logService.Log("Bot start");
            Console.WriteLine("Command mode");
            logService.Log("Use \"stop\" to stop bot");

            //stop command
            while (true)
            {
                var command = Console.ReadLine();
                if(string.IsNullOrEmpty(command))
                {
                    continue;
                }
                var lowerCommand = command.ToLower();


                //stop command
                if (lowerCommand.Contains(ConsoleCommands.Stop))
                {
                    logService.StopLog();
                    return;
                }

                if (lowerCommand.Contains("sort"))
                {
                    //cheking files in image directory
                    string[] files;
                    if((files = Directory.GetFiles(saveDirectory)).Length <= 0)
                    {
                        logService.Log("No photo in directory");
                        continue;
                    }
                    
                    var comparer = new ImageComparer();

                    //going through the files
                    foreach (var sourseFile in files)
                    {
                        foreach (var comparisonFile in files)
                        {
                            if(sourseFile == comparisonFile)
                            {
                                continue;
                            }

                            using (var sourseImage = Image.Load<Rgb24>(sourseFile))
                            using (var comparisonImage = Image.Load<Rgb24>(comparisonFile))
                            {
                                var comparerResult = comparer.CompareImages(sourseImage, comparisonImage);
                                Console.WriteLine($"comp {sourseFile} to {comparisonFile} -> {comparerResult * 100}%");
                            }
                        }
                    }


                    /*var files = Directory.GetFiles(saveDirectory);
                    if (files.Length >= 1)
                    {
                        Image image = Image.FromFile(files[0]);


                        using (Bitmap b = new(image, new Size(w, h)))
                        {
                            b.Save("lol.jpeg");
                        }

                    }*/
                }

                //unauthorize command
                else if (lowerCommand.Contains(ConsoleCommands.UnAuth))
                {
                    var parts = lowerCommand.Split(' ');

                    if (parts.Length != 2)
                    {
                        Console.WriteLine("Wrong command");
                        continue;
                    }

                    if (long.TryParse(parts[1], out var value))
                    {
                        var user = db.Users.FirstOrDefault(_ => _.TelegramId == value);

                        if (user == null)
                        {
                            Console.WriteLine("Unknown user");
                            continue;
                        }

                        user.isAuthorized = false;
                        await db.SaveChangesAsync();
                        logService.Log($"User with code {value} now is NOT authorized");
                    }

                }

                //authorize command
                else if (lowerCommand.Contains(ConsoleCommands.Auth))
                {
                    var parts = lowerCommand.Split(' ');

                    if (parts.Length != 2 )
                    {
                        Console.WriteLine("Wrong command");
                        continue;
                    }

                    if(long.TryParse(parts[1], out var value))
                    {
                        var user = db.Users.FirstOrDefault(_ => _.TelegramId == value);

                        if (user == null)
                        {
                            Console.WriteLine("Unknown user");
                            continue;
                        }

                        user.isAuthorized = true;
                        await db.SaveChangesAsync();
                        logService.Log($"User with code {value} now is authorized");
                    }

                }

                //go to log menu screen
                else if (lowerCommand.Contains(ConsoleCommands.Log))
                {
                    Console.Clear();                    
                    Console.WriteLine("Log mode (press \'C\' to stop)");
                    logService.EnableBotOut();

                    while(true)
                    {
                        if (Console.ReadKey().Key == ConsoleKey.C)
                        {
                            break;
                        }
                    }

                    logService.DisableBotOut();
                    Console.Clear();
                    Console.WriteLine("Command mode");

                }

                //help command
                else if (lowerCommand.Contains(ConsoleCommands.Help))
                {
                    Console.WriteLine($"{ConsoleCommands.Stop} - stop bot\n{ConsoleCommands.Help} - view this list\n{ConsoleCommands.Auth} - send with user code, to authorize him\n{ConsoleCommands.UnAuth} - send with user code, to unauthorize him\n{ConsoleCommands.Log} - changes the mode to log");
                }
                else
                {
                    Console.WriteLine("Wrong command");
                }
                

                /*var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                switch (command.ToLower())
                {
                    case "stop":
                    case "exit":
                        {
                            Console.Clear();
                            Console.WriteLine("Bot stopped");
                            return;
                        }
                    case "help":
                        {
                            Console.Clear();
                            Console.WriteLine("stop\\exit - stop bot");
                            Console.WriteLine("log - print last 100 logged messages");
                            break;
                        }
                    case "log":
                        {
                            //todo: create
                            break;
                        }
                    default:
                        {
                            Console.Clear();
                            Console.WriteLine("Unknown command");
                            break;
                        }
                }*/

            }
        }
    }
}
