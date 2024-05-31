
using System.ComponentModel.DataAnnotations;

namespace WebPBL3.DTO
{
    public class NewVM
    {
        [Required]
        public string NewsID { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty ;
        [Required]
        public string Photo { get; set; } = string.Empty;
        [Required]
        public DateTime CreateAt { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
