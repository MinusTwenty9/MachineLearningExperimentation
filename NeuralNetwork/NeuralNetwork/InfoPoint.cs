using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace ZNetwork
{
    public class InfoPoint
    {
        #region Variables

        private Thread con_thread;
        private bool is_running = false;
        private Encoding enc = Encoding.GetEncoding(850);

        private TcpListener listener;
        public int port = 128;

        #region Msg content

        public string msg_title = "InfoPoint";
        public string msg_body = "<h2>Info:</h2><br/>";

        private string http_head = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: [LENGTH]\r\nAccept-Ranges: bytes\r\n\r\n";

        private string http_html = "<!DOCTYPE html><html><head><title>[TITLE]</title></head></body>[BODY]</body></html>";

        #endregion

        #endregion

        #region Constructor

        public InfoPoint()
        { }

        #endregion

        #region Methods

        public void Initialize()
        {
            if (con_thread != null)
                con_thread.Abort();

            con_thread = new Thread(new ThreadStart(accept_cons));
            con_thread.ApartmentState = ApartmentState.STA;
            con_thread.Start();
        }

        public void Close()
        {
            is_running = false;

            if (con_thread != null)
                con_thread.Abort();
        }

        public void Set_Message_Body(string msg_body)
        {
            this.msg_body = msg_body;
        }

        public void Set_Message_Title(string msg_title)
        {
            this.msg_title = msg_title;
        }

        public void Wait_For_Start(int time_out = 10000)
        {
            int elapsed = 0;
            while (is_running == false && elapsed < time_out)
            {
                Thread.Sleep(15);
                elapsed += 15;
            }
        }

        private void accept_cons()
        {
            is_running = true;
            // Find next free port up from 128
            int index = this.port;
            while (!is_port_free((this.port = index)))
                index++;

            // Start listening on set port
            listener = new TcpListener(this.port);
            listener.Start();

            while (is_running)
            {
                TcpClient mem_client = listener.AcceptTcpClient();
                NetworkStream mem_net_stream = mem_client.GetStream();

                Thread msg_thread = new Thread(new ParameterizedThreadStart(send_msg));
                msg_thread.ApartmentState = ApartmentState.STA;
                msg_thread.Start((object)mem_net_stream);


                //mem_client.Close();
                //mem_client = null;

                //mem_net_stream.Close();
                //mem_net_stream.Dispose();
            }

            listener.Stop();
        }

        // Send the InfoPoint message
        private void send_msg(object obj_net_stream)
        {
            NetworkStream net_stream = (NetworkStream)obj_net_stream;

            string msg = http_html.Replace("[TITLE]", msg_title);
            msg = msg.Replace("[BODY]", msg_body);

            string send = http_head.Replace("[LENGTH]", msg.Length.ToString()) + msg;

            send_data(send, net_stream);
            Thread.Sleep(1500);
            //send_data(msg,net_stream);
        }

        // Send string over net_stream
        private void send_data(string data, NetworkStream net_stream)
        {
            byte[] s_buffer = enc.GetBytes(data);
            net_stream.Write(s_buffer, 0, s_buffer.Length);
        }

        // Receive string over net_stream
        private string receive_data(NetworkStream net_stream)
        {
            string back = "";
            byte[] r_buffer = new byte[1024];

            do
            {
                int r_length = net_stream.Read(r_buffer, 0, r_buffer.Length);
                back += enc.GetString(r_buffer);

            } while (net_stream.DataAvailable);

            return back;
        }

        // Check if port is already bound to other socket listener
        private bool is_port_free(int port)
        {
            bool back = true;

            IPGlobalProperties ip_global_prop = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcp_cons = ip_global_prop.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcp_con in tcp_cons)
                if (tcp_con.LocalEndPoint.Port == port)
                {
                    back = false;
                    break;
                }

            return back;
        }

        #endregion
    }
}
