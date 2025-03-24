using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;

public class DatabaseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConnectionFactory _connectionFactory;

    public DatabaseMiddleware(RequestDelegate next, ConnectionFactory connectionFactory)
    {
        _next = next;
        _connectionFactory = connectionFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var dbName = jwt.Claims.FirstOrDefault(c => c.Type == "Database")?.Value;

            if (!string.IsNullOrEmpty(dbName))
            {
                context.Items["UserDatabase"] = _connectionFactory.GetConnectionForUser(dbName);
            }
        }

        await _next(context);
    }
}
