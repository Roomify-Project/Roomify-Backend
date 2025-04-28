public class LoginResponseDto
{
    public string Token { get; set; }
    public string UserName { get; set; }
    public string Roles { get; set; }
    public string Email { get; set; }

    public Guid UserId { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
    public string Message { get; set; } // كمان لو عايز تبعتله رسالة حلوة للواجهة

}
    