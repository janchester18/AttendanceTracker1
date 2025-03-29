using AttendanceTracker1.Services.FileService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("image/{fileName}")]
        [Authorize] // Or remove if you don't require auth for file access.
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var fileBytes = await _fileService.GetFileAsync(fileName);
                var contentType = _fileService.GetContentType(fileName);
                return File(fileBytes, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found.");
            }
            catch (Exception ex)
            {
                // Log exception if needed.
                return StatusCode(500, ex.Message);
            }
        }
    }
}
