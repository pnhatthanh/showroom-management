using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TextTemplating;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using WebPBL3.Models;
using WebPBL3.DTO;
using Microsoft.AspNetCore.Authorization;
namespace WebPBL3.Controllers
{
    public class CarController : Controller
    {
        private ApplicationDbContext _db;
        private IWebHostEnvironment _environment;
        // Số lượng item mỗi trang
        private int limits = 10;
        public CarController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string searchTerm = "")
        {
			ViewBag.HideHeader = false;
			ViewBag.SearchTerm = searchTerm;

            var makes = await _db.Makes.ToListAsync();
            var origins = await _db.Cars.Select(c => c.Origin).Distinct().ToListAsync();
            var colors = await _db.Cars.Select(c => c.Color).Distinct().ToListAsync();
            var fuels = await _db.Cars.Select(c => c.FuelConsumption).Distinct().ToListAsync();
            var seats = await _db.Cars.Select(c => c.Seat).Distinct().ToListAsync();

            // Đưa danh sách vào ViewBag
            ViewBag.Makes = new SelectList(makes, "MakeID", "MakeName");
            ViewBag.Origins = new SelectList(origins);
            ViewBag.Colors = new SelectList(colors);
            ViewBag.Fuels = new SelectList(fuels);
            ViewBag.Seats = new SelectList(seats);

            return View();
        }

