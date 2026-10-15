using System.ComponentModel.DataAnnotations;

namespace ToDoTask.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Deadline { get; set; }

        [Required]
        public string Attachment { get; set; } = string.Empty;
    }
}
