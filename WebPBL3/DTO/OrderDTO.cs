using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
namespace WebPBL3.DTO
{
    public class OrderDTO
    {
        [Display(Name = "Họ và tên ")]
        [Required(ErrorMessage = "Bạn chưa nhập họ và tên")]
        [StringLength(maximumLength: 50)]
        public string FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(maximumLength: 15)]
        [Required(ErrorMessage = "Bạn chưa nhập số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Số CCCD")]
        [StringLength(maximumLength: 12, MinimumLength = 9, ErrorMessage = "CCCD phải tối thiểu 9 và tối đa 12 chữ số.")]
        [Required(ErrorMessage ="Bạn chưa nhập số CCCD")]
        public string IdentityCard { get; set; }

        [Display(Name = "Địa chỉ")]

        [StringLength(maximumLength: 200, ErrorMessage = "Địa chỉ quá dài")]
        [Required(ErrorMessage ="Bạn chưa nhập địa chỉ")]
        public string? Address { get; set; }
        [Range(1,Int32.MaxValue,ErrorMessage = "Bạn chưa chọn xã phường")]
        public int WardID {  get; set; }
        [Range(1, Int32.MaxValue, ErrorMessage = "Bạn chưa chọn quận huyện")]
        public int DistrictID {  get; set; }
        [Range(1, Int32.MaxValue, ErrorMessage = "Bạn chưa chọn tỉnh thành phố")]
        public int ProvinceID { get; set; }

        [Display(Name = "Ngày tạo đơn hàng")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Tổng thanh toán")]
        public double Totalprice { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        [Required(ErrorMessage ="Bạn chưa chọn trạng thái thanh toán")]
        public string Status { get; set; }

        [Display(Name = "Email")]
        [StringLength(maximumLength: 50)]
        [Required(ErrorMessage ="Bạn chưa nhập địa chỉ email")]
        public string Email { get; set; }
        [MinLength(1, ErrorMessage = "Phải chọn mua ít nhất một ô tô")]
        public List<Items> items { get;set; }=new List<Items>();
    }
}
