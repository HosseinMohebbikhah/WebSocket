using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Clients
    {
        private long Id;
        private WebSocket WebSocket;

        public Clients(long id = 0, WebSocket sw = null)
        {
            if (id != 0 && sw != null)
            {
                this.Id = id;
                this.WebSocket = sw;
            }
        }

        public long getId()
        {
            return Id;
        }
        public void setId(long id)
        {
            this.Id = id;
        }
        public WebSocket getWebSocket()
        {
            return this.WebSocket;
        }
        public void setWebSocket(WebSocket ws)
        {
            this.WebSocket = ws;
        }
    }
}
