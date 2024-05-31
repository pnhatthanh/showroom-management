using System.ComponentModel.DataAnnotations;

namespace WebPBL3.DTO
{
    public class HistoryOrderDto
    {
        [Display(Name = "Ngày tạo đơn hàng")]
        public DateTime Date { get; set; } = DateTime.Now;
        [Display(Name = "Trạng thái thanh toán")]
        
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Tổng thanh toán")]
        public double Totalprice { get; set; }
        public string OrderID { get; set; } = string.Empty;
        public List<HistoryOrderItem> Items { get; set; } = new List<HistoryOrderItem>();
    }
}
