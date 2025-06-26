using System.ComponentModel.DataAnnotations;

namespace TaskFlow.WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Store a HASH, never plain text!
    }
}