using System.ComponentModel.DataAnnotations;
using WebPBL3.Models;

namespace WebPBL3.DTO
{
    public class NewsDto
    {
        public int? STT;
        public string? NewsID { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        
        [Required]
        public string Photo { get; set; }
        [Required]
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? UpdateBy { get; set; }
        public string? FullName {  get; set; }
        public string StaffID { get; set; }
    }
}
