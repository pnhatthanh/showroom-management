using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using WebPBL3.Models;

namespace WebPBL3.Models
{
    public class Ward
    {
        public Ward()
        {
            WardName = string.Empty;
            WardID = 0;
        }

        [Key]

        [Display(Name = "Mã xã")]
        public int WardID { get; set; }

        [Display(Name = "Tên xã")]

        [StringLength(maximumLength: 50)]
        public string WardName { get; set; }

        public District District { get; set; }
        public int DistrictID { get; set; }

    }
}