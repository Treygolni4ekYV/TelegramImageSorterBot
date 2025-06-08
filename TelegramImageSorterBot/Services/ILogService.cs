using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TelegramImageSorterBot.Models.Log;

namespace TelegramImageSorterBot.Services
{
    internal interface ILogService
    {
        void Log(string message, LogMessageType messageType, bool isBotOut = false);
        

        void Log(string message, bool isBotOut = false);
        void LogError(string message, bool isBotOut = false);
        void LogNotification(string message, bool isBotOut = false);
        void LogAdminInput(string message, bool isBotOut = false);


        void StartLog();
        void StopLog();

        void EnableBotOut();
        void DisableBotOut();
    }
}
