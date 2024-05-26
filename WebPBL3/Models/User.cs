using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebPBL3.Models;

namespace WebPBL3.Models
{
    public class User
    {
        public User()
        {
            UserID = string.Empty;
            FullName = "User";
            PhoneNumber = string.Empty;
            IdentityCard = string.Empty;
            Address = string.Empty;
            Photo = string.Empty;
        }

        [Key]
        [Display(Name = "Mã người dùng")]
        [StringLength(maximumLength: 10)]
        public string UserID { get; set; }


        [Display(Name = "Họ và tên ")]
        [StringLength(maximumLength: 50)]
        public string? FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(maximumLength: 15)]
        
        public string? PhoneNumber { get; set; }

        [Display(Name = "Số CCCD")]
        [StringLength(maximumLength: 12, MinimumLength = 9, ErrorMessage = "CCCD phải tối thiểu 9 và tối đa 12 chữ số.")]
        public string? IdentityCard { get; set; }

        [Display(Name = "Giới tính")]
        public bool? Gender { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(maximumLength: 200, ErrorMessage = "Địa chỉ quá dài")]
        public string? Address { get; set; }
        public int? WardID { get; set; }
        public Ward? Ward { get; set; }

        [Display(Name = "Ảnh")]
        public string? Photo { get; set; } = "";


        public Account Account { get; set; }
        [StringLength(maximumLength: 10)]
        public string AccountID { get; set; }

        public Staff Staff { get; set; }

        public List<Feedback> Feedbacks { get; set; }
        public List<Order> Orders { get; set; }
    }
}