using Microsoft.EntityFrameworkCore;
using ToDoTask.Models;

namespace ToDoTask.Date
{
    public class AppDbContext: DbContext
    {
        

        public DbSet<TaskItem> TaskItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=.; Initial Catalog=ToDoList;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;");
        }
    }
}
