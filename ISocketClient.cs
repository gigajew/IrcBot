using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrcBot
{
    public interface ISocketClient
    {
        event EventHandler<bool> StateChanged;
        event EventHandler<string> MessageReceived;
        event EventHandler<int> MessageSent;
        void Connect(string host, int port);
    }
}
