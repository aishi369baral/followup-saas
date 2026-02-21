using System.IdentityModel.Tokens.Jwt; // provides classes to create and write JWT tokens.
using System.Security.Claims; //provides Claim and ClaimTypes clases
using System.Text;  // needed for Encoding.UTF8.GetBytes()
using FollowUp.Api.Models; //lets this service use User model as it needs user data to create the token
using Microsoft.IdentityModel.Tokens; //provides SymmetricSecurityKey , SigningCredentials, SecurityAlorithms which are used to cryptographically sign the token so that it cant be tampered with.

namespace FollowUp.Api.Auth;

public class JwtService // service class responsible only for JWT token creation
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }


    // Token Generation method.
    // Called after successful login / register
    // Takes User object and returns Json Web Token(JWT) string.
    // This token is stored in frontend and sends in Authorization: Bearer <token>
    public string GenerateToken(User user)
    {

        //Claim creation // creating payload for JWT 
        // Claim / JWT payload (in this case : user.id and user.email) become available to Authorization endpoints
       
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };


        // secret key creation. This key is used to sign the token hence prevent hacker tampering of the tokens / generating fake tokens
        
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );


        // signing credentials.
        // Tells us which key and hashing algorithm to use
        //Ensures the token is trusted by your API.

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        // Actual token creation
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],      // you created the token. your API verifies it later
            audience: _config["Jwt:Audience"],   // Who it was created for. Prevent token reuse in another system 
            claims: claims,                        // Claims: user data inside the token
            expires: DateTime.UtcNow.AddDays(7),    // Token Lifetime (7 days)
            signingCredentials: creds                // Attaches the digital signature
        );

        return new JwtSecurityTokenHandler().WriteToken(token);     // Convert token object --> String  (Serializes the token into the standard format which is then sent to the client after login/register) 

    }
}

//Claims are data stored inside the JWT token (userid, email, roles, etc)
// Encoding.UTF8.GetBytes() converts secret key string -> byte[] for crytographic signing