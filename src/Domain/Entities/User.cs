using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Common;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Entities;
using System.Security.Cryptography; 

public sealed class User : IdentityUser<int>
{
    public byte[] PasswordSalt { get; set; } = default!;
    public UserRole Role { get; set; }

    public User()
    {
        
    }

    public User(string newName, string newPassword, UserRole newRole)
    {
        var salt = GenerateSalt();
        
        //hashing algorithm
        HashAlgorithm algorithm = SHA256.Create();
        
        //hash the password and the salt
        var byteSalt = algorithm.ComputeHash(Encoding.UTF8.GetBytes(salt));
        
        var bytePassword = algorithm.ComputeHash(Encoding.UTF8.GetBytes(newPassword));

        //concatenate the two hashed values
        var hash = ConcatTwoHashes(bytePassword, byteSalt);
        
        
        UserName = newName;
        PasswordSalt = byteSalt;
        //hash again the combination
        PasswordHash = hash;
        Role = newRole;
    }

    public Result<string> Authenticate(string password)
    {
        HashAlgorithm algorithm = SHA256.Create();
        
        var bytePassword = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        //concatenate the two hashed values
        var hash = ConcatTwoHashes(bytePassword, PasswordSalt);

        if (!PasswordHash.Equals(hash))
        {
            return Result.Fail("invalid password");
        }
        
        var token = generateJwtToken(this);
        return Result.Ok(token);
    }

    private string generateJwtToken(User user)
    {
        List<Claim> claims = new List<Claim>()
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("StrONGKAutHENTICATIONKEy"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: "https://localhost:5001",
            audience:"http://localhost:3000",
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    private string ConcatTwoHashes(byte[] hash1, byte[] hash2)
    {
        byte[] plainTextWithSaltBytes = 
            new byte[hash1.Length + hash2.Length];

        for (int i = 0; i < hash1.Length; i++)
        {
            plainTextWithSaltBytes[i] = hash1[i];
        }
        for (int i = 0; i < hash2.Length; i++)
        {
            plainTextWithSaltBytes[hash1.Length + i] = hash2[i];
        }

        return Encoding.UTF8.GetString( plainTextWithSaltBytes);
    }

    private string GenerateSalt()
    {
        //generate a random string for using as a salt
        Random random = new Random();
        
        const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
