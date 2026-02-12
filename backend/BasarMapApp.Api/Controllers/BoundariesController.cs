using BasarMapApp.Api.DTOs.Boundary;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    /// <summary>
    /// GIS Referans Verileri API Controller
    /// Yüksek performanslı read-only boundary verisi sağlar
    /// Şema: ogc_fid, detay_adı, adı, kategori
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Referans veriler public erişime açık
    public class BoundariesController : ControllerBase
    {
        private readonly IBoundaryService _boundaryService;
        private readonly ILogger<BoundariesController> _logger;

        public BoundariesController(
            IBoundaryService boundaryService,
            ILogger<BoundariesController> logger)
        {
            _boundaryService = boundaryService;
            _logger = logger;
        }

        // ============= İL SINIRLARI (Province) Endpoints =============

        /// <summary>
        /// Tüm il sınırlarını getirir (simplified geometry ile)
        /// </summary>
        /// <remarks>
        /// Sadece görsel sınır katmanı - detay_adı her zaman "İL_SINIRI"
        /// GIST indeks sayesinde yüksek performans
        /// </remarks>
        [HttpGet("provinces")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProvinceDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProvinceDto>>>> GetAllProvinces()
        {
            _logger.LogInformation("Getting all province boundaries");
            
            var result = await _boundaryService.GetAllProvincesAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli bir il sınırını ID'ye göre getirir
        /// </summary>
        [HttpGet("provinces/{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProvinceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProvinceDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProvinceDto>>> GetProvinceById(int id)
        {
            _logger.LogInformation("Getting province boundary with ID: {ProvinceId}", id);
            
            var result = await _boundaryService.GetProvinceByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        // ============= İLÇE SINIRLARI (District) Endpoints =============

        /// <summary>
        /// Tüm ilçe sınırlarını getirir
        /// </summary>
        [HttpGet("districts")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DistrictDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DistrictDto>>>> GetAllDistricts()
        {
            _logger.LogInformation("Getting all district boundaries");
            
            var result = await _boundaryService.GetAllDistrictsAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Belirli bir ilçe sınırını ID'ye göre getirir
        /// </summary>
        [HttpGet("districts/{id}")]
        [ProducesResponseType(typeof(ApiResponse<DistrictDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DistrictDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DistrictDto>>> GetDistrictById(int id)
        {
            _logger.LogInformation("Getting district boundary with ID: {DistrictId}", id);
            
            var result = await _boundaryService.GetDistrictByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// İsme göre ilçe arar
        /// </summary>
        [HttpGet("districts/search")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DistrictDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DistrictDto>>>> SearchDistricts(
            [FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var error = ApiResponse<IEnumerable<DistrictDto>>.FailureResult(
                    "Invalid search query",
                    "Query parameter cannot be empty"
                );
                return BadRequest(error);
            }

            _logger.LogInformation("Searching districts with query: {Query}", query);
            
            var result = await _boundaryService.SearchDistrictsByNameAsync(query);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        // ============= YERLEŞİM MERKEZLERİ (Settlement) Endpoints =============

        /// <summary>
        /// Yerleşim merkezlerini getirir
        /// </summary>
        /// <remarks>
        /// Filtreleme seçenekleri:
        /// - category: BAŞKENT, İL, İLÇE
        /// - bbox: Bounding Box (minLon,minLat,maxLon,maxLat)
        /// </remarks>
        [HttpGet("settlements")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SettlementDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<SettlementDto>>>> GetSettlements(
            [FromQuery] string? category = null,
            [FromQuery] string? bbox = null)
        {
            // Kategori filtresi (öncelikli)
            if (!string.IsNullOrWhiteSpace(category))
            {
                _logger.LogInformation("Getting settlements by category: {Category}", category);
                
                var result = await _boundaryService.GetSettlementsByCategoryAsync(category);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            
            // Bounding Box filtresi
            if (!string.IsNullOrWhiteSpace(bbox))
            {
                var bboxParts = bbox.Split(',');
                
                if (bboxParts.Length != 4)
                {
                    var error = ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                        "Invalid bbox format",
                        "bbox must be in format: minLon,minLat,maxLon,maxLat"
                    );
                    return BadRequest(error);
                }

                if (!double.TryParse(bboxParts[0], out double minLon) ||
                    !double.TryParse(bboxParts[1], out double minLat) ||
                    !double.TryParse(bboxParts[2], out double maxLon) ||
                    !double.TryParse(bboxParts[3], out double maxLat))
                {
                    var error = ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                        "Invalid bbox coordinates",
                        "All bbox values must be valid numbers"
                    );
                    return BadRequest(error);
                }

                _logger.LogInformation(
                    "Getting settlements in bounding box: [{MinLon},{MinLat},{MaxLon},{MaxLat}]",
                    minLon, minLat, maxLon, maxLat);
                
                var result = await _boundaryService.GetSettlementsInBoundingBoxAsync(
                    minLon, minLat, maxLon, maxLat);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }

            // Filtre yoksa tümünü getir
            _logger.LogInformation("Getting all settlement centers");
            
            var allResult = await _boundaryService.GetAllSettlementsAsync();
            
            if (allResult.Success)
                return Ok(allResult);
            
            return BadRequest(allResult);
        }

        /// <summary>
        /// Belirli bir yerleşim merkezini ID'ye göre getirir
        /// </summary>
        [HttpGet("settlements/{id}")]
        [ProducesResponseType(typeof(ApiResponse<SettlementDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<SettlementDto>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SettlementDto>>> GetSettlementById(int id)
        {
            _logger.LogInformation("Getting settlement center with ID: {SettlementId}", id);
            
            var result = await _boundaryService.GetSettlementByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }
    }
}
