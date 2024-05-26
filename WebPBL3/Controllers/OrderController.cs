using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPBL3.DTO;
using WebPBL3.Models;
using System.Text.Json;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace WebPBL3.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext db)
        {
            _context = db;
        }
        public IActionResult Index(int page = 1)
        {
            string orderJson = TempData["orders"] as string;
            if(TempData["status"] != null)
            {
                ViewBag.status = TempData["status"] as string;
                
            }
            if (TempData["idUser"] != null)
            {
                ViewBag.idUser = TempData["idUser"] as string;
            }
            List<Order> orders;
            if (orderJson != null)
            {
                orders = JsonConvert.DeserializeObject<List<Order>>(orderJson);
            }
            else
            {
                orders = _context.Orders.ToList();
            }
            double total = orders.Count;
            var totalPage = (int)Math.Ceiling(total / 10);
            if (page < 1)
                page = 1;
            if (page > totalPage) 
                page = totalPage;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;
            orders = orders.Skip((page - 1) * 10).Take(10).ToList();
            return View(orders);
        }
        public IActionResult Creat() 
        {
            OrderDTO orderDTO;
            string orderDTOJson = TempData["orderDTO"] as string;
            if (orderDTOJson == null)
            {
                orderDTO = new OrderDTO();
            }
            else
            {
                orderDTO = JsonConvert.DeserializeObject<OrderDTO>(orderDTOJson);
            }
            return View(orderDTO);
        }
        [HttpPost]
        public IActionResult ExtractEmail(string existEmail)
        {
            User? u = _context.Users
                .Include(a => a.Account)
                .Include(w=>w.Ward.District)
                .FirstOrDefault(u => u.Account.Email == existEmail);
            if (u != null)
            {
                OrderDTO orderDTO = new OrderDTO
                {
                    FullName = u.FullName,
                    IdentityCard = u.IdentityCard,
                    PhoneNumber = u.PhoneNumber,
                    Email = existEmail,
                    Address = u.Address,
                    WardID = u.WardID??0,
                    ProvinceID = u.Ward!= null ? u.Ward.District.ProvinceID : 0,
                    DistrictID = u.Ward != null ? u.Ward.DistrictID : 0,
                };
                string orderDTOJson = JsonConvert.SerializeObject(orderDTO);
                TempData["orderDTO"] = orderDTOJson;
            }
            
            return RedirectToAction("Creat");
        }
        [HttpPost]
        public IActionResult AddItem([FromBody] Items item)
        {
            var car = _context.Cars.Find(item.carID);
            if (car == null)
            {
                return NotFound("Mã xe không tồn tại");
            }
            if (car.Quantity < item.quantity)
            {
                return BadRequest("Số lượng xe không đủ");
            }
            item.carName = car.CarName;
            item.price = car.Price;
            item.color = car.Color;
            Console.WriteLine(Json(item));
            return Json(item);   
        }
        //[HttpPost]
        //public IActionResult DeleteItem(OrderDTO orderDTO, string delItemId)
        //{
        //    orderDTO.items.RemoveAll(item => item.carID == delItemId);
        //    string orderDTOJson = JsonConvert.SerializeObject(orderDTO);
        //    TempData["orderDTO"] = orderDTOJson;
        //    return RedirectToAction("Creat");
        //}
        [HttpPost]
        public async Task<IActionResult> Creat(OrderDTO orderDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(orderDTO);
            }
            User? u = _context.Users
                .Include(a => a.Account)
                .FirstOrDefault(u => u.Account.Email == orderDTO.Email);
            Staff? staff=_context.Staffs
                .Include(u=>u.User.Account)
                .FirstOrDefault(u=>u.User.Account.Email== HttpContext.User.FindFirstValue(ClaimTypes.Name));
            if(u == null)
            {
                var account_id = 1;
                var lastAccount = _context.Accounts.OrderByDescending(a => a.AccountID).FirstOrDefault();
                if (lastAccount != null)
                {
                    account_id = Convert.ToInt32(lastAccount.AccountID) + 1;
                }
                var accountID= account_id.ToString().PadLeft(8, '0');
                Account a = new Account
                {
                    Email = orderDTO.Email,
                    AccountID = accountID,
                    Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Status = true,
                    RoleID = 1,
                };
                _context.Accounts.Add(a);
                var user_id = 1;
                var lastUser = _context.Users.OrderByDescending(u => u.UserID).FirstOrDefault();
                if (lastUser != null)
                {
                    user_id = Convert.ToInt32(lastUser.UserID.Substring(2)) + 1;
                }
                var userID = "KH" + user_id.ToString().PadLeft(6, '0');
                u = new User
                {
                    UserID =userID,
                    FullName = orderDTO.FullName,
                    Address = orderDTO.Address,
                    IdentityCard = orderDTO.IdentityCard,
                    PhoneNumber = orderDTO.PhoneNumber,
                    AccountID=a.AccountID,
                    WardID=orderDTO.WardID
                };
                _context.Users.Add(u);
            }
            else
            {
                u.FullName = orderDTO.FullName;
                u.Address = orderDTO.Address;
                u.IdentityCard = orderDTO.IdentityCard;
                u.PhoneNumber = orderDTO.PhoneNumber;
                u.WardID = orderDTO.WardID;
                _context.Users.Update(u);
            }
            var order_id = 1;
            var lastOrder = _context.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault();
            if (lastOrder != null)
            {
                order_id = Convert.ToInt32(lastOrder.OrderID.Substring(2)) + 1;
            }
            var orderID = "DH" + order_id.ToString().PadLeft(6, '0');
            Order order = new Order
            {
                OrderID = orderID,
                Date = orderDTO.Date,
                Totalprice = orderDTO.Totalprice,
                Status = orderDTO.Status,
                Flag = true,
                UserID = u.UserID,
                StaffID = staff.StaffID
            };
            _context.Orders.Add(order);
            foreach (var item in orderDTO.items)
            {
                DetailOrder detailOrder = new DetailOrder
                {
                    DetailOrderID = Guid.NewGuid().ToString().Substring(0, 10),
                    Quantity = item.quantity,
                    Price = item.price,
                    OrderID = order.OrderID,
                    CarID = item.carID
                };
                var c=_context.Cars.Find(item.carID);
                c.Quantity -= item.quantity;
                _context.DetailOrders.Add(detailOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public JsonResult GetProvince()
        {
            var provinces = _context.Provinces.ToList();
            return new JsonResult(provinces);
        }

        public JsonResult GetDistrict(int id)
        {
            var districts = _context.Districts.Where(d => d.ProvinceID == id).Select(d => new { id = d.DistrictID, name = d.DistrictName }).ToList();
            return new JsonResult(districts);
        }

        public JsonResult GetWard(int id)
        {
            var wards = _context.Wards.Where(w => w.DistrictID == id).Select(w => new { id = w.WardID, name = w.WardName }).ToList();
            return new JsonResult(wards);
        }
        public IActionResult Detail(string id)
        {
            Order? order = _context.Orders
                .Include(u => u.User.Account)
                .Include(st => st.Staff.User.Account)
                .FirstOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return NotFound();
            }
            var details = _context.DetailOrders
                     .Include(c => c.Car)
                     .Where(o => o.OrderID == id)
                     .ToList();
            DetailOrderDTO detailOrderDTO = new DetailOrderDTO
            {
                OrderId = order.OrderID,
                CustomerName = order.User.FullName,
                Address = order.User.Address,
                EmailCustomer = order.User.Account.Email,
                Phone = order.User.PhoneNumber,
                StaffId = order.Staff.StaffID,
                StaffName = order.Staff.User.FullName,
                EmailStaff = order.Staff.User.Account.Email,
                PurchaseDate=order.Date,
                Status = order.Status,
                ToTalPrice=order.Totalprice
            };
            foreach (var item in details)
            {
                detailOrderDTO.items.Add(new Items
                {
                    carID= item.CarID,
                    carName=item.Car.CarName,
                    color=item.Car.Color,
                    price=item.Price,
                    quantity=item.Quantity,
                });
            }
            return View(detailOrderDTO);
        }
        public async Task<IActionResult> DeleteOrder(string id)
        {
            Order o=_context.Orders.FirstOrDefault(o=>o.OrderID == id);
            if (o != null)
            {
                _context.Orders.Remove(o);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> EditOrder(string id)
        {
            Order o = _context.Orders.FirstOrDefault(o => o.OrderID == id);
            if (o != null) 
            {
                o.Status = "Đã thanh toán";
                _context.Update(o);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        public IActionResult FilterOrder(string status,string idUser)
        {
            List<Order> orders=_context.Orders.Where(o=>(status.IsNullOrEmpty()||o.Status.Contains(status))&&(idUser.IsNullOrEmpty()||o.UserID.Contains(idUser))).ToList();
            string orderJson = JsonConvert.SerializeObject(orders);
            TempData["orders"] = orderJson;
            TempData["status"]=status;
            TempData["idUser"] = idUser;
            return RedirectToAction("Index");
        }
    }
}
