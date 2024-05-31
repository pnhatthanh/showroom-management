using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DiaSymReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Linq;
using System.Net;
using System.Numerics;
using WebPBL3.DTO;
using WebPBL3.Models;

namespace WebPBL3.Controllers
{
    [Authorize(Roles = "Admin,Staff")]   
    
    public class UserController : Controller
    {
        ApplicationDbContext _db;
        private IWebHostEnvironment _environment;
        private int limits = 10;
        public UserController (ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }
        public IActionResult Index()
        {
            return View();
        }
        // GET
        public async Task<IActionResult> UserListTable(string searchtxt = "",int fieldsearch = 1, int page = 1)
        {
            

            List<UserDto> users =  await _db.Users
                .Include(a => a.Account)
                .Include(w => w.Ward)
                .ThenInclude(d => d.District)
                .Where(u => u.Account.RoleID == 3 &&  (searchtxt.IsNullOrEmpty() 
                || (fieldsearch == 1 && u.FullName.Contains(searchtxt)) 
                || (fieldsearch == 2 && u.PhoneNumber.Contains(searchtxt))
                || (fieldsearch == 3 && u.Account.Email.Contains(searchtxt))
                || (fieldsearch == 4 && u.IdentityCard.Contains(searchtxt))))
                .Select(u => new UserDto
               {
                AccountID = u.AccountID,
                Email = u.Account.Email,
                Password = u.Account.Password,
                Status = u.Account.Status,

                RoleID = 3,
                UserID = u.UserID,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                IdentityCard = u.IdentityCard,

                Gender = u.Gender,
                BirthDate = u.BirthDate,
                Address = u.Address,
                Photo = u.Photo,
                WardID = u.WardID,
                WardName = u.Ward.WardName,
                DistrictName = u.Ward.District.DistrictName,
                ProvinceName = u.Ward.District.Province.ProvinceName,

            }).ToListAsync();
            // tổng số  người dùng
            int total = users.Count;
            // tổng số trang
            int totalPage = (total + limits - 1) / limits;

            if (page < 1) page = 1;
            if (page > totalPage) page = totalPage;
            ViewBag.totalRecord = total;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;
            ViewBag.searchtxt = searchtxt;
            ViewBag.fieldsearch = fieldsearch;
            users = users.Skip((page - 1) * limits).Take(limits).ToList();
            
           
            return View(users);
        }
        // GET
        public IActionResult Create()
        {
            
            return View();
        }
        // POST
        [HttpPost]
        public async Task<IActionResult> Create(UserDto user, IFormFile? uploadimage)
        {
            
            if (ModelState.IsValid)

            {   
                // Account
                int accid = 1;

                Account? lastAcc = await _db.Accounts.OrderByDescending(a => a.AccountID).FirstOrDefaultAsync();
                if (lastAcc != null)
                {
                    accid = Convert.ToInt32(lastAcc.AccountID)+1;
                }
                //Console.WriteLine(accid + user.Email);
                var accidTxt = accid.ToString().PadLeft(8,'0');
                user.AccountID = accidTxt;
                Account? accWithEmail =  await _db.Accounts.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (accWithEmail != null)
                {
                    TempData["Error"] = "Email đã tồn tại";
                    return View(user);
                }
                // Add Account
                try
                {
                    _db.Accounts.Add(new Account
                    {
                        AccountID = user.AccountID,
                        Email = user.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Status = false,
                        RoleID = 3,
                    });
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // 404
                    return BadRequest("Error add account: " + ex.Message);

                }
                
                // User
                var userid = 1;
                var lastUser = _db.Users.OrderByDescending(u => u.UserID).FirstOrDefault();
                if (lastUser != null)
                {
                    userid = Convert.ToInt32(lastUser.UserID.Substring(2)) + 1;
                }
                var useridTxt = "KH" + userid.ToString().PadLeft(6, '0');
                user.UserID = useridTxt;
                if (uploadimage != null && uploadimage.Length > 0)
                {
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newFileName += Path.GetExtension(uploadimage!.FileName);
                    user.Photo = newFileName;
                    string imageFullPath = Path.Combine(_environment.WebRootPath, "upload\\user", newFileName);
                    using (var fileStream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await uploadimage.CopyToAsync(fileStream);

                    }
                }
            

                //Console.WriteLine("Ward: " + user.WardID);
                try
                {
                    _db.Users.Add(new User
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        IdentityCard = user.IdentityCard,
                        Gender = user.Gender,
                        Address = user.Address,
                        BirthDate = user.BirthDate,
                        Photo = user.Photo,
                        WardID = user.WardID,
                        AccountID = user.AccountID,

                    });

                    await _db.SaveChangesAsync();
                } catch (DbUpdateException ex)
                {
                    return BadRequest("Error add user: " + ex.Message);
                }   
                
                return RedirectToAction("UserListTable");

            }
            return View(user);
        }
        // GET
        public async Task<IActionResult> Edit(string ?id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Id is null");
            }
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound("User is not found");
            }

            Account? account = await _db.Accounts.FirstOrDefaultAsync(a => a.AccountID == user.AccountID);
            if (account == null)
            {
                return NotFound("Account is not found");
            }

            
            Ward? ward = await _db.Wards.Where(w => w.WardID == user.WardID).FirstOrDefaultAsync();
            ViewBag.DistrictID = 0;
            ViewBag.ProvinceID = 0;
            if (ward != null)
            {
                District? district = await _db.Districts.FirstOrDefaultAsync(d => d.DistrictID == ward.DistrictID);
                if (district != null)
                {
                    ViewBag.DistrictID = district.DistrictID;
                    ViewBag.ProvinceID = district.ProvinceID;
                }
            } else user.WardID = 0;
            
            UserDto userDtoFromDb = new UserDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Password = account.Password,
                Email = account.Email,
                PhoneNumber = user.PhoneNumber,
                IdentityCard = user.IdentityCard,
                Gender = user.Gender,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Photo = user.Photo,
                WardID = user.WardID,
                AccountID = user.AccountID,

            };
            return View(userDtoFromDb);
        }
        

        // POST
        [HttpPost]
        public async Task<IActionResult> Edit(UserDto userdto, IFormFile? uploadimage)
        {
            if (ModelState.IsValid)
            {
                User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == userdto.UserID);
                if (user == null)
                {
                    return NotFound("User is not found");
                }
                if (uploadimage != null && uploadimage.Length > 0)
                {
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newFileName += Path.GetExtension(uploadimage!.FileName);
                    if (!userdto.Photo.IsNullOrEmpty())
                    {
                        string oldImageFullPath = Path.Combine(_environment.WebRootPath, "upload\\user", userdto.Photo);
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }
                    userdto.Photo = newFileName;
                    string imageFullPath = Path.Combine(_environment.WebRootPath, "upload\\user", newFileName);
                    using (var fileStream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await uploadimage.CopyToAsync(fileStream);

                    }
                }

                user.FullName = userdto.FullName;
                user.PhoneNumber = userdto.PhoneNumber;
                user.IdentityCard = userdto.IdentityCard;
                user.Gender = userdto.Gender;
                user.Address = userdto.Address;
                user.BirthDate = userdto.BirthDate;
                user.Photo = userdto.Photo;
                user.WardID = (userdto.WardID>0?userdto.WardID:null);


                try
                {
                    _db.Users.Update(user);

                    await _db.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return BadRequest("Error edit user: " + ex.Message);

                }


                return RedirectToAction("UserListTable");
            }
            return View(userdto);
        }
        // GET
        public async Task<IActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Id is null");
            }
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound("User is not found");
            }

            Account? account =  await _db.Accounts.FirstOrDefaultAsync(a => a.AccountID == user.AccountID);
            if (account == null)
            {
                return NotFound("Account is not found");
            }
            string wardName = string.Empty, districtName = string.Empty, provinceName = string.Empty;
            Ward? ward = await _db.Wards.Where(w => w.WardID == user.WardID).FirstOrDefaultAsync();
            
            if (ward != null)
            {   
                wardName = ward.WardName;
                District? district = await _db.Districts.FirstOrDefaultAsync(d => d.DistrictID == ward.DistrictID);
                if (district != null)
                {
                    districtName = district.DistrictName;
                    Province? province = await _db.Provinces.FirstOrDefaultAsync(p => p.ProvinceID == district.ProvinceID);
                    if (province != null)
                    {
                        provinceName = province.ProvinceName;
                    }
                }
            }
            
            UserDto userFromDb = new UserDto
            {
                UserID = user.AccountID,
                FullName = user.FullName,
                Email = account.Email,
                PhoneNumber = user.PhoneNumber,
                IdentityCard = user.IdentityCard,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                Address = user.Address,
                ProvinceName = provinceName,
                DistrictName = districtName,
                WardName = wardName,  
                Photo = user.Photo,
            };

            return View(userFromDb);
        }
        [HttpGet]
        public async Task<IActionResult>  Delete(string ?id)
        {
            //Console.WriteLine(id);
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Id is null");
            }
            
            User? user =  await _db.Users.FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null)
            {
                return NotFound("User is not found");
            }
            Account? account = await _db.Accounts.FirstOrDefaultAsync(a => a.AccountID == user.AccountID);
            if (account == null)
            {
                return NotFound("Account is not found");
            }

           
            try
            {
                _db.Users.Remove(user);
                _db.Accounts.Remove(account);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                return BadRequest("Error delete user and account: " + ex.Message);

            }
            return RedirectToAction("UserListTable");
          
        }

        
    }
}
