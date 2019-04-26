using System;

namespace IrcBot
{
    class Program
    {
        const string nickname = "carter983569";
        const string realname = "Nick Jones";
        const string channels = "#botpool";

        const string owner_nick = "somebodyrandom24";

        const string client_software = "KmSPike v3.5";

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
                Console.WriteLine("Connected");
                irc.Ident(nickname, realname);
                irc.Join(channels);

            }
            else
            {
                Console.WriteLine("Disconnected");
            }
        }

        private static void Irc_MessageSent(object sender, int e)
        {
            // print that the message was sent
            Console.WriteLine("[{0}]: Sent {1} characters", DateTime.Now.ToString("hh:mm:ss"), e.ToString());
        }

        private static void Irc_MessageReceived(object sender, string e)
        {
            Irc irc = (Irc)sender;

            // ping pong response
            if (e.IndexOf("PING") == 0)
            {
                irc.Send("PONG " + e.Substring(e.IndexOf(':')));
            }
            int index  = e.IndexOf("PRIVMSG");
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
                string message = e.Substring(message_start );

                Console.WriteLine("[[[[[[[[PRIVATE MESSAGE]]]]]]]]]");
                Console.WriteLine(sender_nickname );
                Console.WriteLine(message);

                if (sender_nickname == owner_nick )
                {
                    if (message == "!download")
                    {
                        irc.Send(string.Format("PRIVMSG {0} :{1}", channels, "Now downloading"));
                    }
                }

                if (message == "VERSION")
                {
                    irc.Send(string.Format ("PRIVMSG {0} :{1}", sender_nickname, client_software));
                }
            }

            // print it out
            Console.WriteLine("[{0}]: {1}", DateTime.Now.ToString("hh:mm:ss"), e);
        }
    }
}
