using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.Db.Models;
using OnlineShopWebApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace OnlineShopWebApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration, IUserService userService, SignInManager<User> signInManager, UserManager<User> userManager, ILogger<AccountController> logger)
        {
            _configuration = configuration;
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("Auth")]
        public async Task<ActionResult> Auth([FromBody] AuthUser user)
        {
            bool isValid = await _userService.IsValidUserForAuth(user, _signInManager);
            if (isValid)
            {
                var tokenString = GenerateJwtToken(user.UserName);
                _logger.LogInformation($"Выполнен вход пользователя {user.UserName}");
                return Ok(new { Token = tokenString, Message = "Вход выполнен" });
            }
            _logger.LogError($"Ошибка авторизации пользователя {user.UserName}");
            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("Registration")]
        public async Task<ActionResult> Registration([FromBody] RegUser user)
        {
            bool isValid = await _userService.IsValidUserForRegister(user, _userManager);
            if (isValid)
            {
                var tokenString = GenerateJwtToken(user.UserName);
                _logger.LogInformation($"Регистрация пользователя {user.UserName} успешно выполнена");
                return Ok(new { Token = tokenString, Message = "Регистрация успешна!" });
            }

            _logger.LogError($"Ошибка регистрации пользователя {user.UserName}");
            return BadRequest();
        }

        private string GenerateJwtToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:key"]);
            var tokenDesriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("UserName", userName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDesriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
