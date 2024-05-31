using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPBL3.DTO.Statistic;
using WebPBL3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static NuGet.Packaging.PackagingConstants;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace WebPBL3.Controllers
{
    [Authorize(Policy = "Admin,Staff")]
    public class StatisticController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StatisticController(ApplicationDbContext db)
        {
            _db = db;
        }

        
        public async Task<IActionResult> Index(DateOnly? _startTime = null, DateOnly? _endTime = null, string? maNV = null, string? maXe = null, string? hangXe = null)
        {
            
            ViewBag._startTime = _startTime;
            ViewBag._endTime = _endTime;
            ViewBag.maNV = maNV;
            ViewBag.maXe = maXe;
            ViewBag.hangXe = hangXe;


            DateTime endTime = DateTime.Now;
            DateTime startTime = endTime.AddMonths(-11);

            ViewBag.userTotal = await _db.Users.CountAsync();
            ViewBag.carTotal = await _db.Cars.CountAsync();
            ViewBag.staffTotal = await _db.Staffs.CountAsync();
            ViewBag.feedbackTotal = await _db.Feedbacks.CountAsync();

           List<Order> orders = await _db.Orders
                .Include(o => o.DetailOrders)
                .ThenInclude(deo => deo.Car)
                .ThenInclude(c => c.Make)
                .Where(o => o.Status == "Đã thanh toán" && (o.Date.Year > startTime.Year || (o.Date.Year == startTime.Year && o.Date.Month >= startTime.Month))
                    && (o.Date.Year < endTime.Year || (o.Date.Year == endTime.Year && o.Date.Month <= endTime.Month)))
                .ToListAsync();

            Dictionary<int, int> quantityMake = new Dictionary<int, int>();
            Dictionary<int, double> revenueMake = new Dictionary<int, double>();
            Dictionary<string, int> quantityMonth = new Dictionary<string, int>();
            Dictionary<string,double> revenueMonth = new Dictionary<string, double>();

            foreach (var order in orders)
            {
                foreach (var detailOrder in order.DetailOrders)
                {
                    if (!quantityMake.ContainsKey(detailOrder.Car.MakeID))
                    {
                        quantityMake[detailOrder.Car.MakeID] = 0;
                    }
                    quantityMake[detailOrder.Car.MakeID] += detailOrder.Quantity;

                    if (!revenueMake.ContainsKey(detailOrder.Car.MakeID))
                    {
                        revenueMake[detailOrder.Car.MakeID] = 0;
                    }
                    revenueMake[detailOrder.Car.MakeID] += detailOrder.Quantity * detailOrder.Price;

                    string timeKey = $"{order.Date.Month}/{order.Date.Year}";
                    if (!quantityMonth.ContainsKey(timeKey))
                    {
                        quantityMonth[timeKey] = 0;
                    }
                    quantityMonth[timeKey] += detailOrder.Quantity;

                    if (!revenueMonth.ContainsKey(timeKey))
                    {
                        revenueMonth[timeKey] = 0;
                    }
                    revenueMonth[timeKey] += detailOrder.Quantity * detailOrder.Price; 
                }
            }

            List<Make> makes = await _db.Makes.ToListAsync();
            List<StatisticMake> statisticMakes = new List<StatisticMake>();
            foreach (var make in makes)
            {
                statisticMakes.Add(new StatisticMake
                {
                    MakeName = make.MakeName,
                    Quantity = quantityMake.ContainsKey(make.MakeID) ? quantityMake[make.MakeID] : 0,
                    Revenue = revenueMake.ContainsKey(make.MakeID) ? revenueMake[make.MakeID] / 1000000000 : 0
                });
            }

            ViewBag.statisticMakes = statisticMakes;

            List<StatisticRevenue> statisticRevenues = new List<StatisticRevenue>();
            for (int i = 0; i < 12; i++)
            {
                string timeKey = $"{startTime.Month}/{startTime.Year}";
                statisticRevenues.Add(new StatisticRevenue
                {
                    Month = timeKey,
                    Quantity = quantityMonth.ContainsKey(timeKey) ? quantityMonth[timeKey] : 0,
                    Revenue = revenueMonth.ContainsKey(timeKey) ? revenueMonth[timeKey]/1000000000 : 0
                });
                startTime = startTime.AddMonths(1);
            }

            ViewBag.statisticRevenues = statisticRevenues;

            orders = await _db.Orders
                .Include(o => o.DetailOrders)
                .ThenInclude(deo => deo.Car)
                .ThenInclude(c => c.Make)
                .Where(o => o.Status == "Đã thanh toán"
                && ((_startTime == null && _endTime == null)
                || (_startTime == null && _endTime != null && DateOnly.FromDateTime(o.Date) <= _endTime)
                || (_startTime != null && _endTime == null && _startTime <= DateOnly.FromDateTime(o.Date))
                || (_startTime != null && _endTime != null && _startTime <= DateOnly.FromDateTime(o.Date) && DateOnly.FromDateTime(o.Date) <= _endTime)))
                .ToListAsync();
            int cnt = 0;
            List<StatisticTable> statisticTables = new List<StatisticTable>();
            foreach (var order in orders)
            {
                foreach (var detailOrder in order.DetailOrders)
                {
                    if (maNV == null || (order.StaffID.ToLower().Contains(maNV.ToLower())))
                    {
                        if (hangXe == null || (detailOrder.Car.Make.MakeName.ToLower().Contains(hangXe.ToLower())))
                        {
                            if (maXe == null || (detailOrder.CarID.ToLower().Contains(maXe.ToLower())))
                            {
                                statisticTables.Add(new StatisticTable
                                {
                                    STT = ++cnt,
                                    CarID = detailOrder.CarID,
                                    MakeName = detailOrder.Car.Make.MakeName,
                                    StaffID = order.StaffID,
                                    Date = order.Date,
                                    Quantity = detailOrder.Quantity,
                                    Price = detailOrder.Price,
                                    Total = detailOrder.Quantity * detailOrder.Price

                                });
                            }
                        }
                    }

                }
            }
            ViewBag.statisticTables = statisticTables;

            return View();
        }


        [HttpPost]
        public IActionResult SaveExcel()
        {
            // Deserialize dữ liệu từ JSON sang list StatisticTable
            //Console.WriteLine(Request.Form["data"].ToString());
            var requestData = Request.Form["data"];
            if (string.IsNullOrEmpty(requestData))
            {
                return BadRequest("Data is null or empty");
            }
            List<StatisticTable> statisticTables;
            try
            {
                statisticTables = JsonConvert.DeserializeObject<List<StatisticTable>>(requestData, new JsonSerializerSettings { MaxDepth = 10 });
            }
            catch (JsonException ex)
            {
                
                return BadRequest("Failed to deserialize data: " +  ex.Message);
            }




            // Tạo một file Excel mới
            using (var package = new ExcelPackage())
                {
                    // Tạo một sheet mới trong file Excel
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                    // Viết tiêu đề cho sheet
                    worksheet.Cells[1, 1].Value = "STT";
                    worksheet.Cells[1, 2].Value = "CarID";
                    worksheet.Cells[1, 3].Value = "MakeName";
                    worksheet.Cells[1, 4].Value = "StaffID";
                    worksheet.Cells[1, 5].Value = "Date";
                    worksheet.Cells[1, 6].Value = "Quantity";
                    worksheet.Cells[1, 7].Value = "Price";
                    worksheet.Cells[1, 8].Value = "Total";

                    // Viết dữ liệu vào sheet
                    for (int i = 0; i < statisticTables.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = statisticTables[i].STT;
                        worksheet.Cells[i + 2, 2].Value = statisticTables[i].CarID;
                        worksheet.Cells[i + 2, 3].Value = statisticTables[i].MakeName;
                        worksheet.Cells[i + 2, 4].Value = statisticTables[i].StaffID;
                        worksheet.Cells[i + 2, 5].Value = statisticTables[i].Date.ToString("yyyy-MM-dd");
                        worksheet.Cells[i + 2, 6].Value = statisticTables[i].Quantity;
                        worksheet.Cells[i + 2, 7].Value = statisticTables[i].Price;
                        worksheet.Cells[i + 2, 8].Value = statisticTables[i].Total;
                    }

                    // Lưu file Excel
                    var memoryStream = new MemoryStream();
                    package.SaveAs(memoryStream);
                    memoryStream.Position = 0;

                    // Trả về file Excel để người dùng chọn đường dẫn lưu file
                    return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "file.xlsx");
                }
            
        }
    }
}