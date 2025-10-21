using System.IdentityModel.Tokens.Jwt;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IJwtService
    {
        JwtSecurityToken getJwtToken(UserEntity user);
    }
}