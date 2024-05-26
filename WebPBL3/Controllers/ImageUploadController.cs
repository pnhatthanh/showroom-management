using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
namespace WebPBL3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ImageUploadController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage()
        {
            try
            {
                var file = Request.Form.Files[0]; // Lấy hình ảnh từ yêu cầu
                var folderName = "images"; // Tên thư mục lưu trữ hình ảnh
                var webRootPath = _hostingEnvironment.WebRootPath; // Đường dẫn gốc của thư mục wwwroot
                var pathToSave = Path.Combine(webRootPath, folderName); // Đường dẫn đầy đủ tới thư mục lưu trữ hình ảnh

                if (file.Length > 0)
                {
                    var fileName =  Path.GetFileName(file.FileName); 
                    var fullPath = Path.Combine(pathToSave, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    Console.WriteLine(fullPath);
                    var imageUrl = fullPath.Replace("\\", "/"); // URL của hình ảnh
                    return Ok(new { imageUrl }); // Trả về URL của hình ảnh
                }
                else
                {
                    return BadRequest("File is empty.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
