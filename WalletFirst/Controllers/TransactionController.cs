using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WalletFirst.Models;
using System.Transactions;
using Transaction = WalletFirst.Models.Transaction;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Medallion.Threading.SqlServer;
using System.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace WalletFirst.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly WalletDBContext _context;

        private readonly object balanceLock = new object();

        public  int i;

        private readonly ILogger<TransactionController> logger;
        private Timer timer;
        private int number;
        Queue queuetest = new Queue();

        public TransactionController (WalletDBContext context)
        {
            _context = context;
        }

        // GET: api/Transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions ()
        {
            //  return await _context.TodoItems.ToListAsync();
            return await _context.Transactions.ToListAsync();
        }

        // GET: api/Transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction (int id)
        {
            var trans = await _context.Transactions.FindAsync(id);

            if (trans == null)
            {
                return NotFound();
            }

            return trans;
        }

        // GET: api/Transaction/GetTransactionDetails/5
        [HttpGet("GetTransactionDetails/{id}")]
        public ActionResult<Transaction> GetTransactionDetails (int id)
        {
            var trans = _context.Transactions
                .Include(trans => trans.Wallet)
                .ThenInclude(trans => trans.Customer)
                .Where(trans => trans.TransId == id)
                .FirstOrDefault();

            if (trans == null)
            {
                return NotFound();
            }

            return trans;
        }



        // Post: api/Transaction/
        [HttpPost("PostTransactionDe")]
        public async Task<ActionResult<Transaction>> PostTransactionDe (Transaction transaction)
        {

            try
            {
                //  using (var scope = new TransactionScope(TransactionScopeOption.Required, new
                //  TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
                // using (TransactionScope ts = new TransactionScope())
                using (TransactionScope ts = CreateTransactionScope())
                {
                    var tran = new Transaction();
                    tran.WalletId = transaction.WalletId;
                    tran.Amount = transaction.Amount;
                    tran.Destination = transaction.Destination;
                    tran.Type = transaction.Type;
                    tran.TimeCreate = DateTime.Now;
                    tran.WalletNo = transaction.WalletNo;

                    var wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);

                    wallet.Balance = wallet.Balance + tran.Amount;
                    wallet.TimeUpdate = DateTime.Now;
                    _context.Transactions.Add(tran);
                    _context.SaveChanges();
                    // scope.Complete();
                    var transactionResult = _context.Transactions
                    //    .Include(transaction => transaction.Wallet)
                    //    .ThenInclude(transaction => transaction.Customer)
                    .FirstOrDefault(transaction => transaction.TransId == tran.TransId);

                    ts.Complete();
                    return transactionResult;
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [HttpPost("PostTransactionTest")]
        public static int PostTransactionTest (Transaction transaction)
        {
            Queue<Transaction> queue = new Queue<Transaction>();
            queue.Enqueue(transaction);
            System.Diagnostics.Debug.WriteLine(queue.Count());
            return queue.Count();

          //  ShortStringDictionary mySSC = new ShortStringDictionary();
       //     mySSC.Add("One", "a");
        }

        [HttpPost("PostTransactionPage")]
        public async Task<ActionResult<Transaction>> PostTransactionPage (Transaction transaction)
        {
           /* while(1 == 1)
            {
                if(queue.Count > 0)
                {
                    Transaction tran = new Transaction();
                    Transaction  transaction = queue.Peek();
                    tran.WalletId = transaction.WalletId;
                    tran.Amount = transaction.Amount;
                    tran.Destination = transaction.Destination;
                    tran.Type = transaction.Type;
                    tran.TimeCreate = DateTime.Now;
                    tran.WalletNo = transaction.WalletNo;
                    //var wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
                    //wallet.Balance = wallet.Balance + tran.Amount;
                    //wallet.TimeUpdate = DateTime.Now;

                    Wallet wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
                    System.Diagnostics.Debug.WriteLine(wallet.Balance);
                    wallet.Balance = wallet.Balance + transaction.Amount;
                    System.Diagnostics.Debug.WriteLine(wallet.Balance);
                    wallet.TimeUpdate = DateTime.Now;

                    _context.Transactions.Add(tran);
                    _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("Dequeue()");
                    queue.Dequeue();
                }
                Thread.Sleep(100);
            }  */
          
            var tran = new Transaction();
            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;
            _context.Transactions.Add(tran);
            //var wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
            //wallet.Balance = wallet.Balance + tran.Amount;
            //wallet.TimeUpdate = DateTime.Now;
            _context.SaveChanges();

            // Thread thread = new Thread(() => ThreadProc(transaction));
            Queue<Transaction> queue = new Queue<Transaction>();

            queue.Enqueue(tran);
            System.Diagnostics.Debug.WriteLine(queue.Peek().TransId);
            System.Diagnostics.Debug.WriteLine(queue.Count);

            if (queue.Count > 0)
                Thread.Sleep(1000);

            while (queue.Peek().TransId != tran.TransId)
            {
                System.Diagnostics.Debug.WriteLine(queue.Peek().TransId == tran.TransId);

                Thread.Sleep(1000);
            }
            System.Diagnostics.Debug.WriteLine(queue.Peek().TransId == tran.TransId);

            var transactionResult = new Transaction();

            if (queue.Peek().TransId == tran.TransId)
            {

                System.Diagnostics.Debug.WriteLine(queue.Peek().TransId + " " + tran.TransId);
                /* Thread thread = new Thread(() => ThreadProc2(tran));
                 System.Diagnostics.Debug.WriteLine("threadStart");
                 thread.Start();

                 thread.Join();
                 System.Diagnostics.Debug.WriteLine("Join");
                 transactionResult = _context.Transactions
                      //    .Include(transaction => transaction.Wallet)
                      //    .ThenInclude(transaction => transaction.Customer)
                      .FirstOrDefault(transaction => transaction.TransId == tran.TransId);
                */
                System.Diagnostics.Debug.WriteLine("start");
                Wallet wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
                System.Diagnostics.Debug.WriteLine("1st " + wallet.Balance);
                wallet.Balance = wallet.Balance + transaction.Amount;
                System.Diagnostics.Debug.WriteLine("2nd " + wallet.Balance);
                wallet.TimeUpdate = DateTime.Now;

                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Dequeue()");
                queue.Dequeue();
            }
            return transactionResult; 
        }

        
        [HttpPost("PostTransactionDet")]
        public async Task<ActionResult<Transaction>> PostTransactionDet (Transaction transaction)
        {

            var tran = new Transaction();
            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;
            _context.Transactions.Add(tran);
            //var wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
            //wallet.Balance = wallet.Balance + tran.Amount;
            //wallet.TimeUpdate = DateTime.Now;
            _context.SaveChanges();

            // Thread thread = new Thread(() => ThreadProc(transaction));
            Queue<Transaction> queue = new Queue<Transaction>();

            queue.Enqueue(tran);
            System.Diagnostics.Debug.WriteLine(queue.Peek().TransId);
            System.Diagnostics.Debug.WriteLine(queue.Count);
            
            if (queue.Count > 0)
                Thread.Sleep(1000);

            while (queue.Peek().TransId != tran.TransId)
            {
                System.Diagnostics.Debug.WriteLine(queue.Peek().TransId == tran.TransId);

                Thread.Sleep(1000);
            }
            System.Diagnostics.Debug.WriteLine(queue.Peek().TransId == tran.TransId);

            var transactionResult = new Transaction();

            if (queue.Peek().TransId == tran.TransId)
            {

                System.Diagnostics.Debug.WriteLine(queue.Peek().TransId + " " + tran.TransId);
                /* Thread thread = new Thread(() => ThreadProc2(tran));
                 System.Diagnostics.Debug.WriteLine("threadStart");
                 thread.Start();

                 thread.Join();
                 System.Diagnostics.Debug.WriteLine("Join");
                 transactionResult = _context.Transactions
                      .FirstOrDefault(transaction => transaction.TransId == tran.TransId);
                */

                System.Diagnostics.Debug.WriteLine("start");
                Wallet wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
                System.Diagnostics.Debug.WriteLine("1st " +wallet.Balance);
                wallet.Balance = wallet.Balance + transaction.Amount;
                System.Diagnostics.Debug.WriteLine("2nd " +wallet.Balance);
                wallet.TimeUpdate = DateTime.Now;

                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Dequeue()");
                queue.Dequeue();
            }
            return transactionResult;
        }


        [HttpGet("thread")]
        public async Task<ActionResult<Transaction>> ThreadProc3 (Transaction transaction )
        {
            int result = ThreadProc(transaction);
            var resultTransaction = _context.Transactions.Where(x => x.TransId == result).FirstOrDefault();
            BackgroundPrinter.AddTransaction(resultTransaction);

            return resultTransaction;
        }

        public int ThreadProc (Transaction transaction)
        {
            Thread thread = Thread.CurrentThread;
            //Transaction transaction = queue.Peek();
            Transaction tran = new Transaction();
            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;
            _context.Transactions.Add(tran);
            _context.SaveChanges();
            return tran.TransId;
        }



        public void ThreadProc2 (Transaction transaction)
        {

            Wallet wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
            System.Diagnostics.Debug.WriteLine(wallet.Balance);
            wallet.Balance = wallet.Balance + transaction.Amount;
            System.Diagnostics.Debug.WriteLine(wallet.Balance);
            wallet.TimeUpdate = DateTime.Now;

            _context.SaveChanges();
            System.Diagnostics.Debug.WriteLine("Dequeue()");
            Queue<Transaction> queue = new Queue<Transaction>();
            queue.Dequeue();

        }

  
[HttpPost("PostTransactionDee")]
        public async Task<ActionResult<Transaction>> PostTransactionDee (Transaction transaction)
        {
            
                var wallet = new Wallet();
                var tran = new Transaction();
         
                lock (balanceLock)
                {

                    tran.WalletId = transaction.WalletId;
                    tran.Amount = transaction.Amount;
                    tran.Destination = transaction.Destination;
                    tran.Type = transaction.Type;
                    tran.TimeCreate = DateTime.Now;
                    tran.WalletNo = transaction.WalletNo;
                    _context.Transactions.Add(tran);

                    wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);

                    wallet.Balance = wallet.Balance + tran.Amount;
                    wallet.TimeUpdate = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine(wallet.Balance);
                    _context.SaveChanges();
                    Thread.Sleep(100);
                }   
                         
            return tran;
        }


        public  TransactionScope CreateTransactionScope ()
            {
                TransactionOptions transactionOptions = new TransactionOptions();
                transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
                transactionOptions.Timeout = TransactionManager.MaximumTimeout;
                return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
            }
        

        private int saveTransaction (Transaction transaction)
        {
            var tran = new Transaction();
            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;

            var wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);

            wallet.Balance = wallet.Balance + tran.Amount;
            wallet.TimeUpdate = DateTime.Now;
            _context.Transactions.Add(tran);
            _context.SaveChanges();
            return tran.TransId;
        }

     

           

   

            /*int tran = 0 ;
            Thread thread = new Thread(() =>
           {
               tran = saveTransaction(transaction);
           });
            thread.Start();

            thread.Join();
            var transactionResult = _context.Transactions
                          //    .Include(transaction => transaction.Wallet)
                          //    .ThenInclude(transaction => transaction.Customer)
                .FirstOrDefault(transaction => transaction.TransId == tran);
            */
        

        // Post: api/Transaction/
        [HttpPost("PostTransactionDetails")]
        public async Task<ActionResult<Transaction>> PostTransactionDetails (Transaction transaction)
        {
            var tran = new Transaction();
            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;

            var wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);
            if (wallet == null)
            {
                return NotFound();
            }

            wallet.Balance = wallet.Balance + tran.Amount;
            wallet.TimeUpdate = DateTime.Now;
            _context.Transactions.Add(tran);
            _context.SaveChanges();

            var transactionResult = _context.Transactions
                         .FirstOrDefault(transaction => transaction.TransId == tran.TransId);

            if (transactionResult == null)
            {
                return NotFound();
            }

            return transactionResult;
        }



        // PUT: api/Transaction/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction (int id, Transaction transaction)
        {
            if (id != transaction.TransId)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var tran = await _context.Transactions.FindAsync(id);
            if (tran == null)
            {
                return NotFound();
            }

            tran.WalletId = transaction.WalletId;
            tran.WalletNo = transaction.WalletNo;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TransactionExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Transaction
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction (Transaction transaction)
        {
            DateTime localDate = DateTime.Now;
            var trans = new Transaction
            {
                WalletId = transaction.WalletId,
                WalletNo = transaction.WalletNo,
                Destination = transaction.Destination,
                Amount = transaction.Amount,
                Type = transaction.Type,
                TimeCreate = localDate
            };

            _context.Transactions.Add(trans);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetTransaction", new { id = trans.TransId }, trans);
        }

        // DELETE: api/Transaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction (int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool TransactionExists (int id)
        {
            return _context.Transactions.Any(e => e.TransId == id);
        }

        // GET: api/Transaction/GetTransactionType
        [HttpGet("GetTransactionTypes")]
        public async Task<ActionResult<IEnumerable<TransactionType>>> GetTransactionTypes ()
        {
            //  return await _context.TodoItems.ToListAsync();
            return await _context.TransactionTypes.ToListAsync();
        }

        // GET: api/Transaction/GetTransactionType/5
        [HttpGet("GetTransactionType/{id}")]
        public async Task<ActionResult<TransactionType>> GetTransactionTypes (int id)
        {
            var transaction = await _context.TransactionTypes.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // POST: api/transaction/AddTransactionType
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("AddTransactionType")]
        public async Task<ActionResult<TransactionType>> AddTransactionType (TransactionType transactionType)
        {

            var trans = new TransactionType
            {
                TypeName = transactionType.TypeName,
                Description = transactionType.Description,
                TimeCreate = DateTime.Now
            };

            _context.TransactionTypes.Add(trans); ;
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction("GetTransactionTypes", new { id = trans.TypeId }, trans);
        }

        // PUT: api/Customer/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("PutTransactionType/{id}")]
        public async Task<IActionResult> PutTransactionType (int id, TransactionType transactionType)
        {
            if (id != transactionType.TypeId)
            {
                return BadRequest();
            }

            // _context.Entry(todoItem).State = EntityState.Modified;
            var transaction = await _context.TransactionTypes.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            transaction.TypeName = transactionType.TypeName;
            transaction.Description = transactionType.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TransactionTypeExists(id))
            {
                return NotFound();
            }
            return NoContent();
        }
        private bool TransactionTypeExists (int id)
        {
            return _context.TransactionTypes.Any(e => e.TypeId == id);
        }

        // DELETE: api/Transaction/5
        [HttpDelete("TransactionType/{id}")]
        public async Task<IActionResult> DeleteTransactionType (int id)
        {
            var transaction = await _context.TransactionTypes.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            

            _context.TransactionTypes.Remove(transaction);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        public static void UpdateBalance (Transaction transaction)
        {
            // var transaction = await _context.TransactionTypes.FindAsync(id);
           // UpdateBalance2(transaction);
        }

        public  void UpdateBalance2 (Transaction transaction)
        {
            // var transaction = await _context.TransactionTypes.FindAsync(id);

        }
    }
   
    public  class BackgroundPrinter : IHostedService, IDisposable
    {
        private readonly WalletDBContext _context;
        private readonly ILogger<BackgroundPrinter> logger;
        private Timer timer;
        static Queue<Transaction> queuetest = new Queue<Transaction>();
        private readonly IServiceScopeFactory scopeFactory;
    
        public BackgroundPrinter (ILogger<BackgroundPrinter> logger , IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
            _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<WalletDBContext>();
        }

        public static void AddTransaction (Transaction transaction)
        {
            Thread.Sleep(100);
            System.Diagnostics.Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
            queuetest.Enqueue(transaction);
        }

        public void updateBalance (Transaction transaction)
        {
            Wallet wallet = _context.Wallets
                .Where(x => x.WalletId == transaction.WalletId)
                .FirstOrDefault();
            wallet.Balance = wallet.Balance + transaction.Amount;
            wallet.TimeUpdate = DateTime.Now;
            _context.SaveChanges();
        }

        public void Dispose ()
        {
            timer?.Dispose();
        }

        public Task StartAsync (CancellationToken cancellationToken)
        {
            timer = new Timer(o =>
            {
                string te = queuetest.Count.ToString();
                logger.LogInformation($"test {te} ");
                if (queuetest.Count > 0) {
                    updateBalance(queuetest.Dequeue());
                    // Interlocked.Increment(ref number);
                    // logger.LogInformation($"Printing the worker number {number}");
                }

            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(100));
            
            return Task.CompletedTask;
        }

        public Task StopAsync (CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
