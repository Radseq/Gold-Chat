using Server.Interfaces;
using System;

namespace Server
{
    class LoggerToConsole : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
