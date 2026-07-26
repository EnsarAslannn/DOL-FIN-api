using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Portfolio
{
    public class TradeRequestDto
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
