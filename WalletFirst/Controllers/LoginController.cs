using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WalletFirst.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WalletFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly WalletDBContext _context;
        private readonly JWTSettings _jwtsettings;


        public LoginController (WalletDBContext context, IOptions<JWTSettings> jwtsettings)
        {
            _context = context;
            _jwtsettings = jwtsettings.Value;
        }


        // GET: api/Login/
        [HttpGet]
        public async Task<ActionResult<UserWithToken>> Login ([FromBody] Customer customer)
        {
            //    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            //    password: customer.Password,
            //    salt: salt,
            //    prf: KeyDerivationPrf.HMACSHA1,
            //    iterationCount: 10000,
            //    numBytesRequested: 256 / 8));
            // Console.WriteLine($"Hashed: {hashed}");

            customer = await _context.Customers
                .Where(user => user.Email == customer.Email && user.Password == customer.Password)
                .FirstOrDefaultAsync();

            UserWithToken userWithToken = null;

            if (customer != null)
            {
                RefreshToken refreshToken = GenerateRefreshToken();
                customer.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                userWithToken = new UserWithToken(customer);
                userWithToken.RefreshToken = refreshToken.Token;
            }    
            if (userWithToken == null)
            {
                return NotFound();
            }

            //sign your token here
            userWithToken.AccessToken = GenerateAccessToken(customer.Id);

            return userWithToken;
        }

        // POST: api/login/RegisterUser
        [HttpPost("RegisterUser")]
        public async Task<ActionResult<UserWithToken>> RegisterUser ([FromBody] Customer user)
        {
            var salt = GenerateSalt();
            var pass = HashingPassword(user.Password, salt);
            var cus = new Customer
            {
                Name = user.Name,
                Lastname = user.Lastname,
                Phone = user.Phone,
                CustomerRefNo = user.CustomerRefNo,
                Address = user.Address,
                Status = user.Status,
                TimeCreate = DateTime.Now,
                TimeUpdate = DateTime.Now,
                Email = user.Email,
                Password = pass,
                RoleId = user.RoleId,
                Salt =   Convert.ToBase64String(salt)
            };

            Wallet wallet = new Wallet();
            wallet.CustomerId = cus.Id;
            wallet.CustomerRefNo = cus.CustomerRefNo;
            wallet.TimeCreate = DateTime.Now;
            wallet.TimeUpdate = DateTime.Now;
            wallet.WalletStatus = 1;
            wallet.WalletNo = cus.Phone +"01";
            wallet.Balance = 0;

            cus.Wallets.Add(wallet);

            _context.Customers.Add(cus);
            await _context.SaveChangesAsync();

            //load role for registered user
            user = await _context.Customers.Include(u => u.Wallets)
                                        .Where(u => u.Id == cus.Id)
                                        .FirstOrDefaultAsync();

            UserWithToken userWithToken = null;

            if (user != null)
            {
                RefreshToken refreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                userWithToken = new UserWithToken(user);
                userWithToken.RefreshToken = refreshToken.Token;
            }

            if (userWithToken == null)
            {
                return NotFound();
            }

            //sign your token here here..
            userWithToken.AccessToken = GenerateAccessToken(user.Id);
            return userWithToken;
        }

        // GET: api/Login/
        [HttpGet("RefreshToken")]
        public async Task<ActionResult<UserWithToken>> RefreshToken ([FromBody] RefreshRequest refreshRequest)
        {
            Customer user = GetCustomerFromAccessToken(refreshRequest.AccessToken);
            if(user != null && ValidateRefreshToken(user,refreshRequest.RefreshToken))
            {
                UserWithToken userWithToken = new UserWithToken(user);
                userWithToken.AccessToken = GenerateAccessToken(user.Id);
               

                 return userWithToken;

            }

            return null;
        }

        private bool ValidateRefreshToken (Customer user, string refreshToken)
        {
            RefreshToken refreshTokenUser =  _context.RefreshTokens.Where(rt => rt.Token == refreshToken)
                                                .OrderByDescending(rt => rt.ExpiryDate)
                                                .FirstOrDefault();

            if(refreshTokenUser != null && refreshTokenUser.UserId == user.Id
                && refreshTokenUser.ExpiryDate > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }

        private Customer GetCustomerFromAccessToken (string accessToken)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            };

            SecurityToken securityToken;

            var principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters,out securityToken);

            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;
            
            if(jwtSecurityToken != null && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var id = principle.FindFirst(ClaimTypes.Name)?.Value;
                return _context.Customers.Where(user => user.Id == Convert.ToInt32(id)).FirstOrDefault();
            }

            return null;
        }

        private RefreshToken GenerateRefreshToken ()
        {
            RefreshToken refreshToken = new RefreshToken();

            var randomNumber = new byte[32];
            using ( var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                refreshToken.Token = Convert.ToBase64String(randomNumber);
            }
            refreshToken.ExpiryDate = DateTime.UtcNow.AddMonths(3);

            return refreshToken;
        }

        private string GenerateAccessToken (int id)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, Convert.ToString(id))
                }),
                Expires = DateTime.UtcNow.AddMinutes(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashingPassword (string password, byte[] salt)
        {

            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            Console.WriteLine($"Hashed: {hashed}");

            return hashed;
        }
        private byte[] GenerateSalt ()
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

            return salt;
        }


        // DATE TIME TUTORIALs
        public string dateTimess ()
        {
            var date = DateTime.Now;
            var date2 = DateTime.UtcNow;
            Console.WriteLine(date);
            Console.WriteLine(date2);
            Int32 unixTimestamp = (Int32) ( DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)) ).TotalSeconds;
            Console.WriteLine(unixTimestamp);
            Int32 unixTimestamp2 = (Int32) ( DateTime.UtcNow.Subtract(DateTime.Now) ).TotalSeconds;
            Console.WriteLine(unixTimestamp2);
            Int32 unixTimestamp3 = (Int32) ( DateTime.UtcNow.Subtract(new DateTime(1997, 7, 5)) ).TotalSeconds;
            Console.WriteLine(unixTimestamp3);
            DateTime foo = DateTime.Now;
            long unixTime = ( (DateTimeOffset) foo ).ToUnixTimeSeconds();
            Console.WriteLine(unixTime);
            DateTime boo = foo.ToUniversalTime();
            Console.WriteLine(boo);

            return null;

        }
    }
}
