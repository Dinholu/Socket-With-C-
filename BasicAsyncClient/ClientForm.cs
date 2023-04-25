using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace BasicAsyncClient
{
    public partial class ClientForm : Form
    {
        private Socket clientSocket;
        private byte[] buffer;

        public ClientForm()
        {
            InitializeComponent();
        }

        private static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = clientSocket.EndReceive(AR);

                if (received == 0)
                {
                    return;
                }


                string message = Encoding.Unicode.GetString(buffer);

                Invoke((Action) delegate
                {
                    Text = "Server says: " + message;
                });

                // Start receiving data again.
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            // Avoid Pokemon exception handling in cases like these.
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndConnect(AR);
                UpdateControlStates(true);
                buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                clientSocket.EndSend(AR);
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        /// <summary>
        /// A thread safe way to enable the send button.
        /// </summary>
        private void UpdateControlStates(bool toggle)
        {
            Invoke((Action)delegate
            {
                buttonSend.Enabled = toggle;
                buttonConnect.Enabled = !toggle;
                labelIP.Visible = textBoxAddress.Visible = !toggle;
            });
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
               // récuperer picturebox1.image et le transformer en bytes 
                ImageConverter converter = new ImageConverter();
                byte[] imageBytes = (byte[])converter.ConvertTo(pictureBox1.Image, typeof(byte[]));
                

                // Convert the image to base64 string.
                string base64String = Convert.ToBase64String(imageBytes);
                
                System.Diagnostics.Debug.WriteLine(base64String.Length.GetType());
                

                // Serialize the data into a PersonPackage.
                PersonPackage person = new PersonPackage(
                    checkBoxMale.Checked,
                    (ushort)numberBoxAge.Value,
                    textBoxEmployee.Text,
                    textBox1.Text,
                    base64String);

                // Send the PersonPackage over the network.
                byte[] buffer = person.ToByteArray();
                clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, null);
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
                UpdateControlStates(false);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
                UpdateControlStates(false);
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the specified host.
                var endPoint = new IPEndPoint(IPAddress.Parse(textBoxAddress.Text), 3333);
                clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (SocketException ex)
            {
                ShowErrorDialog(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                ShowErrorDialog(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            openFileDialog1.Title = "Select an image file";
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                pictureBox1.Image = new Bitmap(filePath);
            }
        }
    }
}
