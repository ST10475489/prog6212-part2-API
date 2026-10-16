namespace RaceDay.Api.Dtos
{
//for registration
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
    }

    public class ResultDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; }
        public string EventName { get; set; }
        public string CategoryName { get; set; }
        public string FinishTime { get; set; }
        public int Position { get; set; }
    }

    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; }
    }
}
