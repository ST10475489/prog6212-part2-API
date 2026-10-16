namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; }
    }
}
