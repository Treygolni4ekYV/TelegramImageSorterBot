using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Polling;

namespace TelegramImageSorterBot.Services
{
    internal interface ITelegramBotService
    {
        bool Start(string saveDirectory);
    }
}
