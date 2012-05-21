using System;
using System.Text.RegularExpressions;
using System.Threading;

using System.Text;
using Server.Game;
using Server;

namespace Server {
    class MainClass {
        public static void Main(string[] args) {

            new ServerInstance();

            while(true) {
                Thread.Sleep(100);
            }
        }
    }
}