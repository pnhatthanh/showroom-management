using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace  WebPBL3.Models
{
    public class Account
    {
        public Account()
        {
            AccountID = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }
        [Key]
        [StringLength(maximumLength: 10)]
        [Display(Name = "Mã tài khoản")]
        public string AccountID { get; set; }

        [Display(Name = "Email")]
        [StringLength(maximumLength: 50)]
        [Required(ErrorMessage = "Bạn phải nhập email")]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [StringLength(maximumLength: 100)]
        [Required(ErrorMessage = "Bạn phải nhập password")]
        public string Password { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool Status { get; set; }


        public Role Role { get; set; }
        public int RoleID { get; set; }
        public User User { get; set; }

    }
}