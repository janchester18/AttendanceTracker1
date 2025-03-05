using Newtonsoft.Json;

namespace AttendanceTracker1.DTO
{
    public class EmailRequestDto
    {
        public string? Email { get; set; } = "janchester.peren.sitesphil.ojt@gmail.com";
        public string Name { get; set; }
        [JsonProperty("sender_email")]
        public string SenderEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
