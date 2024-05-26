using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DiaSymReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Numerics;
using WebPBL3.Models;

namespace WebPBL3.Controllers
{
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
        public IActionResult UserListTable(string searchtxt ="", int page = 1)
        {
            if (!TempData.ContainsKey("wards"))
            {
                List<Ward> wards = _db.Wards.ToList();
                List<District> districts = _db.Districts.ToList();
                List<Province> provinces = _db.Provinces.ToList();
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                TempData["wards"] = wards;
                TempData["wards"] = JsonConvert.SerializeObject(wards, settings);
                TempData.Keep("wards");
                TempData["districts"] = districts;
                TempData["districts"] = JsonConvert.SerializeObject(districts, settings);
                TempData.Keep("districts");
                TempData["provinces"] = provinces;
                TempData["provinces"] = JsonConvert.SerializeObject(provinces, settings);
                TempData.Keep("provinces");
                
             

            }

            List<UserDto> users = _db.Users.Include(a => a.Account).Include(w => w.Ward).Where(u => (searchtxt.IsNullOrEmpty() || u.FullName.Contains(searchtxt))).Select(u => new UserDto
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

            }).ToList();
            var total = users.Count;
            var totalPage = (total + limits - 1) / limits;
            if (page < 1) page = 1;
            if (page > totalPage) page = totalPage;
            ViewBag.totalRecord = total;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;
            ViewBag.searchtxt = searchtxt;
            users = users.Skip((page - 1) * limits).Take(limits).ToList();
            int cnt = 0;
            foreach (var user in users)
            {
                user.STT = ++cnt;
            }
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
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Error: {error.ErrorMessage}");
            }
            if (ModelState.IsValid)

            {   
                // Account
                var accid = 1;
                var lastAcc = _db.Accounts.OrderByDescending(a => a.AccountID).FirstOrDefault();
                if (lastAcc != null)
                {
                    accid = Convert.ToInt32(lastAcc.AccountID)+1;
                }
                Console.WriteLine(accid + user.Email);
                var accidTxt = accid.ToString().PadLeft(8,'0');
                user.AccountID = accidTxt;
                var accWithEmail = _db.Accounts.FirstOrDefault(u => u.Email == user.Email);
                if (accWithEmail != null)
                {
                    TempData["Error"] = "Email đã tồn tại";
                    return View(user);
                }
                // Add Account
                _db.Accounts.Add(new Account
                {
                    AccountID = user.AccountID,
                    Email = user.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Status = false,
                    RoleID = 3,
                });
                await _db.SaveChangesAsync();
                // User
                var userid = 1;
                var lastUser = _db.Users.OrderByDescending(u => u.UserID).FirstOrDefault();
                if (lastUser != null)
                {
                    userid = Convert.ToInt32(lastUser.UserID.Substring(2)) + 1;
                }
                var useridTxt = "KH" + userid.ToString().PadLeft(6, '0');
                user.UserID = useridTxt;
                if (user.Photo.IsNullOrEmpty()) user.Photo = "userKH000000.jpg";
                else
                {
                    int index = uploadimage.FileName.IndexOf('.');
                    string _FileName = "user" + user.UserID + "." + uploadimage.FileName.Substring(index + 1);
                    user.Photo = _FileName;
                }

                string _path = Path.Combine(_environment.WebRootPath, "upload\\user", user.Photo);
                using (var fileStream = new FileStream(_path, FileMode.Create))
                {
                    uploadimage.CopyTo(fileStream);

                }
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
                
                return RedirectToAction("UserListTable");

            }
            return View(user);
        }
        // GET
        public IActionResult Edit(string ?id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            User? user = _db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            Account? account = _db.Accounts.Find(user.AccountID);
            if (account == null)
            {
                return NotFound();
            }
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
        public async Task<IActionResult>  Edit(UserDto user, IFormFile? uploadimage)
        {
            if (ModelState.IsValid)
            {
                User _user = _db.Users.Where(u => u.UserID == user.UserID).FirstOrDefault();
                if (uploadimage != null && uploadimage.Length > 0)
                {
                    int index = uploadimage.FileName.IndexOf('.');

                    string _FileName = "user" + user.UserID + "." + uploadimage.FileName.Substring(index + 1);
                    user.Photo = _FileName;
                    string _path = Path.Combine(_environment.WebRootPath, "upload\\user", _FileName);
                    Console.WriteLine(_path);
                    using (var fileStream = new FileStream(_path, FileMode.Create))
                    {
                        uploadimage.CopyTo(fileStream);

                    }
                }


                _user.FullName = user.FullName;
                _user.PhoneNumber = user.PhoneNumber;
                _user.IdentityCard = user.IdentityCard;
                _user.Gender = user.Gender;
                _user.Address = user.Address;
                _user.BirthDate = user.BirthDate;
                _user.Photo = user.Photo;
                _user.WardID = user.WardID;


                try
                {
                    _db.Users.Update(_user);

                    await _db.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    NotFound();

                }


                return RedirectToAction("UserListTable");
            }
            return View(user);
        }
        // GET
        public IActionResult Details(string? id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            User? user = _db.Users.Find(id);

        if (user == null)
        {
            return NotFound();
        }

        Account account = _db.Accounts.Where(a => a.AccountID == user.AccountID).FirstOrDefault();
            string wardName = string.Empty, districtName = string.Empty, provinceName = string.Empty;
            Ward ward = _db.Wards.Where(w => w.WardID == user.WardID).FirstOrDefault();
            if (ward != null)
            {   
                wardName = ward.WardName;
                District district = _db.Districts.Where(d => d.DistrictID == ward.DistrictID).FirstOrDefault();
                if (district != null)
                {
                    districtName = district.DistrictName;
                    Province province = _db.Provinces.Where(p => p.ProvinceID == district.ProvinceID).FirstOrDefault();
                    if (province != null)
                    {
                        provinceName = province.ProvinceName;
                    }
                }
            }
            Console.WriteLine(user.Photo);
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
        public IActionResult Delete(string ?id)
        {
            Console.WriteLine(id);
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            
            User user = _db.Users.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return NotFound();
            }
            Account account = _db.Accounts.FirstOrDefault(a => a.AccountID == user.AccountID);
            if (account == null)
            {
                return NotFound();
            }

           
            try
            {
                _db.Users.Remove(user);
                _db.Accounts.Remove(account);
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                NotFound();

            }
            return RedirectToAction("UserListTable");
          
        }
    }
}
