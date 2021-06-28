using System;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace LoremipsumSharp.EfCore
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UnitOfWorkAttribute : Attribute
    {
        public UnitOfWorkAttribute(bool isTransactional = false)
        {
            IsTransactional = isTransactional;
        }

        public UnitOfWorkAttribute(IsolationLevel isolationLevel)
        {
            IsTransactional = true;
            IsolationLevel = isolationLevel;
        }
        public bool IsTransactional { get; set; }


        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.RepeatableRead;
    }
}