using Ecommerce.IdentityJWT.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    //B1: Tao Fields
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;//read file appsettings.json

    //B2: Tao Contructor (Ctrl . )
    public AuthenticateController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }
    
    //B3: Controller

    /*Register user*/
    [HttpPost]
    [Route("register-user")]
    public async Task<IActionResult> RegisterUser ([FromBody] UserRegisterModel model)
    {
        //1. Check exits user
        var userExits = await this._userManager.FindByNameAsync(model.UserName);
        if(userExits != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,new ResponseHeader 
            { Status="Error",Message="User already exists..."});
        }
        //2. Create new IdentityUser
        IdentityUser user = new() {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.UserName
        };
        //3. Apply into DB via UserManager
        var result = await this._userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader
            { Status = "Error", Message = "User created fail, please check...." });
        }
        return Ok(new ResponseHeader { Status = "Success", Message = "User created ok..." });
    }

    /*Register user has roles (admin)*/
    [HttpPost]
    [Route("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] UserRegisterModel model)
    {
        //1. Check exits user
        var userExits = await this._userManager.FindByNameAsync(model.UserName);
        if (userExits != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader
            { Status = "Error", Message = "User already exists..." });
        }
        //2. Create new IdentityUser
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.UserName
        };
        //3. aplly into DB via UserManager
        var result = await this._userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseHeader
            { Status = "Error", Message = "User created fail, please check...." });
        }
        //4. If register ok then add roles (admin, user and client)
        if(!await this._roleManager.RoleExistsAsync(UserRoles.AdminRole))
        {
            await this._roleManager.CreateAsync(new IdentityRole(UserRoles.AdminRole));
        }
        if(!await this._roleManager.RoleExistsAsync(UserRoles.UserRole))
        {
            await this._roleManager.CreateAsync(new IdentityRole(UserRoles.UserRole));
        }
        if(!await _roleManager.RoleExistsAsync(UserRoles.ClientRole))
        {
            await this._roleManager.CreateAsync(new IdentityRole(UserRoles.ClientRole));
        }
        //5. add user to each roles
        if (await this._roleManager.RoleExistsAsync(UserRoles.AdminRole))
        {
            await this._userManager.AddToRoleAsync(user, UserRoles.AdminRole);
        }
        if (await this._roleManager.RoleExistsAsync(UserRoles.UserRole))
        {
            await this._userManager.AddToRoleAsync(user, UserRoles.UserRole);
        }
        if (await this._roleManager.RoleExistsAsync(UserRoles.ClientRole))
        {
            await this._userManager.AddToRoleAsync(user, UserRoles.ClientRole);
        }

        return Ok(new ResponseHeader
        {
            Status="Success",
            Message="User created successfully..."
        });
    }

    /*Login & generated token value */
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> UserLogin([FromBody] UserLoginModel model)
    {
        //1. Check exist user 
        var user = await this._userManager.FindByNameAsync(model.UserName);
        if(user != null && await this._userManager.CheckPasswordAsync(user, model.Password))
        {
            var userRoles = await this._userManager.GetRolesAsync(user);//Get all roles of the user
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //2. if user has roles (optional) then add to claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role,userRole));
            }

            //3. render token 
            var tokenValue = GeneratedToken(authClaims);

            //4. return ok to browser
            return Ok(
                new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(tokenValue),
                    expiration = tokenValue.ValidTo
                });

        }
        return Unauthorized();
    }

    //GeneratedToken Method
    private JwtSecurityToken GeneratedToken(List<Claim> authClaims)
    {
        //1. Convert private key to by array
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration["JWT:Secret"]));
        //2. render token value
        var tokenValue = new JwtSecurityToken(
            issuer: this._configuration["JWT:ValidIssuer"],
            audience: this._configuration["JWT:ValidAudience"],
            expires : DateTime.Now.AddHours(2),//TG het han
            claims : authClaims,//information sign
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
        return tokenValue;
    }
}
