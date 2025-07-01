using AutoMapper;
using BasarMapApp.Api.DTOs.Point;
using BasarMapApp.Api.DTOs.Line;
using BasarMapApp.Api.DTOs.Polygon;
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

            // MapLine mappings
            CreateMap<MapLine, LineDto>()
                .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => 
                    src.Geometry.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList()));

            CreateMap<CreateLineDto, MapLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => 
                    geometryFactory.CreateLineString(src.Coordinates.Select(c => new Coordinate(c[0], c[1])).ToArray())));

            CreateMap<UpdateLineDto, MapLine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => 
                    geometryFactory.CreateLineString(src.Coordinates.Select(c => new Coordinate(c[0], c[1])).ToArray())));

            // MapPolygon mappings
            CreateMap<MapPolygon, PolygonDto>()
                .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => 
                    new List<List<List<double>>> {
                        src.Geometry.ExteriorRing.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList()
                    }.Concat(src.Geometry.InteriorRings.Select(ring => 
                        ring.Coordinates.Select(c => new List<double> { c.X, c.Y }).ToList())).ToList()));

            CreateMap<CreatePolygonDto, MapPolygon>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => 
                    CreatePolygonFromCoordinates(geometryFactory, src.Coordinates)));

            CreateMap<UpdatePolygonDto, MapPolygon>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => 
                    CreatePolygonFromCoordinates(geometryFactory, src.Coordinates)));
        }

        private static NetTopologySuite.Geometries.Polygon CreatePolygonFromCoordinates(GeometryFactory factory, List<List<List<double>>> coordinates)
        {
            if (coordinates == null || coordinates.Count == 0)
                throw new ArgumentException("Polygon must have at least one ring");

            var shell = factory.CreateLinearRing(coordinates[0].Select(c => new Coordinate(c[0], c[1])).ToArray());
            
            var holes = coordinates.Skip(1).Select(ring => 
                factory.CreateLinearRing(ring.Select(c => new Coordinate(c[0], c[1])).ToArray())).ToArray();

            return factory.CreatePolygon(shell, holes);
        }
    }
}
