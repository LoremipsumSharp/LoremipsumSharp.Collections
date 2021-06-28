using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoremipsumSharp.EfCore.Uow
{

    public class EntityFrameworkCoreUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        public TContext DbContext { get; set; }
        private IDbContextTransaction _dbContextTransaction;

        public EntityFrameworkCoreUnitOfWork(TContext context)
        {

            DbContext = context;
        }

        public event EventHandler Completed;

        public void Begin(IsolationLevel isolationLevel = IsolationLevel.RepeatableRead)
        {
            _dbContextTransaction = DbContext.Database.BeginTransaction(isolationLevel);
        }

        public void Complete()
        {

            DbContext.SaveChanges();
            _dbContextTransaction?.Commit();

            OnCompleted();
        }

        public async Task CompleteAsync()
        {

            await DbContext.SaveChangesAsync();
            _dbContextTransaction?.Commit();

            OnCompleted();
        }

        /// <summary>
        /// Called to trigger <see cref="Completed"/> event.
        /// </summary>
        protected virtual void OnCompleted()
        {
            if (Completed == null)
            {
                return;
            }
            Completed(this, EventArgs.Empty);
        }

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContextTransaction?.Dispose();
                    DbContext.Dispose();
                }
            }
            _disposed = true;
        }
    }
}