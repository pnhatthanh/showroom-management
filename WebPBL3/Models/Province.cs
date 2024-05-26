using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace WebPBL3.Models
{
    public class Province
    {
        public Province()
        {
            ProvinceName = string.Empty;
            ProvinceID = 0;
        }

        [Key]

        [Display(Name = "Mã tỉnh")]
        public int ProvinceID { get; set; }

        [Display(Name = "Tên tỉnh")]

        [StringLength(maximumLength: 50)]

        [Required]
        public string ProvinceName { get; set; }

        public List<District> Districts { get; set; }


    }
}
