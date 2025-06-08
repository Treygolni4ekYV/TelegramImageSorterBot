using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramImageSorterBot.Models.db
{
    internal class DBUser
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public long TelegramId { get; set; }
        [Required]
        public string TelegramUsername { get; set; } = string.Empty;
        public bool isAuthorized { get; set; } = false;

        List<DBPhoto> photos { get; set; } = new();
    }
}
