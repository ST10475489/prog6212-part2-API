namespace RaceDay.Api.Models
{
    public class EventRoute
    {
        public int RouteId { get; set; }
        public int EventId { get; set; }
        public string RouteName { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public string MapUrl { get; set; }
    }
}
