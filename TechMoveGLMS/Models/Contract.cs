using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Models
{
    // This class represents a contract between TechMove and a client
    public class Contract
    {
        // Primary key
        [Key]
        public int ContractId { get; set; }

        // Which client this contract belongs to
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        // Link to the Client object
        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        // When contract starts
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        // When contract ends
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        // Status: Draft, Active, Expired, OnHold
        [Required]
        [Display(Name = "Contract Status")]
        public string Status { get; set; } = "Draft";

        // Service level: Standard, Premium, Enterprise
        [Required]
        [StringLength(50)]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        // Where the PDF file is stored on server
        [Display(Name = "Signed Agreement")]
        public string? SignedAgreementPath { get; set; }

        // Original name of the uploaded file
        [Display(Name = "Agreement File Name")]
        public string? SignedAgreementFileName { get; set; }

        // One contract can have many service requests
        public virtual ICollection<ServiceRequest>? ServiceRequests { get; set; }
    }
}