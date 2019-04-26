using System;

namespace IrcBot
{
    class Program
    {
        const string nickname = "carter983569";
        const string realname = "Nick Jones";
        const string channels = "#botpool";

        const string owner_nick = "awfakwfakw";

        static void Main(string[] args)
        {

            Irc irc = new Irc
            {
                ThrowExceptions = false
            };
            irc.MessageReceived += Irc_MessageReceived;
            irc.MessageSent += Irc_MessageSent;
            irc.StateChanged += Irc_StateChanged;
            irc.Connect("irc.freenode.net", 6667);

            Console.ReadLine();
        }

        private static void Irc_StateChanged(object sender, bool e)
        {
            Irc irc = (Irc)sender;
            if (e)
            {
                Log("Connected", ConsoleColor.Cyan);
                irc.Ident(nickname, realname);
                irc.Join(channels);

            }
            else
            {
                Log("Disconnected", ConsoleColor.Red);
            }
        }

        private static void Irc_MessageSent(object sender, int e)
        {
            // print that the message was sent
            Log(string.Format("Sent {0} characters", e.ToString()), ConsoleColor.DarkGray);
        }

        private static void Irc_MessageReceived(object sender, string e)
        {
            Irc irc = (Irc)sender;

            // ping pong response
            if (e.IndexOf("PING") == 0)
            {
                irc.Send("PONG " + e.Substring(e.IndexOf(':')));
            }
            int index = e.IndexOf("PRIVMSG");
            if (index != -1)
            {
                int message_start = e.IndexOf(':', index) + 1;
                if (message_start == -1)
                    return;

                int sender_nickname_start = e.IndexOf(':') + 1;
                if (sender_nickname_start == -1)
                    return;

                int sender_nickname_end = e.IndexOf('!', sender_nickname_start);
                if (sender_nickname_end == -1)
                    return;

                string sender_nickname = e.Substring(sender_nickname_start, sender_nickname_end - sender_nickname_start);
                string message = e.Substring(message_start);

                Log("[Private Message]", ConsoleColor.Red);
                Log("Sender:  " + sender_nickname, ConsoleColor.Red);
                Log("Message: " + message, ConsoleColor.Red);

                if (sender_nickname == owner_nick && message.IndexOf("!download") == 0)
                {
                    string url = message.Substring(
                        message.IndexOf(' ') + 1
                    );
                    irc.Send(string.Format("PRIVMSG {0} :{1}", channels, "Now downloading "+ url));
                }
            }

            // print it out
            Log(e, ConsoleColor.Gray);
        }

        private static void Log(string message, ConsoleColor color)
        {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine("[{0}] {1}", DateTime.Now.ToString("hh:mm:ss"), message);
            Console.ForegroundColor = previous;
        }
    }
}
