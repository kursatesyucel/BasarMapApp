using BasarMapApp.Api.DTOs.Boundary;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;

namespace BasarMapApp.Api.Services.Implementations
{
    /// <summary>
    /// GIS Boundary Service implementasyonu
    /// Read-only, yüksek performanslı spatial veri servisi
    /// </summary>
    public class BoundaryService : IBoundaryService
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly ISettlementRepository _settlementRepository;
        private readonly GeoJsonWriter _geoJsonWriter;
        private const double SimplificationTolerance = 0.001; // ~100m tolerans

        public BoundaryService(
            IProvinceRepository provinceRepository,
            IDistrictRepository districtRepository,
            ISettlementRepository settlementRepository)
        {
            _provinceRepository = provinceRepository;
            _districtRepository = districtRepository;
            _settlementRepository = settlementRepository;
            _geoJsonWriter = new GeoJsonWriter();
        }

        // ============= İL SINIRLARI (Province) Servisleri =============

        public async Task<ApiResponse<IEnumerable<ProvinceDto>>> GetAllProvincesAsync()
        {
            try
            {
                var provinces = await _provinceRepository.GetAllProvincesAsync();
                
                var provinceDtos = provinces.Select(p => new ProvinceDto
                {
                    Id = p.Id,
                    Geometry = ParseGeoJson(SimplifyGeometry(p.Geom))
                }).ToList();

                return ApiResponse<IEnumerable<ProvinceDto>>.SuccessResult(
                    provinceDtos,
                    $"{provinceDtos.Count} province boundaries retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<ProvinceDto>>.FailureResult(
                    "An error occurred while retrieving province boundaries",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<ProvinceDto>> GetProvinceByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<ProvinceDto>.FailureResult(
                        "Invalid province ID",
                        "Province ID must be greater than 0"
                    );
                }

                var province = await _provinceRepository.GetProvinceByIdAsync(id);
                
                if (province == null)
                {
                    return ApiResponse<ProvinceDto>.FailureResult(
                        "Province boundary not found",
                        $"No province boundary found with ID: {id}"
                    );
                }

                var provinceDto = new ProvinceDto
                {
                    Id = province.Id,
                    Geometry = ParseGeoJson(SimplifyGeometry(province.Geom))
                };

                return ApiResponse<ProvinceDto>.SuccessResult(
                    provinceDto,
                    "Province boundary retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<ProvinceDto>.FailureResult(
                    "An error occurred while retrieving the province boundary",
                    ex.Message
                );
            }
        }

        // ============= İLÇE SINIRLARI (District) Servisleri =============

        public async Task<ApiResponse<IEnumerable<DistrictDto>>> GetAllDistrictsAsync()
        {
            try
            {
                var districts = await _districtRepository.GetAllDistrictsAsync();
                
                var districtDtos = districts.Select(d => new DistrictDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Geometry = ParseGeoJson(SimplifyGeometry(d.Geom))
                }).ToList();

                return ApiResponse<IEnumerable<DistrictDto>>.SuccessResult(
                    districtDtos,
                    $"{districtDtos.Count} district boundaries retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DistrictDto>>.FailureResult(
                    "An error occurred while retrieving district boundaries",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<DistrictDto>> GetDistrictByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<DistrictDto>.FailureResult(
                        "Invalid district ID",
                        "District ID must be greater than 0"
                    );
                }

                var district = await _districtRepository.GetDistrictByIdAsync(id);
                
                if (district == null)
                {
                    return ApiResponse<DistrictDto>.FailureResult(
                        "District boundary not found",
                        $"No district boundary found with ID: {id}"
                    );
                }

                var districtDto = new DistrictDto
                {
                    Id = district.Id,
                    Name = district.Name,
                    Geometry = ParseGeoJson(SimplifyGeometry(district.Geom))
                };

                return ApiResponse<DistrictDto>.SuccessResult(
                    districtDto,
                    "District boundary retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<DistrictDto>.FailureResult(
                    "An error occurred while retrieving the district boundary",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<DistrictDto>>> SearchDistrictsByNameAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ApiResponse<IEnumerable<DistrictDto>>.FailureResult(
                        "Invalid search term",
                        "Search term cannot be empty"
                    );
                }

                var districts = await _districtRepository.SearchDistrictsByNameAsync(searchTerm);
                
                var districtDtos = districts.Select(d => new DistrictDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Geometry = ParseGeoJson(SimplifyGeometry(d.Geom))
                }).ToList();

