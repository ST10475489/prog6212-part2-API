namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; }
        public decimal Distance { get; set; }
        public int Participants { get; set; }   // maximum number of participants
    }
}
