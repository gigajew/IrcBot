using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Timers;

namespace IrcBot
{
    /*
     * Written by gigajew @ www.hackforums.net
     */
    public class Irc
    {
        public event EventHandler<string> MessageReceived;
        public event EventHandler<int> MessageSent;

        private Socket socketObject;
        private Timer readTimer;
        private Queue<string> messageQueue;

        public Irc(Socket socket)
        {
            socketObject = socket;
            readTimer = new Timer(1000);
            readTimer.Elapsed += OnRead;
            messageQueue = new Queue<string>();
        }

        public void Start()
        {
            readTimer.Start();
        }

        public void Stop()
        {
            readTimer.Stop();

        }

        protected void OnRead(object sender, ElapsedEventArgs e)
        {
            while (messageQueue.Count > 0)
            {
                if (Equals(MessageReceived, null))
                    continue;
                MessageReceived(this, messageQueue.Dequeue());
            }

            bool connected = !((socketObject.Poll(1000, SelectMode.SelectRead) && (socketObject.Available == 0)) || !socketObject.Connected);
            if (!connected)
                readTimer.Stop();

            bool has_data = socketObject.Poll(3000, SelectMode.SelectRead) && socketObject.Available > 0;
            if (!has_data)
                return;

            using (NetworkStream s = new NetworkStream(socketObject))
            using (StreamReader sr = new StreamReader(s))
            {
                int available = socketObject.Available;
                char[] buffer = new char[available];
                int received = sr.Read(buffer, 0, available);

                string raw = new string(buffer);
                string[] messages = raw.Split('\n');
                for (int i = 0; i < messages.Length; i++)
                {
                    string formatted = messages[i].Trim();
                    if (!string.IsNullOrEmpty(formatted))
                        messageQueue.Enqueue(formatted);
                }

                messages = null;
                buffer = null;
            }
        }

        public void SendMessage(string message)
        {
            using (NetworkStream s = new NetworkStream(socketObject))
            using (StreamWriter sw = new StreamWriter(s))
            {
                if (!socketObject.Poll(3000, SelectMode.SelectWrite))
                    throw new Exception("couldn't send");

                sw.WriteLine(message);

                if (Equals(MessageSent, null))
                    return;

                MessageSent(this, message.Length);
            }
        }
    }
}
