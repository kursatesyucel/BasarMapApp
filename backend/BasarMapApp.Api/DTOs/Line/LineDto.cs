namespace BasarMapApp.Api.DTOs.Line
{
    public class LineDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<List<double>> Coordinates { get; set; } = new List<List<double>>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 