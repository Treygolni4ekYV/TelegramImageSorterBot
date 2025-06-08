using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramImageSorterBot.Models.db;

namespace TelegramImageSorterBot.db
{
    internal class BotContext : DbContext
    {
        public DbSet<DBUser> Users { get; set; }
        public DbSet<DBPhoto> Photos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=LocalDatabase.db");
        }

        public BotContext(DbContextOptions<BotContext> options)
        {
            Database.EnsureCreated();
        }
    }
}
