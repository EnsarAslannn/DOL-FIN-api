using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Portfolio
{
    public class AmountRequestDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
