using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using WebPBL3.DTO;
using WebPBL3.Models;
using WebPBL3.ViewModel;

namespace WebPBL3.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private IWebHostEnvironment _environment;
        public AccountController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
           _environment = environment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Accounts.Include(u => u.Role).FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                    {
                        var claims = new List<Claim>()
                        {
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim(ClaimTypes.Role,user.Role.RoleName)
                        };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["Error"] = "Tài khoản hoặc mật khẩu không hợp lệ";
                        return View();
                    }
                }
                TempData["Error"] = "Tài khoản hoặc mật khẩu không hợp lệ";
                return View();
            }
            else
            {
                return View();
            }
        }
        public IActionResult Register()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.RetypePassword)
                {
                    TempData["Error"] = "Mật khẩu nhập lại không khớp";
                    return View();
                }
                var currentAccount = _db.Accounts.FirstOrDefault(u => u.Email == model.Email);
                if (currentAccount != null)
                {
                    TempData["Error"] = "Email đã tồn tại";
                    return View();
                }
                Role role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                var account_id = 1;
                var lastAccount = _db.Accounts.OrderByDescending(a => a.AccountID).FirstOrDefault();
                if (lastAccount != null)
                {
                    account_id = Convert.ToInt32(lastAccount.AccountID) + 1;
                }
                var accountID = account_id.ToString().PadLeft(8, '0');
                var newAccount = new Account
                {
                    AccountID = accountID,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Status = true,
                    RoleID = role.RoleID,
                };
                _db.Accounts.Add(newAccount);
                var user_id = 1;
                var lastUser = _db.Users.OrderByDescending(u => u.UserID).FirstOrDefault();
                if (lastUser != null)
                {
                    user_id = Convert.ToInt32(lastUser.UserID.Substring(2)) + 1;
                }
                var userID = "KH" + user_id.ToString().PadLeft(6, '0');
                var user = new User
                {
                    UserID = userID,
                    AccountID = newAccount.AccountID
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View();
        }
        
        public IActionResult Denied()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var existAccount = _db.Accounts.FirstOrDefault(u => u.Email == email);
            if (existAccount!=null)
            {
                string otp = GenerateOTP();
                HttpContext.Session.SetString("OTP",otp);
                HttpContext.Session.SetString("Email", email);
                string message = "Mã OTP: " + otp;
                existAccount.Password= BCrypt.Net.BCrypt.HashPassword(otp);
                _db.Accounts.Update(existAccount);
                await _db.SaveChangesAsync();
                await SendMailGoogleSmtp(email, "Thiết lập mật khẩu mới", message);
                return RedirectToAction("setupNewPassword");
            }
            else
            {
                TempData["Error"] = "Email không tồn tại!";
                return View();
            }
        }
        public static async Task SendMailGoogleSmtp( string _to, string _subject,
                                                            string _body)
        {
            string _mail = "thanhpnwork22@gmail.com";
            string _pw = "imid wxgq ttvd ndaz";
            MailMessage message = new MailMessage(
                from: _mail,
                to: _to,
                subject: _subject,
                body: _body
            );
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = true;
            var client = new SmtpClient("smtp.gmail.com",587)
            {
                Credentials = new NetworkCredential(_mail, _pw),
                EnableSsl = true
            };  
            await client.SendMailAsync(message);
        }
        private string GenerateOTP()
        {
            Random random = new Random();
            string result = "";
            for (int i = 0; i < 6; i++)
            {
                result += random.Next(0, 10);
            }
            return result;
        }
        public IActionResult setupNewPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> setupNewPassword(string otp, string password,
            string retypepassword)
        {
            if (password != retypepassword)
            {
                TempData["Error"] = "Mật khẩu nhập lại không khớp";
                return View();
            }
            var sessionValue = HttpContext.Session.GetString("OTP");
            if (sessionValue != otp &&otp != null)
            {
                TempData["Error"] = "Mã OTP không hợp lệ";
                return View();
            }
            var email = HttpContext.Session.GetString("Email");
            if (email == null)
            {
                TempData["Error"] = "Xảy ra lỗi khi truyền dữ liệu. Vui lòng thao tác lại";
                return View();
            }
            var existAccount = _db.Accounts.FirstOrDefault(u => u.Email == email);
            if (existAccount != null)
            {
                existAccount.Password = BCrypt.Net.BCrypt.HashPassword(password);
                _db.Accounts.Update(existAccount);
                await _db.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View();
        }
        public IActionResult InforAccount()
        {
            string email = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            if (email == null)
            {
                return RedirectToAction("Login");
            }
            Account account = _db.Accounts.Include(a => a.User.Ward.District).FirstOrDefault(a => a.Email == email);
            if (account == null)
            {
                return RedirectToAction("Login");
            }
            UserDto user = new UserDto
            {
                UserID=account.User.UserID,
                Email = account.Email,
                FullName = account.User.FullName,
                PhoneNumber = account.User.PhoneNumber,
                IdentityCard = account.User.IdentityCard,
                Gender = account.User.Gender,
                BirthDate = account.User.BirthDate,
                Address = account.User.Address,
                Photo = account.User.Photo,
                WardID = account.User.Ward!=null ? account.User.WardID : 0,
                ProvinceID= account.User.Ward != null ? account.User.Ward.District.ProvinceID : 0,
                DistrictID =account.User.Ward != null ? account.User.Ward.DistrictID : 0
            };
            if (account.RoleID == 2)
            {
                var staff = _db.Staffs.FirstOrDefault(s => s.UserID == account.User.UserID);
                ViewBag.Staff = staff;
            }
            if (_db.Orders.Any(o => o.UserID == account.User.UserID))
            {
                ViewBag.Unchange = true;
            }
            else
            {
                ViewBag.Unchange = false;
            }
            
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> InforAccount(UserDto userDto, IFormFile? uploadimage)
        {
            User user = _db.Users.Find(userDto.UserID);
            if (user == null)
            {
                return NotFound();
            }
            user.FullName = userDto.FullName;
            user.PhoneNumber = userDto.PhoneNumber;
            user.IdentityCard = userDto.IdentityCard;
            user.Gender = userDto.Gender;
            user.BirthDate = userDto.BirthDate;
            if (!_db.Orders.Any(o => o.UserID == user.UserID))
            {
                user.Address = userDto.Address;
                user.WardID = userDto.WardID;
            } 
            if (uploadimage != null && uploadimage.Length > 0)
            {
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(uploadimage.FileName);
                user.Photo = fileName;
                string _path = Path.Combine(_environment.WebRootPath, "upload\\user", fileName);
                Console.WriteLine(_path);
                using (var fileStream = new FileStream(_path, FileMode.Create))
                {
                    uploadimage.CopyTo(fileStream);
                }
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string password, string newPassword, string retypePassword)
        {
            if (newPassword != retypePassword)
            {
                TempData["Error"] = "Mật khẩu nhập lại không khớp";
                return View();
            }
            string email = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            Account account = _db.Accounts.FirstOrDefault(a => a.Email == email);
            if (account == null)
            {
                return RedirectToAction("Login");
            }
            if(!BCrypt.Net.BCrypt.Verify(password, account.Password))
            {
                TempData["Error"] = "Mật khẩu không chính xác";
                return View();
            }
            account.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [Authorize(Policy = "User")]
        // [GET]
        public async Task<IActionResult> HistoryOrder()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return BadRequest("Người dùng chưa đăng nhập");
            }
            string? email = User.Identity.Name;
            Account? account = await _db.Accounts.Include(a => a.User).FirstOrDefaultAsync(a => a.Email == email);

            if (account == null)
            {
                return NotFound("Account is not found");

            }


            User? user = account.User;

            if (user == null)
            {
                return NotFound("User is not found");
            }
            List<Order> orders = await _db.Orders.Where(o => o.UserID == user.UserID)
                .Include(o => o.DetailOrders)
                .ThenInclude(d => d.Car).ThenInclude(c => c.Make).OrderByDescending(o => o.Date).ToListAsync();


            List<HistoryOrderDto> historyOrders = new List<HistoryOrderDto>();

            foreach (var item in orders)
            {
                HistoryOrderDto historyOrder = new HistoryOrderDto
                {
                    Date = item.Date,
                    Totalprice = item.Totalprice,
                    Status = item.Status,
                    OrderID = item.OrderID,
                };
                foreach (var itemDetailOrder in item.DetailOrders)
                {
                    historyOrder.Items.Add(new HistoryOrderItem
                    {

                        CarID = itemDetailOrder.CarID,
                        Photo = itemDetailOrder.Car.Photo,
                        CarName = itemDetailOrder.Car.CarName,
                        MakeName = itemDetailOrder.Car.Make.MakeName,
                        Color = itemDetailOrder.Car.Color,
                        Price = itemDetailOrder.Car.Price,
                        Quantity = itemDetailOrder.Quantity
                    });
                }
                historyOrders.Add(historyOrder);
            }
            //Console.WriteLine(historyOrders.Count);
            return View(historyOrders);
        }
    }
}

