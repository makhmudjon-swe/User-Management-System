using Domain.Entities;

namespace Application.Services
{
    public interface IJwtTokenService
    {
        string CreateToken(User user);
    }
}
