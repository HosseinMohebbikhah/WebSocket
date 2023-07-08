using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Windows.Forms;

namespace Server
{
    public partial class serverForm : Form
    {
        WebSocketServer server = new WebSocketServer();
        List<Clients> clients = new List<Clients>();

        bool isConnected = false;

        public serverForm()
        {
            InitializeComponent();
        }

        async private void serverForm_Load(object sender, EventArgs e)
        {
            server.OnDataReceived += Server_OnDataReceived;
            server.OnChangedStatus += (status) => this.Invoke(new Action(() => lblStatus.Text = status));
            await server.StartAsync(txtURL.Text);
        }

        private void Server_OnDataReceived(string data, WebSocket webSocket)
        {
            Clients client = new Clients();
            DataModel dataReceived = JsonSerializer.Deserialize<DataModel>(data);

            if (clients.Count((item) => item.getWebSocket() == webSocket) == 0)
            {
                if (dataReceived.type == "settings")
                {
                    this.Invoke(new Action(() =>
                    {
                        lsbClients.Items.Add(dataReceived.settings.id);
                        lsbChats.Items.Add($"{dataReceived.settings.id} connected!.");
                    }));
                    client.setId(dataReceived.settings.id);
                    client.setWebSocket(webSocket);
                    clients.Add(client);
                }
            }
            else
                client = clients.Where((item) => item.getWebSocket() == webSocket).First();

            switch (dataReceived.type)
            {
                case "message":
                    if (dataReceived.data.to == 111)
                    {
                        this.Invoke(new Action(() =>
                        {
                            lsbChats.Items.Add($"Received '{dataReceived.type}' data from {dataReceived.data.from} to send to the Server has this message : {dataReceived.data.message}");
                        }));
                    }
                    else
                    {
                        sendMessage(dataReceived.data.message, dataReceived.data.from, dataReceived.data.to);
                        this.Invoke(new Action(() =>
                        {
                            lsbChats.Items.Add($"Received '{dataReceived.type}' data from {dataReceived.data.from} to send to the {dataReceived.data.to}");
                        }));
                    }
                    break;

                case "settings":
                    isConnected = true;
                    this.Invoke(new Action(() =>
                    {
                        lsbChats.Items.Add($"Received '{dataReceived.type}' data from {dataReceived.data.from} to send to the Server");
                    }));
                    break;
            }
            string encodeMessage = JsonSerializer.Serialize<DataModel>(new DataModel
            {
                type = "socket",
                data = new DataModel.Data
                {
                    message = "ok",
                },
            });
            server.SendAsync(webSocket, $"{encodeMessage}").Wait();
        }

        void sendMessage(string message, long from = 0, long to = 0)
        {
            if (clients.Count() > 0)
            {
                if (to != 0)
                {
                    clients.RemoveAll(ws => !(ws.getWebSocket().State == WebSocketState.Open));
                    string encodeMessage = JsonSerializer.Serialize<DataModel>(new DataModel
                    {
                        type = "message",
                        data = new DataModel.Data
                        {
                            message = message,
                            from = from
                        },
                    });
                    server.SendAsync(clients.Where(cl => cl.getId() == to).First().getWebSocket(), $"{encodeMessage}").Wait();
                }
                else
                {
                    foreach (Clients client in clients)
                    {
                        string encodeMessage = JsonSerializer.Serialize<DataModel>(new DataModel
                        {
                            type = "message",
                            data = new DataModel.Data
                            {
                                message = message,
                                from = from
                            },
                        });
                        server.SendAsync(client.getWebSocket(), $"{encodeMessage}").Wait();
                    }
                }
            }
        }

        private void serverForm_FormClosing(object sender, FormClosingEventArgs e) => server.Dispose();

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                lsbChats.Items.Add($"Send Message '{txtMessage.Text}' to all clients.");
            }));
            sendMessage(txtMessage.Text, 111);
        }

        private void checkerClients_Tick(object sender, EventArgs e)
        {
            if (clients.Count > 0)
            {
                clients.RemoveAll(ws => !(ws.getWebSocket().State == WebSocketState.Open || ws.getWebSocket().State == WebSocketState.Connecting));
                this.Invoke(new Action(() =>
                {
                    lsbClients.Items.Clear();
                    lsbClients.Items.AddRange(clients.Select(client => client.getId().ToString()).ToArray());
                }));
            }
            else
            {
                if (isConnected)
                {
                    isConnected = false;
                    this.Invoke(new Action(() =>
                    {
                        lblStatus.Text = "Listening...";
                    }));
                }
            }
        }
    }
}
