using System;
using System.Text.RegularExpressions;
using System.Threading;

using System.Text;
using Server.Game;

namespace Server {
    class MainClass {
        public static void Main(string[] args) {

            ServerInstance server = new ServerInstance();

            while(true) {
                Thread.Sleep(1000);
            }
        }
    }
}