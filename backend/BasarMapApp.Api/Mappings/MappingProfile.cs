using AutoMapper;
using BasarMapApp.Api.DTOs.Point;
using BasarMapApp.Api.Models;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

            // MapPoint -> PointDto
            CreateMap<MapPoint, PointDto>()
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Geometry.Y))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Geometry.X));

            // CreatePointDto -> MapPoint
            CreateMap<CreatePointDto, MapPoint>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => 
                    geometryFactory.CreatePoint(new Coordinate(src.Longitude, src.Latitude))));

            // UpdatePointDto -> MapPoint
            CreateMap<UpdatePointDto, MapPoint>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => 
                    geometryFactory.CreatePoint(new Coordinate(src.Longitude, src.Latitude))));
        }
    }
}
