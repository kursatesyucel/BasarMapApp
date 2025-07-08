using BasarMapApp.Api.DTOs.Camera;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CamerasController : ControllerBase
    {
        private readonly ICameraService _cameraService;
        private readonly IWebHostEnvironment _environment;

        public CamerasController(ICameraService cameraService, IWebHostEnvironment environment)
        {
            _cameraService = cameraService;
            _environment = environment;
        }

        /// <summary>
        /// Get all cameras
        /// </summary>
        /// <returns>List of all cameras</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CameraDto>>>> GetAllCameras()
        {
            var result = await _cameraService.GetAllAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get only active cameras
        /// </summary>
        /// <returns>List of active cameras</returns>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CameraDto>>>> GetActiveCameras()
        {
            var result = await _cameraService.GetActiveCamerasAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get a camera by ID
        /// </summary>
        /// <param name="id">Camera ID</param>
        /// <returns>Camera with the specified ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CameraDto>>> GetCamera(int id)
        {
            var result = await _cameraService.GetByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get a camera by video file name
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <returns>Camera with the specified video file</returns>
        [HttpGet("by-video/{videoFileName}")]
        public async Task<ActionResult<ApiResponse<CameraDto>>> GetCameraByVideoFileName(string videoFileName)
        {
            var result = await _cameraService.GetByVideoFileNameAsync(videoFileName);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Create a new camera
        /// </summary>
        /// <param name="createCameraDto">Camera creation data</param>
        /// <returns>Created camera</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CameraDto>>> CreateCamera([FromBody] CreateCameraDto createCameraDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<CameraDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _cameraService.CreateAsync(createCameraDto);
            
            if (result.Success)
                return CreatedAtAction(
                    nameof(GetCamera), 
                    new { id = result.Data!.Id }, 
                    result
                );
            
            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing camera
        /// </summary>
        /// <param name="id">Camera ID</param>
        /// <param name="updateCameraDto">Camera update data</param>
        /// <returns>Updated camera</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CameraDto>>> UpdateCamera(int id, [FromBody] UpdateCameraDto updateCameraDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<CameraDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _cameraService.UpdateAsync(id, updateCameraDto);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Delete a camera
        /// </summary>
        /// <param name="id">Camera ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCamera(int id)
        {
            var result = await _cameraService.DeleteAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Stream video file with range request support for chunk-based loading
        /// </summary>
        /// <param name="videoFileName">Video file name</param>
        /// <returns>Video stream with range support</returns>
        [HttpGet("stream/{videoFileName}")]
        public async Task<IActionResult> StreamVideo(string videoFileName)
        {
            try
            {
                // Validate video file name
                if (string.IsNullOrWhiteSpace(videoFileName))
                {
                    return BadRequest("Video file name is required");
                }

                // Security: Only allow specific video files and prevent path traversal
                var allowedExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
                var extension = Path.GetExtension(videoFileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Unsupported video format");
                }

                // Construct file path
                var videosPath = Path.Combine(_environment.WebRootPath, "videos", "cameras");
                var filePath = Path.Combine(videosPath, videoFileName);

                // Security: Ensure the resolved path is within the videos directory
                var fullVideosPath = Path.GetFullPath(videosPath);
                var fullFilePath = Path.GetFullPath(filePath);
                
                if (!fullFilePath.StartsWith(fullVideosPath))
                {
                    return BadRequest("Invalid file path");
                }

                // Check if file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"Video file '{videoFileName}' not found");
                }

                var fileInfo = new FileInfo(filePath);
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                
                // Get content type
                var contentType = GetContentType(extension);
                
                // Handle range requests for chunk-based streaming
                var rangeHeader = Request.Headers["Range"].ToString();
                
                if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes="))
                {
                    return await HandleRangeRequest(fileStream, fileInfo.Length, rangeHeader, contentType, videoFileName);
                }

                // Return full file if no range request
                return File(fileStream, contentType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error streaming video: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle HTTP Range requests for chunk-based video streaming
        /// </summary>
        private async Task<IActionResult> HandleRangeRequest(FileStream fileStream, long fileLength, string rangeHeader, string contentType, string fileName)
        {
            try
            {
                var ranges = rangeHeader.Substring(6).Split(',');
                var range = ranges[0];
                var rangeParts = range.Split('-');
                
                long start = 0;
                long end = fileLength - 1;
                
                if (!string.IsNullOrEmpty(rangeParts[0]))
                {
                    start = long.Parse(rangeParts[0]);
                }
                
                if (rangeParts.Length > 1 && !string.IsNullOrEmpty(rangeParts[1]))
                {
                    end = long.Parse(rangeParts[1]);
                }
                
                // Ensure valid range
                if (start >= fileLength || end >= fileLength || start > end)
                {
                    Response.Headers.Add("Content-Range", $"bytes */{fileLength}");
                    return StatusCode(416); // Range Not Satisfiable
                }
                
                var contentLength = end - start + 1;
                
                // Set response headers for partial content
                Response.StatusCode = 206; // Partial Content
                Response.Headers.Add("Accept-Ranges", "bytes");
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileLength}");
                Response.Headers.Add("Content-Length", contentLength.ToString());
                Response.ContentType = contentType;
                
                // Seek to start position and read the requested range
                fileStream.Seek(start, SeekOrigin.Begin);
                var buffer = new byte[16384]; // 16KB buffer - Daha büyük chunk'lar için
                // Diğer seçenekler:
                // 4096  = 4KB  (Daha küçük chunk'lar)
                // 8192  = 8KB  (Varsayılan)
                // 16384 = 16KB (Daha büyük chunk'lar)
                // 32768 = 32KB (En büyük chunk'lar)
                var totalBytesToRead = contentLength;
                var totalBytesRead = 0L;
                
                while (totalBytesRead < totalBytesToRead)
                {
                    var bytesToRead = (int)Math.Min(buffer.Length, totalBytesToRead - totalBytesRead);
                    var bytesRead = await fileStream.ReadAsync(buffer, 0, bytesToRead);
                    
                    if (bytesRead == 0)
                        break;
                    
                    await Response.Body.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                }
                
                return new EmptyResult();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error processing range request");
            }
            finally
            {
                fileStream?.Dispose();
            }
        }

        /// <summary>
        /// Get cameras within a polygon area
        /// </summary>
        /// <param name="polygonCoordinates">Polygon coordinates in GeoJSON format</param>
        /// <returns>List of cameras within the polygon</returns>
        [HttpPost("within-polygon")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CameraDto>>>> GetCamerasWithinPolygon([FromBody] List<List<List<double>>> polygonCoordinates)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<IEnumerable<CameraDto>>.FailureResult(
                    "Invalid model state",
                    "Please check your request data"
                ));
            }

            var result = await _cameraService.GetCamerasWithinPolygonAsync(polygonCoordinates);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get content type based on file extension
        /// </summary>
        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",
                _ => "application/octet-stream"
            };
        }
    }
} 