using System.ComponentModel.DataAnnotations;

namespace WebPBL3.DTO
{
    public class FeedbackDTO
    {
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Bạn chưa nhập họ và tên")]
        public string FullName { get; set; }
        [Display(Name ="Email")]
        [Required(ErrorMessage = "Bạn chưa nhập địa chỉ email")]
        public string Email { get; set; }
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Bạn chưa nhập tiêu đề")]
        public string Title { get; set; }


        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Bạn chưa nhập nội dung")]
        public string Content { get; set; }
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }
    }
}
