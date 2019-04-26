using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IrcBot
{
    public class Irc : ISocketClient
    {
        public event EventHandler<bool> StateChanged;
        public event EventHandler<string> MessageReceived;
        public event EventHandler<int> MessageSent;

        public bool Connected
        {
            get { return m_Connected; }
            protected set { m_Connected = value; }
        }

        public bool ThrowExceptions
        {
            get { return m_ThrowExceptions; }
            set { m_ThrowExceptions = value; }
        }

        public Irc()
        {
            m_Encoding = new UTF8Encoding(false);
            m_SendEvent = new AutoResetEvent(true);
            m_SendQueue = new Queue<string>();
        }

        public void Connect(string host, int port)
        {
            if (Connected || m_Connecting)
                return;

            m_Connecting = true;

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_Socket.BeginConnect(host, port, new AsyncCallback(ConnectCallback), this);
        }

        protected static void ConnectCallback(IAsyncResult ar)
        {
            Irc irc = (Irc)ar.AsyncState;
            List<Exception> exceptions = new List<Exception>();

            try
            {
                irc.m_Socket.EndConnect(ar);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                irc.OnStateChanged(false);
            }

            if (irc.ThrowExceptions && exceptions.Count > 0)
                    throw new AggregateException(exceptions);

            irc.OnStateChanged(irc.m_Socket.Connected);
        }

        protected void Receive()
        {
            if (!Connected || m_Connecting )
                return;

            m_Stream = new NetworkStream(m_Socket);
            m_StreamReader = new StreamReader(m_Stream, m_Encoding);
            m_StreamWriter = new StreamWriter(m_Stream, m_Encoding);
            m_StreamWriter.AutoFlush = true;
            
            do
            {
                string line = null;
                try
                {
                    line = m_StreamReader.ReadLine();
                    if (line == null)
                        throw new SocketException((int)SocketError.ConnectionReset);

                } catch (Exception e)
                {
                    if (ThrowExceptions)
                        throw e;

                    OnStateChanged(false);
                }

                if (!string.IsNullOrEmpty(line) && !string.IsNullOrWhiteSpace(line))
                    OnMessageReceived(line);

            } while (Connected);
        }

        public void Send(string message)
        {
            if (!Connected || m_Connecting)
                throw new InvalidOperationException("not connected");

            lock (m_SendQueue )
            m_SendQueue.Enqueue(message);

            Task.Factory.StartNew(() => Send());
        }

        protected void Send()
        {
            if (m_SendQueueRunning)
                return;

            m_SendQueueRunning = true;

            List<Exception> exceptions = new List<Exception>();

            do
            {
                string message;
                lock (m_SendQueue)
                {
                    if (m_SendQueue.Count == 0)
                        break;
                    message = m_SendQueue.Dequeue();
                }
                try
                {
                    m_StreamWriter.WriteLine(message);
                    OnMessageSent(message);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            } while (Connected);

            if (ThrowExceptions && exceptions.Count > 0)
                throw new AggregateException(exceptions);

            m_SendQueueRunning = false;
        }

        public void Ident(string nick, string real)
        {
            Send(string.Format("NICK {0}", nick));
            Send(string.Format("USER {0} hostname servername :{1}", nick, real));
        }

        public void Join(string chatrooms)
        {
            Send(string.Format("JOIN {0}", chatrooms));
        }

        protected void OnMessageSent(string message)
        {
            EventHandler<int> handler = MessageSent;
            if (!Equals(handler, null))
                handler(this, message.Length);
            m_SendEvent.Set();
        }

        protected void OnMessageReceived(string message)
        {
            EventHandler<string> handler = MessageReceived;
            if (!Equals(handler, null))
                handler(this, message);
        }

        protected void OnStateChanged(bool connected)
        {
            Connected = connected;

            if (connected)
            {
                m_ReceiveThread = new Thread(Receive);
                m_ReceiveThread.IsBackground = true;
                m_ReceiveThread.Start();

                m_Connecting = false;
            }
            else
            {
                if (!Equals(m_ReceiveThread, null))
                    m_ReceiveThread.Abort();
            }

            EventHandler<bool> handler = StateChanged;
            if (!Equals(handler, null))
                handler(this, connected);
        }

        private Encoding m_Encoding;
        private Socket m_Socket;
        private Thread m_ReceiveThread;
        private StreamReader m_StreamReader;
        private StreamWriter m_StreamWriter;
        private NetworkStream m_Stream;
        private AutoResetEvent m_SendEvent;

        private Queue<string> m_SendQueue;

        private bool m_Connected;
        private bool m_Connecting;
        private bool m_SendQueueRunning;

        private bool m_ThrowExceptions;
    }
}
