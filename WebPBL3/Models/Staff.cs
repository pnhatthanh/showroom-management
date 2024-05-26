using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebPBL3.Models
{

    public class Staff
    {
        public Staff()
        {
            StaffID = string.Empty;
            Position = string.Empty;
            Salary = 0;
        }

        [Key]

        [Display(Name = "Mã nhân viên")]
        [StringLength(maximumLength: 10)]
        public string StaffID { get; set; }


        [Display(Name = "Lương")]
        public int Salary { get; set; }

        [Required(ErrorMessage = "Chức vụ chưa có")]
        [Display(Name = "Chức vụ")]
        [StringLength(maximumLength: 50)]
        public string Position { get; set; }


        public User User { get; set; }
        [StringLength(maximumLength: 10)]
        public string UserID { get; set; }

        public List<News> News { get; set; }
    }
}
