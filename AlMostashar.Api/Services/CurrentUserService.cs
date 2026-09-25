using AlMostashar.Application.Common.Interfaces;
using System.Security.Claims;

namespace AlMostashar.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                // Get the user from the current HTTP request
                var user = _httpContextAccessor?.HttpContext?.User;

                if (user != null)
                {
                    // Look for the NameIdentifier claim first
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

                    // If not found, look for the "sub" claim
                    if (userIdClaim == null)
                    {
                        userIdClaim = user.FindFirst("sub");
                    }
                    // Try to convert the string value to an integer safely
                    // It returns true if successful, and sets the 'id' variable
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
                    {
                        return id;
                    }
                }

                throw new UnauthorizedAccessException("User identity could not be determined from the token.");
            }
        }
    }
}