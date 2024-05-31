using System.ComponentModel.DataAnnotations;

namespace WebPBL3.DTO
{
    public class CarDto
    {
        public string? CarID { get; set; }
        [Required]
        public string CarName { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; }

        [Required]
        public int Seat { get; set; }
        [Required]
        public string Origin { get; set; } = string.Empty;
        [Required]
        public string Dimension { get; set; } = string.Empty;
        [Required]
        public double Capacity { get; set; }
        [Required]
        public float Topspeed { get; set; }
        [Required]
        public string Color { get; set; } = string.Empty;
        [Required]
        public string Photo { get; set; } = string.Empty;
        [Required]
        public int Year { get; set; }
        [Required]
        public string Engine { get; set; } = string.Empty;
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string FuelConsumption { get; set; } = string.Empty;
        
        public string? MakeName { get; set; }
        
        public int MakeID { get; set; }
        
    }
}