        public ActionResult Cars(string txtSearch = "",string makeName = "", string origin = "", string color = "", string seat = "",int page = 1, int perPage = 9, string sortBy = "")
        {
			var item = _db.Cars
		    .Include(c => c.Make)
		    .Where(c => !c.Flag) // Lọc những xe không bị gắn cờ
		    .ToList();
            if (!string.IsNullOrEmpty(txtSearch))
            {
                item = item.Where(car => car.CarName.ToLower().Contains(txtSearch.ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(makeName))
            {
                item = item.Where(c => c.Make.MakeName == makeName).ToList();
            }
			if (!string.IsNullOrEmpty(origin))
			{
				item = item.Where(c => c.Origin == origin).ToList();
			}
			if (!string.IsNullOrEmpty(color))
			{
				item = item.Where(c => c.Color == color).ToList();
			}
			if (!string.IsNullOrEmpty(seat) && int.TryParse(seat, out int seatNumber))
			{
				item = item.Where(c => c.Seat == seatNumber).ToList();
			}
            switch(sortBy)
            {
                case "Price":
                    item = item.OrderBy(p => p.Price).ToList();
                    break;
                case "bestSelling":
                    var orders = _db.Orders
                        .Include(o => o.DetailOrders)
                        .ThenInclude(deo => deo.Car)
                        .Where(o => o.Status == "Đã thanh toán")
                        .ToList();
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
                    var bestSellingCarIds = sortedDict.Select(x => x.Key);
                    item = item.Where(c => bestSellingCarIds.Contains(c.CarID)).ToList();
                    break;
            }
            int totalCount = item.Count();
            var cars = item.Skip((page-1) * perPage)
                .Take(perPage)
                .Select(i => new List<string>
            {
                i.CarID,
                i.Photo,
                i.Price.ToString(),
                i.CarName
            }) ;
            int totalPages = (int)Math.Ceiling((double)totalCount / perPage);
            return Json(new { Data = cars, TotalPages = totalPages });
		}

		public IActionResult Detail(string id)
        {
			ViewBag.HideHeader = false;
			if (String.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			Car? c = _db.Cars.Find(id);

			if (c == null)
			{
				return NotFound();
			}
			var makeName = _db.Makes.Where(m => m.MakeID == c.MakeID).FirstOrDefault().MakeName;
			CarDto carDtoFromDb = new CarDto
			{
				CarID = c.CarID,
				CarName = c.CarName,
				Photo = c.Photo,

				Capacity = c.Capacity,
				FuelConsumption = c.FuelConsumption,
				Color = c.Color,

				Description = c.Description,
				Dimension = c.Dimension,
				Engine = c.Engine,

				Origin = c.Origin,
				Price = c.Price,
				Quantity = c.Quantity,

				Seat = c.Seat,
				Topspeed = c.Topspeed,
				Year = c.Year,

				MakeID = c.MakeID,
				MakeName = makeName,

			};
            var relatedCars = _db.Cars
                        .Where(car => car.MakeID == c.MakeID && car.CarID != c.CarID && !car.Flag)
                        .ToList();
            ViewBag.RelatedCars = relatedCars;
            return View(carDtoFromDb);
        }

        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> CarListTable(int makeid = 0,string searchtxt = "", int page = 1)
        {

            // Kiểm tra và lấy dữ liệu "makes" nếu chưa có trong TempData
            // TemData lưu trữ dữ liệu trong Session, khi Session hết hạn hoặc bị xóa thì makes bay
            if (!TempData.ContainsKey("makes"))
            {
                List<Make> makes = _db.Makes.ToList();
                TempData["makes"] = JsonConvert.SerializeObject(makes);
                TempData.Keep("makes");
            }
            List<CarDto> cars = await _db.Cars
                .Include(c => c.Make)
                .OrderBy(c => c.CarID)
                .Where(c => c.Flag == false && (makeid == 0||c.MakeID == makeid) && (searchtxt.IsNullOrEmpty() || c.CarName.Contains(searchtxt)))
                .Select(c => new CarDto
            {
                CarID = c.CarID,
                CarName = c.CarName,
                Photo = c.Photo,

                Capacity = c.Capacity,
                FuelConsumption = c.FuelConsumption,
                Color = c.Color,

                Description = c.Description,
                Dimension = c.Dimension,
                Engine = c.Engine,

                Origin = c.Origin,
                Price = c.Price,
                Quantity = c.Quantity,

                Seat = c.Seat,
                Topspeed = c.Topspeed,
                Year = c.Year,

                MakeID = c.MakeID,
                MakeName = c.Make.MakeName,
            }).ToListAsync();
            // tổng số sản phẩm
            int total = cars.Count();
            // tổng số trang
            var totalPage = (total +limits - 1) / limits;
            // sử dụng khi previous là 1
            if (page < 1) page = 1;
            // sử dụng khi next là totalPage 
            if (page > totalPage) page = totalPage;

            ViewBag.totalRecord = total;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;
            ViewBag.makeid = makeid;
            ViewBag.searchtxt = searchtxt;
            cars = cars.Skip((page - 1) * limits).Take(limits).ToList();
             
            return View(cars);
        }
        // [GET]
        [Authorize(Roles = "Admin, Staff")]
        public IActionResult Create()
        {
            return View();
        }
        // [POST]
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Create(CarDto car, IFormFile uploadimage)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {   
                // carid bằng 1 vì trường hợp tập rỗng
                var carid = 1;
                // lấy xe có id lớn nhất
                var lastCar = await _db.Cars.OrderByDescending(c => c.CarID).FirstOrDefaultAsync();
                if (lastCar != null)
                {
                    carid = Convert.ToInt32(lastCar.CarID.Substring(2)) + 1;
                }
                // chuyển về đúng định dạng OT - 6 chữ số
                var caridTxt = "OT" + carid.ToString().PadLeft(6, '0');
                car.CarID = caridTxt;

                

                
                if (uploadimage != null && uploadimage.Length > 0)
                {
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newFileName += Path.GetExtension(uploadimage!.FileName);
                    car.Photo = newFileName;
                    string imageFullPath = Path.Combine(_environment.WebRootPath, "upload\\car", newFileName);
                    using (var fileStream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await uploadimage.CopyToAsync(fileStream);

                    }
                }
                try
                {
                    _db.Cars.Add(new Car
                    {
                        CarID = car.CarID,
                        CarName = car.CarName,
                        Photo = car.Photo,

                        Capacity = car.Capacity,
                        FuelConsumption = car.FuelConsumption,
                        Color = car.Color,

                        Description = car.Description,
                        Dimension = car.Dimension,
                        Engine = car.Engine,

                        Origin = car.Origin,
                        Price = car.Price,
                        Quantity = car.Quantity,

                        Seat = car.Seat,
                        Topspeed = car.Topspeed,
                        Year = car.Year,

                        MakeID = car.MakeID,

                    });

                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // 404
                    return BadRequest("Error add car: " + ex.Message);

                }
                return RedirectToAction("CarListTable");

            }
            return View(car);

        }
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Id is null");
            }
            Car? car = await _db.Cars.FirstOrDefaultAsync(c => c.CarID == id);
            
            if (car == null)
            {
                return NotFound("Car is not found");
            }

            CarDto carDtoFromDb = new CarDto
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Photo = car.Photo,

                Capacity = car.Capacity,
                FuelConsumption = car.FuelConsumption,
                Color = car.Color,

                Description = car.Description,
                Dimension = car.Dimension,
                Engine = car.Engine,

                Origin = car.Origin,
                Price = car.Price,
                Quantity = car.Quantity,

                Seat = car.Seat,
                Topspeed = car.Topspeed,
                Year = car.Year,

                MakeID = car.MakeID,

            };
            return View(carDtoFromDb);
        }
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Edit(CarDto cardto,IFormFile? uploadimage)
        {
            
            if (ModelState.IsValid)
            {   
                Car? car = await _db.Cars.FirstOrDefaultAsync(c => c.CarID == cardto.CarID);
                if (car == null)
                {
                    return NotFound("Car is not found");
                }
                if (uploadimage != null && uploadimage.Length > 0)
                {
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newFileName += Path.GetExtension(uploadimage!.FileName);
                    if (!car.Photo.IsNullOrEmpty())
                    {
                        string oldImageFullPath = Path.Combine(_environment.WebRootPath, "upload\\car", car.Photo);
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }
                    cardto.Photo = newFileName;
                    string imageFullPath = Path.Combine(_environment.WebRootPath, "upload\\car", newFileName);
                    using (var fileStream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await uploadimage.CopyToAsync(fileStream);

                    }
                }
                

                car.CarName = cardto.CarName;
                car.Photo = cardto.Photo;

                car.Capacity = cardto.Capacity;
                car.FuelConsumption = cardto.FuelConsumption;
                car.Color = cardto.Color;

                car.Description = cardto.Description;
                car.Dimension = cardto.Dimension;
                car.Engine = cardto.Engine;

                car.Origin = cardto.Origin;
                car.Price = cardto.Price;
                car.Quantity = cardto.Quantity;

                car.Seat = cardto.Seat;
                car.Topspeed = cardto.Topspeed;
                car.Year = cardto.Year;

                car.MakeID = cardto.MakeID;


                try
                {
                    _db.Cars.Update(car);

                    await _db.SaveChangesAsync();
                        
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // 404
                    return BadRequest("Error edit car: " + ex.Message);

                }
                return RedirectToAction("CarListTable");
            }
            return View(cardto);

        }
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Details(string? id)
        {
            
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Id is null");
            }
            Car? car = await _db.Cars.FirstOrDefaultAsync(c => c.CarID == id);

            if (car == null)
            {
                return NotFound("Car is not found");
            }
            Make? make = await _db.Makes.FirstOrDefaultAsync(m => m.MakeID == car.MakeID);
            
            CarDto carDtoFromDb = new CarDto
            {
                CarID = car.CarID,
                CarName = car.CarName,
                Photo = car.Photo,

                Capacity = car.Capacity,
                FuelConsumption = car.FuelConsumption,
                Color = car.Color,

                Description = car.Description,
                Dimension = car.Dimension,
                Engine = car.Engine,

                Origin = car.Origin,
                Price = car.Price,
                Quantity = car.Quantity,

                Seat = car.Seat,
                Topspeed = car.Topspeed,
                Year = car.Year,

                MakeID = car.MakeID,
                MakeName = make.MakeName,

            };
            return View(carDtoFromDb);
        }
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> Delete(string? id)
        {
            
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Id is null");
            }
            Car? car = await _db.Cars.FirstOrDefaultAsync(c => c.CarID == id);

            if (car == null)
            {
                return NotFound("Car is not found");
            }
           
            car.Flag = true;
            try
            {
                _db.Cars.Update(car);
                await _db.SaveChangesAsync();
                
            }
            catch (DbUpdateConcurrencyException ex)
            {
                
                return BadRequest("Error delete car: " + ex.Message);
            }

            return RedirectToAction("CarListTable");
            
        }
	}
}
