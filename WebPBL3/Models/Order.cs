using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPBL3.Models;

namespace WebPBL3.Models
{
    public class Order
    {
        public Order()
        {
            OrderID = UserID = StaffID = string.Empty;
            Totalprice = 0;
            Status = string.Empty;
            Flag = false;
        }

        [Key]
        [StringLength(maximumLength: 10)]
        [Display(Name = "Mã hóa đơn")]
        public string OrderID { get; set; }



        [Required(ErrorMessage = "Ngày xuất đơn không thể trống")]
        [Display(Name = "Ngày xuất đơn")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Tổng thanh toán không thể trống")]
        [Display(Name = "Tổng thanh toán")]
        public double Totalprice { get; set; }

        [Required(ErrorMessage = "Trạng thái thanh toán không thể trống")]
        [Display(Name = "Trạng thái thanh toán")]
        public string Status { get; set; }

        public bool Flag { get; set; }



        public User User { get; set; }

        public string UserID { get; set; }

        public Staff Staff { get; set; }

        public string StaffID { get; set; }
        public List<DetailOrder> DetailOrders { get; set; }
    }
}