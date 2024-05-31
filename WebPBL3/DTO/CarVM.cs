using System.ComponentModel.DataAnnotations;

namespace WebPBL3.DTO
{
    public class CarVM
    {
        public string? CarID { get; set; }
        [Required]
        public string CarName { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; }
        [Required]
        public string Photo { get; set; } = string.Empty;
        [Required]

        public string? MakeName { get; set; }

    }
}
