
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebPBL3.Models
{
    public class District
    {
        public District()
        {
            DistrictName = string.Empty;
            DistrictID = 0;
        }

        [Key]

        [Display(Name = "Mã huyện")]
        public int DistrictID { get; set; }

        [Display(Name = "Tên huyện")]

        [StringLength(maximumLength: 50)]

        [Required]
        public string DistrictName { get; set; }

        public Province Province { get; set; }
        public int ProvinceID { get; set; }

        public List<Ward> Wards { get; set; }



    }
}