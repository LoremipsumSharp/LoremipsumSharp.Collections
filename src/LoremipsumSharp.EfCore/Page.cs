using System.Collections.Generic;

namespace LoremipsumSharp.EfCore
{
    public class Page<T>
    {
        public long TotalCount{get;set;}
        public List<T> Items { get; set; }
    }
}