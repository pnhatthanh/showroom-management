using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebPBL3.Models;

namespace WebPBL3.Models
{
    public class News
    {
        [Key]
        [StringLength(maximumLength: 10)]
        public string NewsID { get; set; }

        [Required(ErrorMessage = "Tiêu đề không thể trống")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }
        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không thể trống")]
        public string Content { get; set; }
        [Display(Name = "Ảnh đính kèm")]
        [Required(ErrorMessage = "Ảnh không thể trống")]
        public string Photo { get; set; }
        [Required]
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        [Display(Name = "Mã nhân viên cập nhật")]
        [StringLength(maximumLength: 10)]
        public string? UpdateBy { get; set; }

        public Staff Staff { get; set; }
        public string StaffID { get; set; }
    }
}