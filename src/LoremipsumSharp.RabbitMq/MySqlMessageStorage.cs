using Dapper;
using LoremipsumSharp.RabbitMq.Abstraction;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoremipsumSharp.RabbitMq
{
    public class MySqlMessageStorage : IMessageStorage, IDisposable
    {
        private readonly MySqlConnection _dbConnection;

        public MySqlMessageStorage(string connStr)
        {
            _dbConnection = new MySqlConnection(connStr);
            _dbConnection.Open();
        }

        public void Dispose()
        {
            _dbConnection.Dispose();
        }

        public async Task InitSchema()
        {
            var sql =
  @"CREATE TABLE IF NOT EXISTS `message_consume_log` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '自增Id',
  `payload` text NOT NULL COMMENT '消息体',
  `description` varchar(1024) NOT NULL COMMENT '消息描述',
  `extra` text COMMENT '消息附加信息',
  `state` bit(1) NOT NULL COMMENT '消费状态:成功(1),失败(0)',
  `create_timestamp` bigint(20) NOT NULL COMMENT '入库时间',
  `exception` text   COMMENT '异常信息',
  PRIMARY KEY (`Id`),
  KEY `idx_createtimestamp` (`create_timestamp`) USING BTREE COMMENT '时间戳索引'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public async Task StoreDeadLetter(string payload, string description, IDictionary<string, object> extra, Exception ex)
        {

            var sql = @"INSERT INTO `message_consume_log`(`payload`, `description`, `extra`, `state`, `create_timestamp`,`exception`) VALUES (@Payload, @Description, @Extra, @State, @CreateTimestamp,@Exception);";

            await _dbConnection.ExecuteAsync(sql, new
            {
                Payload = payload ?? string.Empty,
                Description = description,
                Extra = JsonConvert.SerializeObject(extra),
                State = 0,
                CreateTimestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds(),
                Exception = ex?.ToString(),
            });


        }
    }
}