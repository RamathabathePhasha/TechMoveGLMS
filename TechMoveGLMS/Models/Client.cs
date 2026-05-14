using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace TechMoveGLMS.Models
{
    // This class represents a client/customer in the system
    public class Client
    {
        // Primary key - unique ID for each client
        [Key]
        public int ClientId { get; set; }

        // Company name - must be provided
        [Required(ErrorMessage = "Please enter company name")]
        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string Name { get; set; } = string.Empty;

        // Email address - must be valid format
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // Phone number
        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        // Which province/region they are in
        [Required(ErrorMessage = "Region is required")]
        [StringLength(50)]
        public string Region { get; set; } = string.Empty;

        // One client can have many contracts
        public virtual ICollection<Contract>? Contracts { get; set; }
    }
}