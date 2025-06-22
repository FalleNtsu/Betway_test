namespace OT.Assessment.App.Models.Requests
{
    public class AuthRequest
    {
        public Guid APIKey { get; set; } = Guid.NewGuid();
        public string Password { get; set; } = string.Empty;
    }
}
