using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq.Abstraction
{
    public interface IMessageStorage
    {

        /// <summary>
        ///  初始化表结构
        /// </summary>
        /// <returns></returns>
        Task InitSchema();
        /// <summary>
        /// 死信
        /// </summary>
        /// <param name="message"></param>
        /// <param name="extra"></param>
        /// <param name="ex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task StoreDeadLetter(string payload, string description, IDictionary<string, object> extra, Exception ex);
    }
}