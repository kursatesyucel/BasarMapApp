using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Implementations
{
    public class CameraRepository : ICameraRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public CameraRepository(ApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<IEnumerable<Camera>> GetAllAsync()
        {
            return await _context.Cameras
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Camera?> GetByIdAsync(int id)
        {
            return await _context.Cameras
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Camera> CreateAsync(Camera camera)
        {
            camera.CreatedAt = DateTime.UtcNow;
            camera.UpdatedAt = null;
            
            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();
            
            return camera;
        }

        public async Task<Camera?> UpdateAsync(int id, Camera camera)
        {
            var existingCamera = await GetByIdAsync(id);
            if (existingCamera == null)
                return null;

            existingCamera.Name = camera.Name;
            existingCamera.Description = camera.Description;
            existingCamera.Geometry = camera.Geometry;
            existingCamera.VideoFileName = camera.VideoFileName;
            existingCamera.IsActive = camera.IsActive;
            existingCamera.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingCamera;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var camera = await GetByIdAsync(id);
            if (camera == null)
                return false;

            _context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cameras.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Camera>> GetActiveCamerasAsync()
        {
            return await _context.Cameras
                .Where(c => c.IsActive)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Camera?> GetByVideoFileNameAsync(string videoFileName)
        {
            return await _context.Cameras
                .FirstOrDefaultAsync(c => c.VideoFileName == videoFileName);
        }

        public async Task<IEnumerable<Camera>> GetCamerasWithinPolygonAsync(Polygon polygon)
        {
            return await _context.Cameras
                .Where(c => polygon.Contains(c.Geometry))
                .ToListAsync();
        }
    }
} 