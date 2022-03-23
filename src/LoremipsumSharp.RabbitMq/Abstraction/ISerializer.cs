using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq.Abstraction
{
    public interface ISerializer<TMessage>
    {
        byte[] Serialize(TMessage message);
        TMessage Deserialize(byte[] bytes);
    }
}