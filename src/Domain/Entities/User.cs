using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Common;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Entities;
using System.Security.Cryptography; 

public class User : IdentityUser<int>
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public byte[] PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public UserRole Role { get; set; }

    public User()
    {
        
    }

    public User(string newName, string newPassword, UserRole newRole)
    {
        var salt = GenerateSalt();
        
        //hashing algorithm
        HashAlgorithm algorithm = new SHA256Managed();
        
        //hash the password and the salt
        var byteSalt = algorithm.ComputeHash(Encoding.UTF8.GetBytes(salt));
        
        var bytePassword = algorithm.ComputeHash(Encoding.UTF8.GetBytes(newPassword));

        //concatenate the two hashed values
        var hash = ConcatTwoHashes(bytePassword, byteSalt);
        
        
        UserName = newName;
        PasswordSalt = byteSalt;
        //hash again the combination
        PasswordHash = algorithm.ComputeHash(hash);
        Role = newRole;
    }

    public Result<string> Authenticate(string password)
    {
        HashAlgorithm algorithm = new SHA256Managed();
        
        var bytePassword = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
        
        //concatenate the two hashed values
        var hash = ConcatTwoHashes(bytePassword, PasswordSalt);

        if (!PasswordHash.SequenceEqual(algorithm.ComputeHash(hash)))
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
            new Claim(ClaimTypes.Name, user.UserName)
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

    private byte[] ConcatTwoHashes(byte[] hash1, byte[] hash2)
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

        return plainTextWithSaltBytes;
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