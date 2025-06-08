using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramImageSorterBot.db;
using TelegramImageSorterBot.Models.db;
using TelegramImageSorterBot.Models.Log;
using TelegramImageSorterBot.Util;

namespace TelegramImageSorterBot.Services.imp
{
    internal class TelegramBotService : ITelegramBotService
    {
        private readonly ILogService logService;
        private readonly BotContext db;
        private string saveDirectory;

        public TelegramBotService(ILogService logService, BotContext db) 
        {
            this.logService = logService;
            this.db = db;
            saveDirectory = Directory.GetCurrentDirectory();
        }

        public bool Start(string saveDirectory)
        {
            try
            {
                this.saveDirectory = saveDirectory;

                var botToken = ConfigurationManager.AppSettings["botToken"];
                if(botToken == null)
                {
                    throw new Exception("Bot token is empty");
                }


                var telegramBotClient = new TelegramBotClient(botToken);
                var cancellationToken = new CancellationTokenSource();
                var botOptions = new ReceiverOptions
                {
                    AllowedUpdates = new[]
                    {
                        UpdateType.Message
                    }
                };
                telegramBotClient.StartReceiving(UpdateHandler, ErrorHandler, botOptions, cancellationToken.Token);

                return true;
            }
            catch (Exception ex)
            {
                logService.LogError(ex.Message, true);
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            return Task.CompletedTask;
        }

        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            var message = update.Message;
                            if (message == null)
                            {
                                return;
                            }

                            var sender = message.From;
                            if (sender == null)
                            {
                                return;
                            }

                            await MessageHandlerAsync(client, message, token, sender);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                logService.LogError(ex.Message, true);
            }
        }

        private bool isAuthorizedUser(User user)
        {
            var dbUser = db.Users.Where(_ => _.TelegramId == user.Id).FirstOrDefault();
            if (dbUser == null)
            {
                return false;
            }

            Debug.WriteLine(dbUser.isAuthorized);
            return dbUser.isAuthorized;
        }

        private async Task MessageHandlerAsync(ITelegramBotClient client, Message message, CancellationToken token, User senderUser)
        {
            var chat = message.Chat;

            //if command don't have text
            if (string.IsNullOrEmpty(message.Text))
            {
                if (!isAuthorizedUser(senderUser))
                {
                    await client.SendMessage(chat.Id, $"Command don't allowed, contact the administrator. You code is {senderUser.Id}");
                    return;
                }

                //image command
                if (message.Photo != null)
                {
                    var photos = message.Photo;

                    await client.SendMessage(chat.Id, "Processing photo");
                    await SavePhotoCommand(photos, client, token, senderUser);
                    return;
                }
                
                return;
            }

            //commands
            switch (message.Text.ToLower())
            {
                case BotCommands.Start:
                    {
                        StartCommand(client, token, chat, senderUser);
                        return;
                    }
                case BotCommands.CheckStatus:
                    {
                        logService.Log("SOEME REALY BITG TEXTSOEME REALY BITG TEXTSOEME REALY BITG TEXTSOEME REALY BITG TEXTSOEME REALY BITG TEXT", true);
                        return;
                    }
                default:
                    {
                        await client.SendMessage(chat.Id, "Unknown command");
                        return;
                    }
            }
        }

        private async void StartCommand(ITelegramBotClient client, CancellationToken token, Chat chat, User user)
        {
            var dbUser = db.Users.FirstOrDefault(_ => _.TelegramId == user.Id);
            if(dbUser == null)
            {
                var newUser = new DBUser()
                {
                    TelegramId = user.Id,
                    TelegramUsername = user.Username!,
                };
                await db.Users.AddAsync(newUser);
                await db.SaveChangesAsync();

                await client.SendMessage(chat.Id, "Account created");
                logService.Log($"User (id: {user.Id}) creating account", true);
                return;
            }
            await client.SendMessage(chat.Id, "You already have account");
            return;
        }


        private async Task SavePhotoCommand(PhotoSize[] photos,  ITelegramBotClient client, CancellationToken token, User senderUser)
        {
            var photoId = photos.Last().FileId;

            var fileInfo = await client.GetFile(photoId, token);

            var photoPath = Path.Combine(saveDirectory, $"{photoId}.jpg");
            using (var stream = new FileStream(photoPath, FileMode.Create))
            {
                await client.DownloadFile(fileInfo.FilePath!, stream, token);
            }

            logService.Log($"{senderUser.Id}({senderUser.Username}) saving photo ({photoId})", true);
        }
    }
}
