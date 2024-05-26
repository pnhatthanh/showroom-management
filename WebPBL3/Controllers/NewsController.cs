using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using WebPBL3.DTO;
using WebPBL3.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebPBL3.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext _db;
        private IWebHostEnvironment _environment;
        private int limits = 10;
        public NewsController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }
        public IActionResult Index()
        {
            List<News> list = _db.NewS.ToList();
            ViewBag.HideHeader = false;
            return View(list);
        }
        public IActionResult ListNews(int newid = 0, string searchtxt = "", int page = 1)
        {
            List<NewsDto> news = _db.NewS.Include(s => s.Staff).ThenInclude(u => u.User).Select(n => new NewsDto
            {
                NewsID = n.NewsID,
                Title = n.Title,
                Content = n.Content,
                Photo = n.Photo,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                UpdateBy = n.UpdateBy,
                StaffID = n.StaffID,
                FullName = n.Staff.User.FullName,

            }).ToList();
            var total = news.Count;
            var totalPage = (total + limits - 1) / limits;
            if (page < 1) page = 1;
            if (page > totalPage) page = totalPage;
            ViewBag.totalRecord = total;
            ViewBag.totalPage = totalPage;
            ViewBag.currentPage = page;
            ViewBag.searchtxt = searchtxt;
            ViewBag.newid = newid;
            news = news.Skip((page - 1) * limits).Take(limits).ToList();
            int cnt = 1;
            foreach (var n in news)
            {
                n.STT = cnt++;
            }
            return View(news);
        }

        //Get
        public IActionResult Create() 
        { 
            return View();
        }
        //Post
        [HttpPost]
        
        public async Task<IActionResult> Create(NewsDto news, IFormFile uploadimage)
        {
            if(!ModelState.IsValid)
            {
                var newsid = 1;
                var lastNews = _db.NewS.OrderByDescending(n => n.NewsID).FirstOrDefault();
                if (lastNews != null)
                {
                    newsid = Convert.ToInt32(lastNews.NewsID.Substring(2)) + 1;
                }
                var newsidTxt = newsid.ToString().PadLeft(6, '0');
                news.NewsID = newsidTxt;
                string FILENAME = "";
                if (TempData["UploadedFileName"] != null)
                {
                    FILENAME = TempData["UploadedFileName"].ToString();
                }
                news.Photo = FILENAME;
                _db.NewS.Add(new News
                {
                    NewsID = news.NewsID,
                    Title = news.Title,
                    Content = news.Content,
                    Photo = news.Photo,
                    CreateAt = DateTime.Now,
                    UpdateAt = null,
                    StaffID = "NV000001",
                }) ;
                await _db.SaveChangesAsync();
                return RedirectToAction("ListNews");
            }
            return View(news);
        }

        public IActionResult Edit(string? id)
        {


            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            News? news = _db.NewS.Find(id);

            if (news == null)
            {
                return NotFound();
            }

            NewsDto newsDtoFromDb = new NewsDto
            {

                NewsID = news.NewsID,
                Title = news.Title,
                Content = news.Content,
                Photo = news.Photo,
                CreateAt = news.CreateAt,
                UpdateAt = DateTime.Now,
                StaffID = "1",

            };
            return View(newsDtoFromDb);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NewsDto n, IFormFile? uploadimage, string? id)
        {

            if (!ModelState.IsValid)
            {
                News? news = _db.NewS.Find(id);
                if (uploadimage != null && uploadimage.Length > 0)
                {
                    int index = uploadimage.FileName.IndexOf('.');

                    string _FileName = "news" + n.NewsID + "." + uploadimage.FileName.Substring(index + 1);
                    n.Photo = _FileName;
                    string _path = Path.Combine(_environment.WebRootPath, "images", _FileName);
                    Console.WriteLine(_path);
                    using (var fileStream = new FileStream(_path, FileMode.Create))
                    {
                        await uploadimage.CopyToAsync(fileStream);

                    }
                }
                news.Title = n.Title;
                news.Content = n.Content;
                news.Photo = n.Photo;
                news.CreateAt = n.CreateAt;
                news.UpdateAt = DateTime.Now;
                news.UpdateBy = 1.ToString();
                news.StaffID = 1.ToString();

                try
                {
                    _db.NewS.Update(news);

                    await _db.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    NotFound();

                }


                return RedirectToAction("ListNews");
            }
            return View(n);
        }

        public IActionResult DetailUser(string? id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            News? news = _db.NewS.Find(id);

            if (news == null)
            {
                return NotFound();
            }

            NewsDto newsDtoFromDb = new NewsDto
            {

                NewsID = news.NewsID,
                Title = news.Title,
                Content = news.Content,
                Photo = news.Photo,
                CreateAt = news.CreateAt,
                UpdateAt = DateTime.Now,
                StaffID = "1",

            };
            List<News> _news=_db.NewS.ToList();
            ViewBag._news = _news;
            ViewBag.HideHeader = true;
            return View(newsDtoFromDb);
        }
        public async Task<IActionResult> Delete(string? id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            News? newsToDelete = _db.NewS.FirstOrDefault(n => n.NewsID == id);

            if (newsToDelete == null)
            {
                return NotFound();
            }

            try
            {
                _db.NewS.Remove(newsToDelete); 
                await _db.SaveChangesAsync(); 
               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }
            return RedirectToAction("ListNews");
        }
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile upload)
        {
            if (upload == null || upload.Length == 0)
            {
                return BadRequest("File is empty."); 
            }

            var fileName = Path.GetFileName(upload.FileName);
            var filePath = Path.Combine(_environment.WebRootPath, "images", fileName);
            TempData["UploadedFileName"] = fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await upload.CopyToAsync(stream);
            }
            var url = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            return Json(new { uploaded = true, url });
        }
    }
}
       




 
