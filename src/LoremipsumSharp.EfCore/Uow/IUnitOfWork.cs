using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LoremipsumSharp.EfCore.Uow;
using Microsoft.EntityFrameworkCore;

namespace LoremipsumSharp.EfCore
{
    public interface IUnitOfWork : IDisposable
    {

        void Begin(IsolationLevel isolationLevel = IsolationLevel.RepeatableRead);

        void Complete();

        Task CompleteAsync();

        event EventHandler Completed;

    }
    
}