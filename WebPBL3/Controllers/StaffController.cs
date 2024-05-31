using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPBL3.DTO.Staff;
using WebPBL3.Models;
using System.IO;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;


namespace WebPBL3.Controllers
{
    [Authorize(Policy = "Admin")]
    public class StaffController : Controller
    {
        private ApplicationDbContext _db;
		private int limits = 10;
		private readonly IWebHostEnvironment environment;
        public StaffController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            this.environment = environment;
        }
        public IActionResult listStaffs(int page = 1)
        {
            var staffDTOs = (from staff in _db.Staffs
                             join user in _db.Users on staff.UserID equals user.UserID
                             join account in _db.Accounts on user.AccountID equals account.AccountID
                             select new GetStaffDTO
                             {
                                 StaffID = staff.StaffID,
                                 FullName = user.FullName,
                                 Email = account.Email,
                                 PhoneNumber = user.PhoneNumber,
                                 IdentityCard = user.IdentityCard,
                                 Gender = user.Gender,
                                 BirthDate = user.BirthDate,
                                 Address = user.Address,
                                 Position = staff.Position,
                                 Salary = staff.Salary,
                                 Status = account.Status
                             }).OrderBy(staff => staff.FullName)
                            .ToList();
            if (staffDTOs.Any())
            {
                ViewBag.staffs = staffDTOs;
            }
            var total = staffDTOs.Count;
			var totalPage = (total + limits - 1) / limits;
			if (page < 1) page = 1;
			if (page > totalPage) page = totalPage;
			ViewBag.totalRecord = total;
			ViewBag.totalPage = totalPage;
			ViewBag.currentPage = page;
			staffDTOs = staffDTOs.Skip((page - 1) * limits).Take(limits).ToList();
			return View(staffDTOs);
        }

        public async Task<int> GetWardIDAsync(string wardName, string districtName, string provinceName)
        {
            var ward = await _db.Wards
                .Include(w => w.District)
                    .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(w => w.WardName.Contains(wardName) &&
                                            w.District.DistrictName.Contains(districtName) &&
                                            w.District.Province.ProvinceName.Contains(provinceName));
            return ward.WardID;
        }
        [HttpGet]
        public JsonResult GetProvince()
        {
            var provinces = _db.Provinces.ToList();
            return new JsonResult(provinces);
        }

        public JsonResult GetDistrict(int id)
        {
            var districts = _db.Districts.Where(d => d.ProvinceID == id).Select(d => new { id = d.DistrictID, name = d.DistrictName }).ToList();
            return new JsonResult(districts);
        }

        public JsonResult GetWard(int id)
        {
            var wards = _db.Wards.Where(w => w.DistrictID == id).Select(w => new { id = w.WardID, name = w.WardName }).ToList();
            return new JsonResult(wards);
        }
        public IActionResult AddStaff()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddStaff(AddStaffDTO staffDTO)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        int newRoleID = 2;
                        var accid = 1;
                        var lastAcc = _db.Accounts.OrderByDescending(a => a.AccountID).FirstOrDefault();
                        if (lastAcc != null)
                        {
                            accid = Convert.ToInt32(lastAcc.AccountID) + 1;
                        }
                        var accidTxt = accid.ToString().PadLeft(8, '0');
                        var accWithEmail = _db.Accounts.FirstOrDefault(u => u.Email == staffDTO.Email);
                        if (accWithEmail != null)
                        {
                            TempData["Error"] = "Email đã tồn tại";
                            return View(staffDTO);
                        }
                        var newAccount = new Account
                        {
                            AccountID = accidTxt,
                            Email = staffDTO.Email,
                            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                            RoleID = newRoleID,
                            Status = false,
                        };
                        _db.Accounts.Add(newAccount);
                        await _db.SaveChangesAsync();

