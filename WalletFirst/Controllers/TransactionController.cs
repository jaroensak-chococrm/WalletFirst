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
        //  private readonly IServiceScopeFactory scopeFactory;

        private static readonly object balanceLock = new object();

        public int i;

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
            Console.Write("das");
            return trans;
        }

        // GET: api/Transaction/GetTransactionDetails/5
        [HttpGet("GetTransactionDetails/{id}")]
        public ActionResult<Transaction> GetTransactionDetails (int id)
        {
            var trans = _context.Transactions
                .Include(trans => trans.Wallet)
                .ThenInclude(trans => trans.WalletId)
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

        //notuse
        [HttpGet("PostTransactionThreadPool")]
        public async Task<ActionResult<Transaction>> PostTransactionThreadPool ()
        {
            //for (var i = 0; i < 5; i++)
            //    ThreadPool.QueueUserWorkItem(WorkItem, i);

            //ThreadPool.SetMaxThreads(32, 10);
            //ThreadPool.SetMinThreads(16, 10);
            //var x = ThreadPool.SetMaxThreads(32, 10);
            //while (ThreadPool.PendingWorkItemCount != 0)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Thread = {ThreadPool.ThreadCount}");
            //    System.Diagnostics.Debug.WriteLine($"Thread = {ThreadPool.CompletedWorkItemCount}");
            //    System.Diagnostics.Debug.WriteLine($"Thread = {ThreadPool.PendingWorkItemCount}");
            //    System.Diagnostics.Debug.WriteLine($" {x}");
            //    Thread.Sleep(500);
            //}

            ThreadPoolExample.testTing();
            return NotFound();

        }

        //notuse
        public void WorkItem (object o)
        {
            Thread.Sleep(new Random().Next(1000, 1000));
            System.Diagnostics.Debug.WriteLine("WorkItem Jaa");
        }
        //notuse
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
        //notuse
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

        //notuse
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


        [HttpGet("thread")]
        public async Task<ActionResult<WalletWithTransactions>> ThreadProc3 (Transaction transaction)
        {
            // Thread thread = new Thread(() => ThreadProc(transaction));
            int result = AddTransaction(transaction);

            var resultTransaction = _context.Transactions
                .Where(x => x.TransId == result)
                .FirstOrDefault();

            WalletWithTransactions walletUpdate = null;
            BackgroundPrinter.AddTransaction(resultTransaction);

            return walletUpdate;
        }

        public int AddTransaction (Transaction transaction)
        {
            //Thread thread = Thread.CurrentThread;
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


        // not use
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

        private static int intNum = 0;

        //Top up with Lock
        [HttpPost("TopUp1")]
        public ActionResult<Transaction> TopUp1 (Transaction transaction)
        {

            var wallet = new Wallet();
            var tran = new Transaction();
            var result = new Transaction();

            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;
            tran.BalanceO = 0;
            tran.BalanceN = 0;

            lock (balanceLock)
            {

                intNum++;
                _context.Transactions.Add(tran);
                _context.SaveChanges();
              
                wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);

                wallet.Balance = wallet.Balance + tran.Amount;
                wallet.TimeUpdate = DateTime.Now;
                tran.BalanceN = wallet.Balance;
                tran.BalanceO = wallet.Balance - tran.Amount;
                _context.SaveChanges();

                result = _context.Transactions.FirstOrDefault(x => x.TransId == tran.TransId);
                result.Wallet = wallet;
                System.Diagnostics.Debug.WriteLine(" Thread Id = " + Thread.CurrentThread.ManagedThreadId + " TransId = " + result.TransId + " WalletId = " + result.WalletId + " Balance = " + result.Wallet.Balance + "  Count= " + intNum);
                
                return result;

            }

        }

        //Top up with Distributed Lock
        [HttpPost("TopUp2")]
        public ActionResult<Transaction> TopUp2 (Transaction transaction)
        {

            var wallet = new Wallet();
            var tran = new Transaction();
            var connectionString = "Server=DESKTOP-AQCHHJ8;Initial Catalog=WalletDB;Trusted_Connection=True;";
            var myDistributedLock = new SqlDistributedLock(transaction.WalletNo,/* _context.Database.GetConnectionString()*/ connectionString);

            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;
            tran.BalanceN = 0;
            tran.BalanceO = 0;

            using (myDistributedLock.Acquire())
            {

                intNum++;
                _context.Transactions.Add(tran);
                _context.SaveChanges();

                wallet = _context.Wallets.FirstOrDefault(x => x.WalletNo == transaction.WalletNo);

                wallet.Balance = wallet.Balance + tran.Amount;
                wallet.TimeUpdate = DateTime.Now;
                tran.BalanceN = wallet.Balance;
                tran.BalanceO = wallet.Balance - tran.Amount;
                _context.SaveChanges();

                tran.Wallet = wallet;
                System.Diagnostics.Debug.WriteLine(" Thread Id = " + Thread.CurrentThread.ManagedThreadId + " TransId = " + tran.TransId + " WalletId = " + tran.WalletId + " Balance = " + tran.Wallet.Balance + "  Count= " + intNum);

            }
            return tran;

        }

        [HttpPost("TopUp3")]
        public  ActionResult<Transaction> TopUp(Transaction transaction)
        {
            lock (balanceLock)
            {
            var wallet = new Wallet();
            var tran = new Transaction();

            tran.WalletId = transaction.WalletId;
            tran.Amount = transaction.Amount;
            tran.Destination = transaction.Destination;
            tran.Type = transaction.Type;
            tran.TimeCreate = DateTime.Now;
            tran.WalletNo = transaction.WalletNo;
            tran.BalanceO = 0;
            tran.BalanceN = 0;

            _context.Transactions.Add(tran);
            _context.SaveChanges();
            
            var commandText = "BEGIN TRANSACTION; " +
                   " UPDATE[dbo].[Wallet] SET balance = balance + @Amount2 WHERE wallet_id = @myWalletId   " +
                   " UPDATE[dbo].[Transaction] SET balance_o = T2.balance - @Amount , balance_n = T2.balance " +
                   " FROM[dbo].[Transaction] T1 , [dbo].[Wallet] T2            " +
                   " WHERE trans_id = @myTransId     " +
                   " COMMIT;";

            var Amount = new SqlParameter("@Amount", tran.Amount);
            var Amount2 = new SqlParameter("@Amount2", tran.Amount);
            var myWalletId = new SqlParameter("@myWalletId", tran.WalletId);
            var myTransId = new SqlParameter("@myTransId", tran.TransId);
            _context.Database.ExecuteSqlRaw(commandText, new[] { Amount2, myWalletId, Amount, myTransId });
            _context.SaveChanges();
          

            var thing = _context.Transactions.Find(tran.TransId);
            _context.Entry(thing).State = EntityState.Detached;
            var result =  _context.Transactions.FirstOrDefault(x => x.TransId == tran.TransId);
            
            return result;
            }
        }


        public TransactionScope CreateTransactionScope ()
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
    }


    public class BackgroundPrinter : IHostedService, IDisposable
    {
        //private readonly WalletDBContext _context;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<BackgroundPrinter> logger;
        private Timer timer;
        static Queue<Transaction> queueTrans = new Queue<Transaction>();
        static Queue<WalletWithTransactions> queueBalance = new Queue<WalletWithTransactions>();
        private readonly object balanceLock = new object();


        public BackgroundPrinter (ILogger<BackgroundPrinter> logger, IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
            //  _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<WalletDBContext>();
        }

        public static void  AddTransaction (Transaction resultTransaction)
        {

            //Thread.Sleep(100);
            System.Diagnostics.Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
            queueTrans.Enqueue(resultTransaction);

            while (queueBalance.Count == 0)
            {
                Thread.Sleep(100);
            }
           
            while (queueBalance.Peek().TransId != resultTransaction.TransId)
            {
                Thread.Sleep(100);
            }
            
           // System.Diagnostics.Debug.WriteLine($" queueBalance Count Down = {queueBalance.Count} ");
           // System.Diagnostics.Debug.WriteLine($" queueBalance ALL COUNT = {queueBalance2.Count} ");
         //   return 
            queueBalance.Dequeue(); 
                    
        }

        public void updateBalance (Transaction resultTransaction)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dBContext = scope.ServiceProvider.GetRequiredService<WalletDBContext>();

                Wallet wallet = dBContext.Wallets
                .FirstOrDefault(x => x.WalletId == resultTransaction.WalletId);

                wallet.Balance = wallet.Balance + resultTransaction.Amount;
                wallet.TimeUpdate = DateTime.Now;
                dBContext.SaveChanges();

                TempTrans(resultTransaction.TransId , wallet.Balance);
            }
        }

        public void TempTrans (int transId , decimal? balance)
        {
            WalletWithTransactions walletWithTransactions = null;
            walletWithTransactions = new WalletWithTransactions();
            walletWithTransactions.TransId = transId;
            walletWithTransactions.Balance = balance;

               
            queueBalance.Enqueue(walletWithTransactions);
           // queueBalance2.Enqueue(walletWithTransactions);
        }
    

        public void Dispose ()
        {
            timer?.Dispose();
        }

        public Task StartAsync (CancellationToken cancellationToken)
        {
            timer = new Timer(o =>
           {
               lock (balanceLock)
               {
                   string qt = queueTrans.Count.ToString();
                  // logger.LogInformation($"Count down {qt} ");
                   if (queueTrans.Count > 0) {
                       updateBalance(queueTrans.Dequeue());
                       // Interlocked.Increment(ref number);
                       // logger.LogInformation($"Printing the worker number {number}");
                   }
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

    public class Fibonacci
    {
        private ManualResetEvent _doneEvent;

        public Fibonacci (int n, ManualResetEvent doneEvent)
        {
            N = n;
            _doneEvent = doneEvent;
        }

        public int N { get; }

        public int FibOfN { get; private set; }

        public void ThreadPoolCallback (Object threadContext)
        {
            int threadIndex = (int) threadContext;
            System.Diagnostics.Debug.WriteLine($"Thread {threadIndex} started...");
            FibOfN = Calculate(N);
            System.Diagnostics.Debug.WriteLine($"Thread {threadIndex} result calculated...");
            _doneEvent.Set();
        }

        public int Calculate (int n)
        {
            if (n <= 1)
            {
                return n;
            }
            return Calculate(n - 1) + Calculate(n - 2);
        }
    }

    public class ThreadPoolExample
    {
        public static void testTing()
        {
            const int FibonacciCalculations = 2;

            var doneEvents = new ManualResetEvent[FibonacciCalculations];
            var fibArray = new Fibonacci[FibonacciCalculations];
            var rand = new Random();

            System.Diagnostics.Debug.WriteLine($"Launching {FibonacciCalculations} tasks...");
            for (int i = 0; i < FibonacciCalculations; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                var f = new Fibonacci(rand.Next(20, 40), doneEvents[i]);
                fibArray[i] = f;
                ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, i);
            }

            WaitHandle.WaitAll(doneEvents);
            System.Diagnostics.Debug.WriteLine("All calculations are complete.");

            for (int i = 0; i < FibonacciCalculations; i++)
            {
                Fibonacci f = fibArray[i];
                System.Diagnostics.Debug.WriteLine($"Fibonacci({f.N}) = {f.FibOfN}");
            }
        }
    }
}



   

