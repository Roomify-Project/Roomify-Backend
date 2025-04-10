public class LoginResponseDto
{
    public string Token { get; set; }
    public string UserName { get; set; }
    public string Roles { get; set; }
    public string Email { get; set; }

    public Guid UserId { get; set; } 
}
