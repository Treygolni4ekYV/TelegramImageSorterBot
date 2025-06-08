using System.Diagnostics.Eventing.Reader;
using System.Text;
using TelegramImageSorterBot.Models.Log;

namespace TelegramImageSorterBot.Services.imp
{
    internal class LogService : ILogService
    {
        private readonly string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        
        private bool isBotOutEnabled = false;
        private StreamWriter? logStream = null;

        ~LogService()
        {
            StopLog();
        }

        public void DisableBotOut()
        {
            isBotOutEnabled = false;
        }

        public void EnableBotOut()
        {
            isBotOutEnabled = true;
        }

        public async void Log(string message, LogMessageType messageType, bool isBotOut = false)
        {
            //console out
            switch (messageType)
            {
                case LogMessageType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogMessageType.notification:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
            }

            if(isBotOut & messageType == LogMessageType.adminInput)
            {
                throw new Exception("Bot can't send admin messages");
            }

            if ((isBotOutEnabled & isBotOut) | (messageType != LogMessageType.adminInput & isBotOut == false))
            {
                Console.WriteLine(message);
            }
            
            Console.ResetColor();

            //out in file
            if (logStream != null)
            {
                var writeMessage = $"({DateTime.Now.ToString()}) {messageType.ToString()} > {message}\n";
                await logStream.WriteAsync(writeMessage);
            }

        }

        public void Log(string message, bool isBotOut = false) => Log(message, LogMessageType.message, isBotOut);

        public void LogAdminInput(string message, bool isBotOut = false) => Log(message, LogMessageType.adminInput, isBotOut);

        public void LogError(string message, bool isBotOut = false) => Log(message, LogMessageType.error, isBotOut);

        public void LogNotification(string message, bool isBotOut = false) => Log(message, LogMessageType.notification, isBotOut);

        public void StartLog()
        {
            if (logStream == null)
            {
                var filePath = Path.Combine(logDirectory, $"{DateTime.Now.ToString("dd.MM.yyyy_HH-mm-ss")}.txt");
                logStream = new StreamWriter(filePath, true, Encoding.UTF8);
            }
        }

        public void StopLog()
        {
            if(logStream != null)
            {
                logStream.Close();
                logStream = null;
            }
        }
    }
}
