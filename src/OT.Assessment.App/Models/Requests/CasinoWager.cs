using System.ComponentModel.DataAnnotations;

namespace OT.Assessment.App.Models.Requests
{
    public class CasinoWager
    {
        [Required]
        public Guid WagerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Theme { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Provider { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string GameName { get; set; } = string.Empty;

        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        public Guid BrandId { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public Guid ExternalReferenceId { get; set; }

        [Required]
        public Guid TransactionTypeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of bets must be at least 1")]
        public int NumberOfBets { get; set; }

        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 letters")]
        public string CountryCode { get; set; } = string.Empty;

        [StringLength(500)]
        public string SessionData { get; set; } = string.Empty;

        [Range(0, long.MaxValue, ErrorMessage = "Duration must be a positive number")]
        public long Duration { get; set; }
    }
}
