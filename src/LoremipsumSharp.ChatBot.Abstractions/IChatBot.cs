
using System.Threading.Tasks;

namespace LoremipsumSharp.ChatBot.Abstractions
{
    public interface IChatBot
    {
        Task SendNotificationAsync(string message);
    }
}