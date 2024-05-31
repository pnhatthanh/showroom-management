using System.ComponentModel.DataAnnotations;
using WebPBL3.Models;

namespace WebPBL3.Models
{
	public class UserDto
	{	
		
		public string? AccountID { get; set; }

        [Display(Name = "Email")]
        [StringLength(maximumLength: 50)]
        [Required(ErrorMessage = "Bạn phải nhập email")]
        public string Email { get; set; } = string.Empty;
		public string? Password { get; set; }
		public bool Status { get; set; }
		public int RoleID { get; set; }
		public string? UserID { get; set; }
		public string? FullName { get; set; }
		public string? PhoneNumber { get; set; }
		public string? IdentityCard { get; set; }

		public bool? Gender { get; set; }
		public DateTime? BirthDate { get; set; }
		public string? Address { get; set; }
		public string? Photo { get; set; } = "";
		public int? WardID { get; set; }
		public int ProvinceID {  get; set; }
		public int DistrictID {  get; set; }
		public string? WardName { get; set; }
		public string? DistrictName { get; set; }
		public string? ProvinceName { get; set; }

		
	}
}
