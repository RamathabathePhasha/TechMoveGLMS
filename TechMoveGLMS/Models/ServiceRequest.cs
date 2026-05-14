using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Models
{
    // This class represents a service request under a contract
    public class ServiceRequest
    {
        // Primary key
        [Key]
        public int ServiceRequestId { get; set; }

        // Which contract this request belongs to
        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        // Link to the Contract object
        [ForeignKey("ContractId")]
        public virtual Contract? Contract { get; set; }

        // What service is needed
        [Required]
        [StringLength(500)]
        [Display(Name = "Service Description")]
        public string Description { get; set; } = string.Empty;

        // Amount in US Dollars
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount (USD)")]
        [Range(0.01, 1000000)]
        public decimal AmountUSD { get; set; }

        // Amount in South African Rand (calculated automatically)
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount (ZAR)")]
        public decimal AmountZAR { get; set; }

        // Status: Pending, InProgress, Completed, Cancelled
        [Required]
        [Display(Name = "Request Status")]
        public string Status { get; set; } = "Pending";

        // When this request was created
        [DataType(DataType.DateTime)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Which exchange rate was used for conversion
        [Column(TypeName = "decimal(10,4)")]
        [Display(Name = "Exchange Rate Used")]
        public decimal ExchangeRateUsed { get; set; }
    }
}