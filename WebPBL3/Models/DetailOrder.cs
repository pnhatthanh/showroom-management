using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPBL3.Models
{
    public class DetailOrder
    {
        public DetailOrder()
        {
            DetailOrderID = OrderID = CarID = string.Empty;
        }

        [Key]
        [StringLength(maximumLength: 10)]
        [Display(Name = "Mã chi tiết hóa đơn")]
        public string DetailOrderID { get; set; }



        [Required(ErrorMessage = "Số lượng không thể trống")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Giá bán tại thời điểm đó không thể trống")]
        [Display(Name = "Giá bán tại thời điểm đó")]
        public double Price { get; set; }

        public Order Order { get; set; }
        public string OrderID { get; set; }
        public Car Car { get; set; }
        public string CarID { get; set; }
    }
}
