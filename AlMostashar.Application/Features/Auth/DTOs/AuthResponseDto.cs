namespace AlMostashar.Application.Features.Auth.DTOs
{
    public class AuthResponseDto
    {
        public AuthUserDto User { get; set; } = null!;
        public AuthTokensDto? Tokens { get; set; }
    }
}

