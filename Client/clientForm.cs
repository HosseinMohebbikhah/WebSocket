using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class clientForm : Form
    {
        long myId;
        WebSocketClient client = new WebSocketClient();

        public clientForm()
        {
            InitializeComponent();
            client.OnDataReceived += (data) =>
            {
                DataModel dataReceived = JsonSerializer.Deserialize<DataModel>(data);
                string from = (dataReceived.data.from == 111) ? "Server" : dataReceived.data.from.ToString();
                switch (dataReceived.type)
                {
                    case "message":
                        this.Invoke(new Action(() =>
                        {
                            lsbChats.Items.Add($"\nReceived '{dataReceived.type}' data from {from} has this message : {dataReceived.data.message}");
                        }));
                        break;
                    case "settings":
                        this.Invoke(new Action(() =>
                        {
                            lsbChats.Items.Add($"\nReceived '{dataReceived.type}' data from {from} has settings data");
                        }));
                        break;
                    case "socket":
                        break;
                }
            };

            client.OnChangedStatus += (status) =>
            {
                lblStatus.Text = status;
            };

            client.OnConnected += () =>
            {
                string encodeSettings = JsonSerializer.Serialize<DataModel>(new DataModel
                {
                    type = "settings",
                    data = new DataModel.Data
                    {
                        from = myId
                    },
                    settings = new DataModel.Settings
                    {
                        id = myId,
                    },
                });
                _ = client.SendAsyncMessage($"{encodeSettings}");
            };

            _ = client.ConnectAsync(txtURL.Text);

        }

        long getID()
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        private void clientForm_Load(object sender, EventArgs e)
        {
            myId = getID();
            lblID.Text = myId.ToString();
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            string input = txtMessage.Text;
            string enccodeMessage = JsonSerializer.Serialize<DataModel>(new DataModel
            {
                type = "message",
                data = new DataModel.Data
                {
                    message = txtMessage.Text,
                    from = myId,
                    to = long.Parse(txtTo.Text),
                }
            });
            _ = client.SendAsyncMessage(enccodeMessage);
            lsbChats.Items.Add($"\n Send message to {txtTo.Text}");
        }

        private void clientForm_FormClosing(object sender, FormClosingEventArgs e) => client.Dispose();
    }
}
