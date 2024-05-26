using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using WebPBL3.DTO;
using WebPBL3.Models;

namespace WebPBL3.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ContactController(ApplicationDbContext db)
        {
            _context = db;
        }
        public IActionResult Index()
        {
            ViewBag.HideHeader = false;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(FeedbackDTO feedback)
        {
            if (ModelState.IsValid)
            {
                User? u = _context.Users
                .Include(a => a.Account)
                .FirstOrDefault(u => u.Account.Email == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                if (u != null)
                {
                    var feedback_id = 1;
                    var lastFeedback = _context.Feedbacks.OrderByDescending(f => f.FeedbackID).FirstOrDefault();
                    if (lastFeedback != null)
                    {
                        feedback_id = Convert.ToInt32(lastFeedback.FeedbackID.Substring(2)) + 1;
                    }
                    var feedbackID = "PH" + feedback_id.ToString().PadLeft(6, '0');
                    Feedback fe=new Feedback
                    {
                        FeedbackID = feedbackID,
                        FullName=feedback.FullName,
                        Email=feedback.Email,
                        Title = feedback.Title,
                        Content = feedback.Content,
                        Rating = feedback.Rating,
                        Status="Chưa xem",
                        CreateAt = DateTime.Now,
                        UserID = u.UserID,
                    };
                    _context.Feedbacks.Add(fe);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
                
            }
            else
            {
                return View();
            }  
        }
        public IActionResult ListFeedback(DateTime? date=null,String title="")
        {
            List<Feedback> feedbacks =
                _context.Feedbacks
                .Where(f => (date==null||f.CreateAt.Date==date)
                            &&(title.IsNullOrEmpty()||f.Title.Contains(title)))
                .ToList();
            ViewBag.DateMessage = date?.ToString("yyyy-MM-dd"); ;
            ViewBag.TitleMessage = title;
            return View(feedbacks);
        }
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            Feedback o = _context.Feedbacks.FirstOrDefault(f => f.FeedbackID == id);
            if (o != null)
            {
                _context.Feedbacks.Remove(o);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("ListFeedback");
        }

        public async Task<JsonResult> getFeedbackById(string id)
        {
            var feedback= _context.Feedbacks.FirstOrDefault(u=>u.FeedbackID== id);
            if(feedback != null)
            {
                feedback.Status = "Đã xem";
                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();
            }
            return new JsonResult(feedback);
        }
    }
}
