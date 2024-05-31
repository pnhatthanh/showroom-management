using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebPBL3.DTO;
using WebPBL3.Models;

namespace WebPBL3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;   
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.HideHeader = false;
            List<Order> orders = await _db.Orders
                .Include(o => o.DetailOrders)
                .ThenInclude(deo => deo.Car)
                .ThenInclude(c => c.Make)
                .Where(o => o.Status == "Đã thanh toán")
                .ToListAsync();
            Dictionary<string, int> quantity = new Dictionary<string, int>();
            foreach (var order in orders)
            {
                foreach (var detailOrder in order.DetailOrders)
                {
                    if (!quantity.ContainsKey(detailOrder.Car.CarID))
                    {
                        quantity[detailOrder.Car.CarID] = 0;
                    }
                    quantity[detailOrder.Car.CarID] += detailOrder.Quantity;  
                }
            }
            var sortedDict = quantity.OrderBy(q => q.Value).Take(4).ToList();
            List<CarVM> cars = new List<CarVM>();
            /*foreach (var item in sortedDict) {
                Car? _car = await _db.Cars.Include(c => c.Make).FirstOrDefaultAsync(c => c.CarID == item.Key);


                cars.Add(new CarVM
                {
                    CarID = item.Key,
                    CarName = _car.CarName,
                    MakeName = _car.Make.MakeName,
                    Price = _car.Price,
                    Photo = _car.Photo
                });
                 
                
            }*/
            for (int i=0; i<4; i++) {
                cars.Add(new CarVM
                {
                    CarID = "1",
                    CarName = "City G",
                    MakeName = "Honda",
                    Price = 500000000,
                    Photo = "carOT000001.jpg"
                }) ;    
            }

            List<Feedback> _feedBacks = await _db.Feedbacks.Include(fb => fb.User).Take(5).ToListAsync();


            /*foreach (var item in _feedBacks)
            {
                feedBacks.Add(new FeedbackVM
                {
                    FullName = item.FullName,
                    Title = item.Title,
                    Content = item.Content,
                });
            }*/

            List<FeedbackVM> feedBacks = new List<FeedbackVM>();
            feedBacks.Add(new FeedbackVM
             {
                 FullName = "Đặng Phúc Long",
                 Title = "Khách hàng Honda",
                 Content = "aa aa a a a a a a a       aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                 Photo = "userKH000001.png"
             });
            feedBacks.Add(new FeedbackVM
            {
                FullName = "Đặng Phúc Long",
                Title = "Khách hàng Honda",
                Content = "aa aa a a a a a a a       aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                Photo = "userKH000001.png"
            });
            feedBacks.Add(new FeedbackVM
            {
                FullName = "Đặng Phúc Long",
                Title = "Khách hàng Honda",
                Content = "aa aa a a a a a a a       aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                Photo = "userKH000001.png"
            });
            feedBacks.Add(new FeedbackVM
            {
                FullName = "Đặng Phúc Long",
                Title = "Khách hàng Honda",
                Content = "aa aa a a a a a a a       aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                Photo = "userKH000001.png"
            });
            feedBacks.Add(new FeedbackVM
            {
                FullName = "Đặng Phúc Long",
                Title = "Khách hàng Honda",
                Content = "aa aa a a a a a a a       aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
                Photo = "userKH000001.png"
            });


            List<News> _news = await _db.NewS.Include(n => n.Staff).ThenInclude( s => s.User).OrderByDescending(s => s.CreateAt).Take(3).ToListAsync();
            List<NewVM> news = new List<NewVM>();
            /*foreach (var item in _news)
            {
                news.Add(new NewVM
                {   

                    NewsID = item.NewsID,
                    Photo = item.Photo,
                    FullName = item.Staff.User.FullName,
                    Title = item.Title,
                    CreateAt = item.CreateAt,
                });
            }*/
            for (int i=0; i<3; i++)
            {
                news.Add(new NewVM
                {   

                    NewsID = "new000003",
                    Photo = "new000003.jpg",
                    FullName = "Đặng Phúc Long",
                    Title = "Báo lá cải",
                    CreateAt = DateTime.Now,
                });
            }
            ViewBag.news = news;
            ViewBag.cars = cars; 
            ViewBag.feedBacks = feedBacks;
            return View();

        }

        public IActionResult About()
        {
            ViewBag.HideHeader = false;
            return View(); 
        }
        public IActionResult Contact()
        {
            ViewBag.HideHeader = false;
            List<Province> provinces = _db.Provinces.ToList();
            return View(provinces);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
