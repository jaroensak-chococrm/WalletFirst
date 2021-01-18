using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletFirst.Models;

namespace WalletFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly WalletDBContext _context;

        public WalletController (WalletDBContext context)
        {
            _context = context;
        }

        // GET: api/Wallet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wallet>>> GetWallets ()
        {
            //  return await _context.TodoItems.ToListAsync();
            return await _context.Wallets.ToListAsync();
        }

        // GET: api/Wallet/5
        [HttpGet("{walletNo}")]
        public async Task<ActionResult<Wallet>> GetWallet (string walletNo)
        {
            var wallet = await _context.Wallets.Where(x=> x.WalletNo == walletNo).FirstOrDefaultAsync();

            if (wallet == null)
            {
                return NotFound();
            }

            return wallet;
        }


        // GET: api/Wallet/getwalletdetails/5
        [HttpGet("GetWalletDetails/{walletNo}")]
        public async Task<ActionResult<Wallet>> GetWalletDetails (string walletNo)
        {
            var wallet = _context.Wallets
                .Include(wal => wal.Customer)
                .Where(wal => wal.WalletNo == walletNo)
                .FirstOrDefault();

            if (wallet == null)
            {
                return NotFound();
            }

            return wallet;
        }

// PUT: api/Wallet/5
// To protect from overposting attacks, enable the specific properties you want to bind to, for
// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
[HttpPut("{walletNo}")]
        public async Task<IActionResult> PutWallet (string walletNo, Wallet wallet)
        {
            if (walletNo != wallet.WalletNo)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var wal = await _context.Wallets.Where(x => x.WalletNo == walletNo).FirstOrDefaultAsync();
            if (wal == null)
            {
                return NotFound();
            }
            
            wal.Balance = wallet.Balance;
            wal.WalletStatus = wallet.WalletStatus;
            wal.TimeUpdate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!WalletExists(wal.WalletId))
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Wallet
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Wallet>> PostWallet (Wallet wallet)
        {
            var customer = _context.Customers
                .Include(cus => cus.Wallets)
                .Where(cus => cus.CustomerRefNo == wallet.CustomerRefNo)
                .FirstOrDefault();

            string walletCountFmt = (customer.Wallets.Count()+1).ToString("00");

            var wal = new Wallet
            {
                CustomerId = wallet.CustomerId,
                CustomerRefNo = wallet.CustomerRefNo,
                Balance = wallet.Balance,
                WalletStatus = wallet.WalletStatus,
                WalletNo = customer.Phone + walletCountFmt,
                TimeCreate = DateTime.Now,
                TimeUpdate = DateTime.Now
            };

            _context.Wallets.Add(wal);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetWallet", new { walletNo = wal.WalletNo }, wal);
        }

        // DELETE: api/wallet/5
        [HttpDelete("{walletNo}")]
        public async Task<IActionResult> DeleteWallet (string walletNo)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.WalletNo == walletNo);
            if (wallet == null)
            {
                return NotFound();
            }

            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Wallet
        [HttpGet("GetWalletsStatus")]
        public async Task<ActionResult<IEnumerable<WalletStatu>>> GetWalletsStatus ()
        {
            return await _context.WalletStatus.ToListAsync();
        }

        // GET: api/Wallet/GetWalletStatus/5
        [HttpGet("GetWalletStatus/{id}")]
        public async Task<ActionResult<WalletStatu>> GetWalletStatus (int id)
        {
            var walletStatu = await _context.WalletStatus.FindAsync(id);

            if (walletStatu == null)
            {
                return NotFound();
            }

            return walletStatu;
        }

        // POST: api/Wallet/AddWalletStatus
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("AddWalletStatus")]
        public async Task<ActionResult<WalletStatu>> AddWalletStatus (WalletStatu wallet)
        {

            var wal = new WalletStatu
            {
                WalletStatusName = wallet.WalletStatusName,
                Description = wallet.Description,
                TimeCreate = DateTime.Now
            };

            _context.WalletStatus.Add(wal); ;
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetWalletStatus", new { id = wal.WalletStatusId }, wal);
        }


        // PUT: api/Wallet/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("PutWalletStatus/{id}")]
        public async Task<IActionResult> PutWalletStatus (int id, WalletStatu walletStatu)
        {
            if (id != walletStatu.WalletStatusId)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var wal = await _context.WalletStatus.FindAsync(id);
            if (wal == null)
            {
                return NotFound();
            }

            wal.Description = walletStatu.Description;
            wal.WalletStatusName = walletStatu.WalletStatusName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!WalletExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/wallet/DelWalletStatus/5
        [HttpDelete("DelWalletStatus/{id}")]
        public async Task<IActionResult> DelWalletStatus (int id)
        {
            var wallet = await _context.WalletStatus.FindAsync(id);
            if (wallet == null)
            {
                return NotFound();
            }

            _context.WalletStatus.Remove(wallet);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WalletExists (int id)
        {
            return _context.Wallets.Any(e => e.WalletId == id);
        }
    }
}