                return ApiResponse<IEnumerable<DistrictDto>>.SuccessResult(
                    districtDtos,
                    $"{districtDtos.Count} districts found matching '{searchTerm}'"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<DistrictDto>>.FailureResult(
                    "An error occurred while searching districts",
                    ex.Message
                );
            }
        }

        // ============= YERLEŞİM MERKEZLERİ (Settlement) Servisleri =============

        public async Task<ApiResponse<IEnumerable<SettlementDto>>> GetAllSettlementsAsync()
        {
            try
            {
                var settlements = await _settlementRepository.GetAllSettlementsAsync();
                
                var settlementDtos = settlements.Select(s => CreateSettlementDto(s)).ToList();

                return ApiResponse<IEnumerable<SettlementDto>>.SuccessResult(
                    settlementDtos,
                    $"{settlementDtos.Count} settlement centers retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                    "An error occurred while retrieving settlement centers",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<SettlementDto>>> GetSettlementsByCategoryAsync(string category)
        {
            try
            {
                // Kategori validasyonu: BAŞKENT, İL, İLÇE
                var validCategories = new[] { "BAŞKENT", "İL", "İLÇE" };
                if (!validCategories.Contains(category.ToUpper()))
                {
                    return ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                        "Invalid category",
                        "Category must be one of: BAŞKENT, İL, İLÇE"
                    );
                }

                var settlements = await _settlementRepository.GetSettlementsByCategoryAsync(category.ToUpper());
                
                var settlementDtos = settlements.Select(s => CreateSettlementDto(s)).ToList();

                return ApiResponse<IEnumerable<SettlementDto>>.SuccessResult(
                    settlementDtos,
                    $"{settlementDtos.Count} {category} centers retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                    "An error occurred while retrieving settlements by category",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<SettlementDto>>> GetSettlementsInBoundingBoxAsync(
            double minLon, double minLat, double maxLon, double maxLat)
        {
            try
            {
                // Bounding box validasyonu
                if (minLon >= maxLon || minLat >= maxLat)
                {
                    return ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                        "Invalid bounding box",
                        "Min coordinates must be less than max coordinates"
                    );
                }

                // Koordinat aralığı kontrolü (SRID 4326 için)
                if (minLon < -180 || maxLon > 180 || minLat < -90 || maxLat > 90)
                {
                    return ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                        "Invalid coordinates",
                        "Coordinates must be in valid WGS84 range"
                    );
                }

                var settlements = await _settlementRepository.GetSettlementsInBoundingBoxAsync(
                    minLon, minLat, maxLon, maxLat);
                
                var settlementDtos = settlements.Select(s => CreateSettlementDto(s)).ToList();

                return ApiResponse<IEnumerable<SettlementDto>>.SuccessResult(
                    settlementDtos,
                    $"{settlementDtos.Count} settlement centers retrieved in bounding box"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SettlementDto>>.FailureResult(
                    "An error occurred while retrieving settlements",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<SettlementDto>> GetSettlementByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<SettlementDto>.FailureResult(
                        "Invalid settlement ID",
                        "Settlement ID must be greater than 0"
                    );
                }

                var settlement = await _settlementRepository.GetSettlementByIdAsync(id);
                
                if (settlement == null)
                {
                    return ApiResponse<SettlementDto>.FailureResult(
                        "Settlement center not found",
                        $"No settlement center found with ID: {id}"
                    );
                }

                var settlementDto = CreateSettlementDto(settlement);

                return ApiResponse<SettlementDto>.SuccessResult(
                    settlementDto,
                    "Settlement center retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<SettlementDto>.FailureResult(
                    "An error occurred while retrieving the settlement center",
                    ex.Message
                );
            }
        }

        // ============= Yardımcı Metodlar =============

        /// <summary>
        /// Geometriyi basitleştirir (ST_SimplifyPreserveTopology benzeri)
        /// </summary>
        private Geometry SimplifyGeometry(Geometry geometry)
        {
            if (geometry is Point)
                return geometry;

            return NetTopologySuite.Simplify.DouglasPeuckerSimplifier
                .Simplify(geometry, SimplificationTolerance);
        }

        /// <summary>
        /// GeoJSON string'ini parse eder ve object olarak döner
        /// </summary>
        private object ParseGeoJson(Geometry geometry)
        {
            var geoJsonString = _geoJsonWriter.Write(geometry);
            return JsonSerializer.Deserialize<object>(geoJsonString) ?? new { };
        }

        /// <summary>
        /// Settlement modelinden DTO oluşturur
        /// </summary>
        private SettlementDto CreateSettlementDto(BasarMapApp.Api.Models.Settlement settlement)
        {
            return new SettlementDto
            {
                Id = settlement.Id,
                Name = settlement.Name,
                Category = settlement.Category,
                Geometry = ParseGeoJson(settlement.Geom),
                Longitude = settlement.Geom.X,
                Latitude = settlement.Geom.Y
            };
        }
    }
}
