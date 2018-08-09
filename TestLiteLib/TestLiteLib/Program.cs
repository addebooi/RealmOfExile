using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLiteLib
{
    class Program
    {
        static void Main(string[] args)

        {
            Console.CursorVisible = false;
            var server = new Server();
            server.Start();

        }
    }
}
