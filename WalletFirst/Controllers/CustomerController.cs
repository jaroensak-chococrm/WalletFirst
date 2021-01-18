using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WalletFirst.Models;

namespace WalletFirst.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly WalletDBContext _context;
        private readonly JWTSettings _jwtsettings;


        public CustomerController (WalletDBContext context/*, IOptions<JWTSettings> jwtsettings*/)
        {
            _context = context;
           // _jwtsettings = jwtsettings.Value;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers ()
        {
            //  return await _context.TodoItems.ToListAsync();
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customer/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer (int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // GET: api/Customer/
        [HttpGet("GetCustomer")]
        public async Task<ActionResult<Customer>> GetCustomer ()
        {
            string email = HttpContext.User.Identity.Name;
            var customer = await _context.Customers
                .Where(user => user.Email == email)
                .FirstOrDefaultAsync();

            customer.Password = null;

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        //// GET: api/Customer/Login
        //[HttpGet("Login")]
        //public async Task<ActionResult<UserWithToken>> Login ([FromBody] Customer customer)
        //{
        //    customer = await _context.Customers
        //        .Where(user => user.Email == customer.Email && user.Password == customer.Password)
        //        .FirstOrDefaultAsync();

        //    UserWithToken userWithToken = new UserWithToken(customer);

        //    if (userWithToken == null)
        //    {
        //        return NotFound();
        //    }

        //    //sign your token here
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_jwtsettings.SecretKey);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new Claim[]
        //        {
        //            new Claim(ClaimTypes.Name, customer.Email)
        //        }),
        //        Expires = DateTime.UtcNow.AddMonths(6),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
        //        SecurityAlgorithms.HmacSha256Signature)
        //    };
        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    userWithToken.AccessToken = tokenHandler.WriteToken(token);
           
        //    return userWithToken;
        //}
       

// PUT: api/Customer/5
// To protect from overposting attacks, enable the specific properties you want to bind to, for
// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer (int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var cus = await _context.Customers.FindAsync(id);
            if (cus == null)
            {
                return NotFound();
            }

            cus.Name = customer.Name;
            cus.Lastname = customer.Lastname;
            cus.Phone = customer.Phone; ;
            cus.Address = customer.Address;
            cus.Status = customer.Status;
            cus.TimeUpdate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!CustomerExists(id))
            {
                return NotFound();
            }
            return NoContent();
        }

        // POST: api/Customer
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer (Customer customer)
        {

            var cus = new Customer
            {
                Name = customer.Name,
                Lastname = customer.Lastname,
                Phone = customer.Phone,
                Address = customer.Address,
                CustomerRefNo = customer.CustomerRefNo,
                Status = customer.Status,
                Email = customer.Email,
                TimeCreate = DateTime.Now,
                TimeUpdate = DateTime.Now
            };

            _context.Customers.Add(cus);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetCustomer", new { id = cus.Id }, cus);
        }

        // DELETE: api/Customer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer (int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // GET: api/GetCustomerDetails/5
        [HttpGet("CustomerDetails/{id}")]
        public ActionResult<Customer> GetCustomerDetails (int id)
        {
            var customer = _context.Customers
              .Include(wal => wal.Wallets)
              .Where(cus => cus.Id == id)
              .FirstOrDefault();

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // Post: api/customer/CreateCustomer
        [HttpPost("CreateCustomer")]
        public async Task<ActionResult<Customer>> CreateCustomer (Customer customer)
        {
            var cus = new Customer
            {
                Name = customer.Name,
                Lastname = customer.Lastname,
                Phone = customer.Phone,
                CustomerRefNo = customer.CustomerRefNo,
                Address = customer.Address,
                Status = customer.Status,
                TimeCreate = DateTime.Now,
                TimeUpdate = DateTime.Now,
                Email = customer.Email,
                Password = customer.Password,
                RoleId = customer.RoleId

            };

            Wallet wallet = new Wallet();
            wallet.CustomerId = cus.Id;
            wallet.CustomerRefNo = cus.CustomerRefNo;
            wallet.TimeCreate = DateTime.Now;
            wallet.TimeUpdate = DateTime.Now;
            wallet.WalletStatus = 1;
            wallet.WalletNo = cus.Phone + "01";
            wallet.Balance = 0;

            cus.Wallets.Add(wallet);

            _context.Customers.Add(cus);
            await _context.SaveChangesAsync();

            var customerDetail = _context.Customers
              .Include(wal => wal.Wallets)
              .Where(customer => customer.CustomerRefNo == cus.CustomerRefNo)
              .FirstOrDefault();

            if (cus == null)
            {
                return NotFound();
            }

            return customerDetail;
        }

        // GET: api/Customer/CustomerStatus
        [HttpGet("CustomerStatus")]
        public async Task<ActionResult<IEnumerable<CustomerStatu>>> GetCustomerStatus ()
        {
            //  return await _context.TodoItems.ToListAsync();
            return await _context.CustomerStatus.ToListAsync();
        }

        // GET: api/Customer/GetCustomerStatus/5
        [HttpGet("CustomerStatus/{id}")]
        public async Task<ActionResult<CustomerStatu>> GetCustomerStatu (int id)
        {
            var customer = await _context.CustomerStatus.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }


        // POST: api/Customer/AddStatus
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("CustomerStatus")]
        public async Task<ActionResult<CustomerStatu>> AddCustomerStatus (CustomerStatu customer)
        {

            var cus = new CustomerStatu
            {
                StatusName = customer.StatusName,
                Description = customer.Description,
                TimeCreate = DateTime.Now
            };

            _context.CustomerStatus.Add(cus);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetCustomer", new { id = cus.StatusId }, cus);
        }

        // PUT: api/Customer/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("CustomerStatus/{id}")]
        public async Task<IActionResult> PutCustomerStatus (int id, CustomerStatu customerStatu)
        {
            if (id != customerStatu.StatusId)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var cus = await _context.CustomerStatus.FindAsync(id);
            if (cus == null)
            {
                return NotFound();
            }

            cus.Description = customerStatu.Description;
            cus.StatusName = customerStatu.StatusName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!CustomerStatusExists(id))
            {
                return NotFound();
            }
            return NoContent();
        }



        // DELETE: api/Customer/5
        [HttpDelete("CustomerStatus/{id}")]
        public async Task<IActionResult> DeleteCustomerStatus (int id)
        {
            var customer = await _context.CustomerStatus.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.CustomerStatus.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool CustomerExists (int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }

        private bool CustomerStatusExists (int id)
        {
            return _context.CustomerStatus.Any(e => e.StatusId == id);
        }

        //private static Transaction Transaction (Transaction transaction) => new Transaction
        //{
        //    TransId = transaction.TransId,
        //    CustomerId = transaction.CustomerId,
        //    WalletId = transaction.WalletId,
        //    Type = transaction.Type,
        //    Destination = transaction.Destination,
        //    Amount = transaction.Amount,
        //    TimeCreate = transaction.TimeCreate
        //};
    }
}
