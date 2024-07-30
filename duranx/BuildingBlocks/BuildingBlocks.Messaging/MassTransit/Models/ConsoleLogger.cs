
using BuildingBlocks.Messaging.MassTransit.Interfaces;

namespace BuildingBlocks.Messaging.MassTransit.Models
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
