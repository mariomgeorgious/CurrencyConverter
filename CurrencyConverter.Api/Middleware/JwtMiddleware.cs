using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IdentityModel.Tokens.Jwt;

namespace CurrencyConverter.Api.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenValidationParameters _tokenValidationParams;

        public JwtMiddleware(RequestDelegate next, TokenValidationParameters tokenValidationParams)
        {
            _next = next;
            _tokenValidationParams = tokenValidationParams;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var jwtTokenHandler = new JwtSecurityTokenHandler();
                // Validation 1 - Validation JWT token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(token, _tokenValidationParams, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        context.Items["Error"] = "Token is Invalid";
                    }
                }
            }
            catch (Exception ex)
            {
                context.Items["Error"] = "Token does not match or may expired."; // userService.GetById(userId);
            }
            await _next(context);
        }
    }
}
