// TaskFlow.WebAPI/Models/TaskItem.cs
using System.ComponentModel.DataAnnotations; // For data validation attributes

namespace TaskFlow.WebAPI.Models
{
    public class TaskItem
    {
        public int Id { get; set; } // Primary Key for the database

        [Required] // Makes this field mandatory
        [StringLength(100)] // Limits the length of the title
        public string? Title { get; set; }

        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Default to current time
    }
}