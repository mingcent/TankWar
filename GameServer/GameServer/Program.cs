using System.Collections.Generic;
using ENet;
using Google.Protobuf;
using Pb;

namespace GameServer
{
    class Program
    {
        private static void Main(string[] args)
        {
            Library.Initialize();
            if (!DbManager.Connect("game", "127.0.0.1", 3306, "root", ""))
            {
                return;
            }


            NetManager.StartLoop(8888);
            Library.Deinitialize();
        }
    }
}