                        var userid = 1;
                        var lastUser = _db.Users.OrderByDescending(u => u.UserID).FirstOrDefault();
                        if (lastUser != null)
                        {
                            userid = Convert.ToInt32(lastUser.UserID.Substring(2)) + 1;
                        }
                        var useridTxt = "NV" + userid.ToString().PadLeft(6, '0');
                        int newWardID = Convert.ToInt32(staffDTO.WardName);
                        var staffId = 1;
                        var lastStaff = _db.Staffs.OrderByDescending(u => u.StaffID).FirstOrDefault();
                        if (lastStaff != null)
                        {
                            staffId = Convert.ToInt32(lastStaff.StaffID.Substring(2)) + 1;
                        }
                        var staffTxt = "NV" + staffId.ToString().PadLeft(6, '0');
                        staffDTO.StaffID = staffTxt;
                        var accWithIdentityCard = _db.Users.FirstOrDefault(u => u.IdentityCard == staffDTO.IdentityCard);
                        if (accWithIdentityCard != null)
                        {
                            TempData["Error"] = "CCCD đã tồn tại";
                            return View(staffDTO);
                        }
                        string newFilename = null;
                        if (staffDTO.Photo != null)
                        {
                            newFilename = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            newFilename += Path.GetExtension(staffDTO.Photo!.FileName);
                            string imageFullPath = environment.WebRootPath + "/upload/staff/" + newFilename;
                            using (var stream = System.IO.File.Create(imageFullPath))
                            {
                                staffDTO.Photo.CopyTo(stream);
                            }
                        }
                        var newUser = new User
                        {
                            UserID = useridTxt,
                            FullName = staffDTO.FullName,
                            PhoneNumber = staffDTO.PhoneNumber,
                            IdentityCard = staffDTO.IdentityCard,
                            Gender = staffDTO.Gender,
                            BirthDate = staffDTO.BirthDate,
                            Address = staffDTO.Address,
                            Photo = newFilename,
                            WardID = newWardID,
                            AccountID = newAccount.AccountID,
                        };
                        _db.Users.Add(newUser);
                        await _db.SaveChangesAsync();
                        var newStaff = new Staff
                        {
                            StaffID = staffDTO.StaffID,
                            Position = staffDTO.Position,
                            Salary = staffDTO.Salary,
                            UserID = newUser.UserID,
                        };
                        _db.Staffs.Add(newStaff);
                        await _db.SaveChangesAsync();
                        transaction.Commit();
                        return RedirectToAction("listStaffs");

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm nhân viên: " + errorMessage);
                    }
                    var provinces = _db.Provinces.ToList();
                    ViewBag.Provinces = new SelectList(provinces, "ProvinceID", "ProvinceName");
                }
            }
            return View(staffDTO);
        }
        public async Task<IActionResult> DeleteStaff(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            Staff? staff = await _db.Staffs.FindAsync(id);
            if (staff != null)
            {
                _db.Staffs.Remove(staff);
            }
            User? user = await _db.Users.FindAsync(staff.UserID);
            if (user != null)
            {
                _db.Users.Remove(user);
            }
            Account? account = await _db.Accounts.FindAsync(user.AccountID);
            if (user != null)
            {
                _db.Accounts.Remove(account);
            }
            await _db.SaveChangesAsync();

            return RedirectToAction("listStaffs");
        }

        public IActionResult Details(string? id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            Staff? staff = _db.Staffs.Find(id);
            if (staff == null)
            {
                return NotFound();
            }
            User? user = _db.Users.FirstOrDefault(u => u.UserID == staff.UserID);
            if(user == null)
            {
                return NotFound();
            }    
            Account? account = _db.Accounts.FirstOrDefault(a => a.AccountID == user.AccountID);
            if (account == null)
            {
                return NotFound();
            }
            Ward? ward = _db.Wards.FirstOrDefault(w => w.WardID == user.WardID);
            District? district = null;
            Province? province = null;

            if (ward != null)
            {
                district =  _db.Districts.FirstOrDefault(d => d.DistrictID == ward.DistrictID);
                province =  _db.Provinces.FirstOrDefault(p => p.ProvinceID == district.ProvinceID);               
            }

            // Kiểm tra nếu district hoặc province là null để tránh lỗi
            string? districtName = district?.DistrictName;
            string? provinceName = province?.ProvinceName;
            string? wardName = ward?.WardName;
            AddStaffDTO staffDTO = new AddStaffDTO
            {
                StaffID = staff.StaffID,
                FullName = user.FullName,
                Email = account.Email,
                PhoneNumber = user.PhoneNumber,
                IdentityCard = user.IdentityCard,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Address = user.Address,
                Position = staff.Position,
                Salary = staff.Salary,
                ProvinceName = districtName,
                DistrictName = provinceName,
                WardName = wardName,
            };
            ViewData["Photo"] = user.Photo;
            return View(staffDTO);
        }

        public async Task<ActionResult> UpdateStaff(string? id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            Staff? staff = await _db.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == staff.UserID);
            if(user == null)
            {
                return NotFound();
            }    
            Account? account = await _db.Accounts.FirstOrDefaultAsync(a => a.AccountID == user.AccountID);
            if(account == null)
            {
                return NotFound();
            }    
            Ward? ward = await _db.Wards.FirstOrDefaultAsync(w => w.WardID == user.WardID);
            District? district = null;
            Province? province = null;

            if (ward != null)
            {
                district = await _db.Districts.FirstOrDefaultAsync(d => d.DistrictID == ward.DistrictID);
                province = await _db.Provinces.FirstOrDefaultAsync(p => p.ProvinceID == district.ProvinceID);
            }

            // Kiểm tra nếu district hoặc province là null để tránh lỗi
            string? districtName = district?.DistrictName;
            string? provinceName = province?.ProvinceName;
            string? wardName = ward?.WardName;
            int? districtID = district?.DistrictID;
            int? provinceID = province?.ProvinceID;
            int? wardID = ward?.WardID;
            AddStaffDTO getstaffDTO = new AddStaffDTO
            {
                StaffID = staff.StaffID,
                FullName = user.FullName,
                Email = account.Email,
                PhoneNumber = user.PhoneNumber,
                IdentityCard = user.IdentityCard,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Address = user.Address,
                Position = staff.Position,
                Salary = staff.Salary,
                ProvinceName = provinceName,
                DistrictName = districtName,
                WardName = wardName,
            };
            ViewBag.ProvinceName = provinceName;
            ViewBag.DistrictName = districtName;
            ViewBag.WardName = wardName;
            ViewBag.ProvinceID = provinceID;
            ViewBag.DistrictID = districtID;
            ViewBag.WardID = wardID;
            ViewData["Photo"] = user.Photo;
            return View("UpdateStaff",getstaffDTO);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateStaff(AddStaffDTO staffDTO)
        {
            Staff? staff = await _db.Staffs.FindAsync(staffDTO.StaffID);
            if (staff == null)
            {
                return NotFound();
            }
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == staff.UserID);
            if(user == null)
            {
                return NotFound();
            }    
            Account? account = await _db.Accounts.FirstOrDefaultAsync(a => a.AccountID == user.AccountID);
            if(account == null)
            {
                return NotFound();
            }    
            string? newFilename = user.Photo;
            if (ModelState.IsValid)
            {
                if(staffDTO.Photo != null)
                {
                    newFilename = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newFilename += Path.GetExtension(staffDTO.Photo!.FileName);
                    string imageFullPath = environment.WebRootPath + "/upload/staff/" + newFilename;
                    using (var stream = System.IO.File.Create(imageFullPath))
                    {
                        staffDTO.Photo.CopyTo(stream);
                    }
                    if (!string.IsNullOrEmpty(user.Photo))
                    {
                        string oldImageFullPath = environment.WebRootPath + "/upload/staff/" + user.Photo;
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }
                    user.Photo = newFilename;
                }          
                staff.Position = staffDTO.Position;
                staff.Salary = (int)staffDTO.Salary;
                user.FullName = staffDTO.FullName;
                account.Email = staffDTO.Email;
                user.Address = staffDTO.Address;
                user.PhoneNumber = staffDTO.PhoneNumber;
                user.IdentityCard = staffDTO.IdentityCard;
                user.Gender = staffDTO.Gender;
                user.BirthDate = staffDTO.BirthDate;
                user.WardID = Convert.ToInt32(staffDTO.WardName);
                try
                {
                    _db.Accounts.Update(account);
                    _db.Users.Update(user);
                    _db.Staffs.Update(staff);
                    _db.SaveChangesAsync();
                    return RedirectToAction("listStaffs");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return NotFound();
                }

            }
            ViewData["Photo"] = user.Photo;
            return View(staffDTO);
        }


        public IActionResult Search(string searchTerm, string searchField, int page = 1)
        {
            page = page < 1 ? 1 : page;
            IQueryable<GetStaffDTO> staffQuery = _db.Staffs
                .Include(s => s.User)
                .Include(s => s.User.Account)
                .Select(s => new GetStaffDTO
                {
                    StaffID = s.StaffID,
                    FullName = s.User.FullName,
                    Email = s.User.Account.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    IdentityCard = s.User.IdentityCard,
                    Gender = s.User.Gender,
                    BirthDate = s.User.BirthDate,
                    Address = s.User.Address,
                    Position = s.Position,
                    Salary = s.Salary
                });

            if (!string.IsNullOrEmpty(searchTerm) && !string.IsNullOrEmpty(searchField))
            {
                switch (searchField)
                {
                    case "FullName":
                        staffQuery = staffQuery.Where(s => s.FullName != null && s.FullName.Contains(searchTerm));
                        break;
                    case "IdentityCard":
                        staffQuery = staffQuery.Where(s => s.IdentityCard != null && s.IdentityCard.Contains(searchTerm));
                        break;
                    case "PhoneNumber":
                        staffQuery = staffQuery.Where(s => s.PhoneNumber != null && s.PhoneNumber.Contains(searchTerm));
                        break;
                    case "Address":
                        staffQuery = staffQuery.Where(s => s.Address != null && s.Address.Contains(searchTerm));
                        break;
                }
            }
            var staffs = staffQuery.ToList();
            if (staffs.Any())
            {
                ViewBag.staffs = staffs;
            }
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SearchField = searchField;

            var total = staffs.Count;
            var totalPage = (total + limits - 1) / limits;
            if (page < 1) page = 1;
            if (page > totalPage) page = totalPage;
            int offset = (page - 1) * limits;
            if (offset < 0) offset = 0;
            ViewBag.totalRecord = total;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;

            var staffDTOs = staffQuery
                .OrderBy(staff => staff.FullName)
                .Skip(offset)
                .Take(limits)
                .ToList();
            return View("listStaffs", staffDTOs);
        }

        [HttpPost]
        public IActionResult SaveExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var staffTable = JsonConvert.DeserializeObject<List<GetStaffDTO>>(Request.Form["data"], new JsonSerializerSettings { MaxDepth = 10 });
            if (staffTable == null || !staffTable.Any())
            {
                TempData["Error"] = "Chưa có dữ liệu";
                return RedirectToAction("listStaffs");
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Mã nhân viên";
                worksheet.Cells[1, 3].Value = "Tên nhân viên";
                worksheet.Cells[1, 4].Value = "Giới tính";
                worksheet.Cells[1, 5].Value = "Số điện thoại";
                worksheet.Cells[1, 6].Value = "CCCD";
                worksheet.Cells[1, 7].Value = "Email";
                worksheet.Cells[1, 8].Value = "Địa chỉ";
                worksheet.Cells[1, 9].Value = "Chức vụ";
                worksheet.Cells[1, 10].Value = "Lương";
                for(int i = 0; i < staffTable.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = i + 1;
                    worksheet.Cells[i + 2, 2].Value = staffTable[i].StaffID;
                    worksheet.Cells[i + 2, 3].Value = staffTable[i].FullName;
                    if (staffTable[i].Gender.HasValue)
                    {
                        if (staffTable[i].Gender == true)
                        {
                            worksheet.Cells[i + 2, 4].Value = "Nam";
                        }
                        else
                        {
                            worksheet.Cells[i + 2, 4].Value = "Nữ";
                        }
                    }
                    else
                    {
                        worksheet.Cells[i + 2, 4].Value = "Không xác định";
                    }
                    worksheet.Cells[i + 2, 5].Value = staffTable[i].PhoneNumber;
                    worksheet.Cells[i + 2, 6].Value = staffTable[i].IdentityCard;
                    worksheet.Cells[i + 2, 7].Value = staffTable[i].Email;
                    worksheet.Cells[i + 2, 8].Value = staffTable[i].Address;
                    worksheet.Cells[i + 2, 9].Value = staffTable[i].Position;
                    worksheet.Cells[i + 2, 10].Value = staffTable[i].Salary;
                }
                var memoryStream = new MemoryStream();
                package.SaveAs(memoryStream);
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "file.xlsx");
            }
        }

	}
}
