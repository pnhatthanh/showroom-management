using System.ComponentModel.DataAnnotations;

namespace WebPBL3.DTO.Staff
{
    public class UpdateStaffDTO
    {
        public string? StaffID { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? IdentityCard { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Photo { get; set; }

        public string? Position { get; set; }

        public int? Salary { get; set; }

        public string? WardName { get; set; }

        public string? DistrictName { get; set; }

        public string? ProvinceName { get; set; }
        public bool? Status { get; set; }
        public bool? Gender { get; internal set; }
    }
}
