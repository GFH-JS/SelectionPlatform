using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Auth
{
    public class JwtServices:IJwtServices
    {
        JwtTokenOptions _jwtTokenOptions;
        public JwtServices(IOptionsMonitor<JwtTokenOptions> jwtTokenOptions)
        {
            _jwtTokenOptions = jwtTokenOptions.CurrentValue;
        }
        public string GenerateToken(string userId, string userRole)
        {
            var claims = new[]
            {
             //JwtRegisteredClaimNames.Sub:
             // 这是一个预定义的 JWT 声明类型，表示 "subject"（主体）。
             // 在此上下文中，它通常用于存储用户的唯一标识符（如用户名或用户ID）。
             new Claim("userId", userId),
             new Claim("userRole", userRole)
         };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenOptions.SecurityKey));//""内填自己的密钥，要足够长。
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),//过期时间
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
