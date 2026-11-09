using Microsoft.AspNetCore.Mvc;
using MyApiApp.DTOs;
using MyApiApp.Services;
using MyApiApp.Validation;

namespace MyApiApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(FileUploadDTO.MaxFileSizeBytes)]
        public async Task<ActionResult<FileResponseDTO>> Upload([FromForm] FileUploadDTO request)
        {
            var errors = FileValidator.Validate(request.File);
            if (errors.Count > 0)
            {
                return BadRequest(new { message = string.Join(" ", errors), errors });
            }

            try
            {
                var result = await _fileService.UploadAsync(request);
                return CreatedAtAction(nameof(GetAll), null, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileResponseDTO>>> GetAll()
        {
            return Ok(await _fileService.GetAllAsync());
        }

        [HttpGet("download/{storedName}")]
        public async Task<IActionResult> Download(string storedName)
        {
            var entity = await _fileService.GetByStoredNameAsync(storedName);
            if (entity == null)
            {
                return NotFound(new { message = $"Không tìm thấy file '{storedName}'" });
            }

            var path = _fileService.GetPhysicalPath(storedName);
            if (path == null)
            {
                return NotFound(new { message = $"File '{entity.OriginalName}' không còn tồn tại trên server" });
            }

            return PhysicalFile(
                path,
                string.IsNullOrWhiteSpace(entity.ContentType) ? "application/octet-stream" : entity.ContentType,
                entity.OriginalName,
                enableRangeProcessing: true);
        }

        [HttpDelete("{storedName}")]
        public async Task<IActionResult> Delete(string storedName)
        {
            var deleted = await _fileService.DeleteAsync(storedName);
            if (!deleted)
            {
                return NotFound(new { message = $"Không tìm thấy file '{storedName}'" });
            }

            return NoContent();
        }
    }
}
