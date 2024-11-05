using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Auth
{
    public static class AuthorizationSetup
    {
        /// <summary>
        /// 前端客户jwt初始化设置
        /// </summary>
        /// <param name="services"></param>
        public static void AddAuthorizationSetupForClient(this IServiceCollection services,IConfiguration configuration)
        {
            services.Configure<JwtTokenOptions>(configuration.GetSection("JwtTokenOptions"));
            JwtTokenOptions jwtTokenOptions = new JwtTokenOptions();
            configuration.Bind("JwtTokenOptions", jwtTokenOptions);
            //启动Jwt身份验证
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {

                    ValidateIssuer = false,//验证令牌的发行者是否有效
                    ValidateAudience = false,//验证令牌的受众是否有效
                    ValidateLifetime = true,//验证令牌的有效期
                    ValidateIssuerSigningKey = true,//验证令牌的签名密钥是否有效

                    //ValidAudience = jwtTokenOptions.Audience,
                    //ValidIssuer = jwtTokenOptions.Issure,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenOptions.SecurityKey)) //密钥
                };
            });
            //启动鉴权
            services.AddAuthorization(options =>
            {
                //定义一个名为 "Admin" 的授权策略。
                //该策略要求 JWT 令牌中必须包含一个 "userRole" 声明，其值为 "admin"。
                options.AddPolicy("Admin", policy => policy.RequireClaim("userRole", "admin"));
                //定义一个名为 "User" 的授权策略。
                //该策略要求 JWT 令牌中必须包含一个 "userRole" 声明，其值为 "user"。
                options.AddPolicy("User", policy => policy.RequireClaim("userRole", "user"));
            });
        }
    }
}
