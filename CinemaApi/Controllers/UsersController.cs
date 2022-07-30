using AuthenticationPlugin;
using CinemaApi.Data;
using CinemaApi.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace CinemaApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private CinemaDbContext _dbContext;
        private IConfiguration _configuration;
        private readonly AuthService _auth;

        public UsersController(CinemaDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _auth = new AuthService(_configuration);
        }
        //GET:api/<MoviesController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_dbContext.Users);

        }


        //GET:api/<MoviesController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var user= _dbContext.Movies.Find(id);
            if (user== null)
            { return NotFound("No record found against this Id"); }
            return Ok(user);
        }
        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            var userWithSameEmail = _dbContext.Users.Where(u => u.Email == user.Email).SingleOrDefault();
            if (userWithSameEmail!= null)
            {
                return BadRequest("User with same email already exist");
                
            }
            var userObj = new User
            {
                Name = user.Name,
                Email = user.Email,
                //Password = user.Password,
                Password = SecurePasswordHasherHelper.Hash(user.Password),
                Role = "Users"
            };
            _dbContext.Users.Add(userObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
        [HttpPost]
        public IActionResult Login([FromBody] User user)
        {
            var userEmail = _dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
            if (userEmail==null)
            {
                return NotFound();
            }
            if (!SecurePasswordHasherHelper.Verify(user.Password,userEmail.Password))
            {
                return Unauthorized();
            }
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email,user.Email ),
                new Claim(ClaimTypes.Email,user.Email ),
            };
            var token = _auth.GenerateAccessToken(claims);
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                user_id=userEmail.Id,
            });



        }






    }
}
