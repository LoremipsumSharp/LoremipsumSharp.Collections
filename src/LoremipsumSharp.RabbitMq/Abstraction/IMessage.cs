using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq.Abstraction
{
    /// <summary>
    ///  标记性接口
    /// </summary>
    public interface IMessage
    {
        string GetDescription();
    }
}