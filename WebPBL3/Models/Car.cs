using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace WebPBL3.Models
{
    public class Car
    {
        public Car()
        {
            CarID = CarName = Origin = Dimension = Photo = Engine = Description = FuelConsumption = string.Empty;
            Price = Capacity = 0.0;
            Topspeed = 0.0F;
            Flag = false;
            Seat = Year = Quantity = 0;
        }
        [Key]
        [Display(Name = "Mã xe")]
        [StringLength(maximumLength: 10)]
        public string CarID { get; set; }


        [Required(ErrorMessage = "Tên xe không thể trống")]
        [Display(Name = "Tên xe")]
        [StringLength(maximumLength: 50)]
        public string CarName { get; set; }



        [Required(ErrorMessage = "Giá xe không thể trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá xe không hợp lệ")]
        [Display(Name = "Giá xe")]
        public double Price { get; set; }


        [Display(Name = "Số chỗ")]
        [Required(ErrorMessage = "Số chỗ không thể trống")]

        public int Seat { get; set; }

        [Display(Name = "Xuất xứ")]
        [StringLength(maximumLength: 50)]
        public string Origin { get; set; }

        [StringLength(maximumLength: 50)]
        [Display(Name = "Kích thước")]
        [Required(ErrorMessage = "Kích thước không thể trống")]
        public string Dimension { get; set; }
       

        [Display(Name = "Dung tích")]
        [Required(ErrorMessage = "Dung tích không thể trống")]
        public double Capacity { get; set; }
        [Display(Name = "Tốc độ tối đa")]
        [Range(0, float.MaxValue, ErrorMessage = "Tốc độ không hợp lệ")]
        public float Topspeed { get; set; }

        [Display(Name = "Màu sắc")]
        [StringLength(maximumLength: 50)]
        [Required(ErrorMessage = "Màu sắc không thể trống")]
        public string Color { get; set; }

        [Display(Name = "Ảnh")]
        [Required]
        public string Photo { get; set; }
        [Display(Name = "Năm sản xuất")]
        [Range(1000, int.MaxValue, ErrorMessage = "Năm không hợp lệ")]
        public int Year { get; set; }
        [Display(Name = "Động cơ")]
        [StringLength(maximumLength: 50)]
        [Required]
        public string Engine { get; set; }
        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Số lượng không thể trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng xe không hợp lệ")]
        public int Quantity { get; set; }
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Mức tiêu thụ")]
        [StringLength(maximumLength: 50)]
        public string FuelConsumption { get; set; }
        public bool Flag { get; set; }
        
        public Make? Make { get; set; }
        [Display(Name = "Mã hãng")]
        public int MakeID { get; set; }
    }
}