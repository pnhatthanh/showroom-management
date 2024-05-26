using System.ComponentModel.DataAnnotations;
namespace WebPBL3.Models
{
    public class Role
    {
        public Role()
        {
            RoleID = 0;
            RoleName = string.Empty;
        }

        [Key]

        [Display(Name = "Mã quyền truy cập")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Tên quyền truy cập không thể trống")]
        [StringLength(maximumLength: 20)]
        [Display(Name = "Tên quyền truy cập")]
        public string RoleName { get; set; }

    }
}