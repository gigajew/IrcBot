using System;
using System.Net.Sockets;

namespace IrcBot
{
    public static class Program
    {
        private static Irc irc;
        private static Socket socket;
        private static int sent;

        /*
         * Configuration
         */
        private const string HOSTNAME = "irc.freenode.net";
        private const int PORT = 6667;
        private const string NICKNAME = "MySuperCoolBot1";
        private const string REALNAME = "Mr. Coolbotio";
        private const string CHATROOMS = "#coolbots,#botpool,#twitter,#botwars";

        public static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(HOSTNAME, PORT );

            irc = new Irc(socket);
            irc.MessageReceived += Read;
            irc.MessageSent += Write;

            irc.SendMessage("NICK " + NICKNAME);
            irc.SendMessage("USER " + NICKNAME + " hostname servername :" + REALNAME);
            irc.SendMessage("JOIN " + CHATROOMS);

            irc.Start();

            Console.ReadLine();
        }

        private static void Write(object sender, int e)
        {
            sent += e;
        }

        private static void Read(object sender, string e)
        {
            /*
             * Pong response, leave this first expression alone
             */
            if (e.StartsWith("PING"))
            {
                Send("PONG " + e.Substring(':') + 1);  return;
            }

            /*
             * Parse some data
             */ 
            int privIndex = e.IndexOf("PRIVMSG");
            if (privIndex == -1)
                return;

            int messageIndex = e.IndexOf(':', 1);
            if (messageIndex == -1)
                return;

            int authorIndex = e.IndexOf('!');
            if (authorIndex == -1)
                return;

            /*
             * Command handling
             */
            string author = e.Substring(1, authorIndex  -1);
            string message = e.Substring(messageIndex + 1);
            switch (message)
            {
                case "!download":
                    Say(CHATROOMS, "download command has started!");
                    break;
                case "!disconnect":
                    Send("QUIT");
                    break;


            }
        }

        private static void Say(string channel, string message)
        {
            irc.SendMessage("PRIVMSG " + channel + " :" + message);
        }

        private static void Send(string raw)
        {
            irc.SendMessage(raw);
        }
    }
}
