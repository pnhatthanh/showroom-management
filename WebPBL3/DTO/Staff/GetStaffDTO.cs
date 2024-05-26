using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebPBL3.Models;

namespace WebPBL3.DTO.Staff
{
    public class GetStaffDTO
    {
        public string? StaffID { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? IdentityCard { get; set; }

        public bool? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Address { get; set; }

        public string? Photo { get; set; }

        public string? Position { get; set; }
        public int? Salary { get; set; }
        public bool Status { get; set; }
        public string? UserID { get; set; }
        public string? AccountID { get; set; }
        public int? WardID { get; set; }
        public int? RoleID { get; set; }
    }
}
