using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dmessage
{
    class Client
    {
        public static void StartClient()
        {
            byte[] bytes = new byte[1024];
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = null;
                foreach (IPAddress addr in ipHostInfo.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = addr;
                        break;
                    }
                }
                if (ipAddress == null)
                    throw new Exception("[ERROR]Can't find ipAddress");

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    string read;
                    int bytesSent;
                    while (true)
                    {
                        read = Console.ReadLine() + "<eof>";
                        if (read.ToLower() == "<end><eof>")
                        {
                            bytesSent = sender.Send(Encoding.ASCII.GetBytes("<end>"));
                            break;
                        }
                        else
                        {
                            msg = Encoding.ASCII.GetBytes(read);
                            bytesSent = sender.Send(msg);
                        }
                    }

                    Console.WriteLine("Connection with {0} closed.", remoteEP);
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    class Server
    {
        public static string data = null;

        public static void StartListening()
        {
            try
            {
                byte[] bytes = new Byte[1024];

                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = null;
                foreach (IPAddress addr in ipHostInfo.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = addr;
                        break;
                    }
                }
                if (ipAddress == null)
                    throw new Exception("[ERROR]Can't find ipAddress");

                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

                Console.WriteLine("localEndPoint: {0}", localEndPoint);

                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    listener.Bind(localEndPoint);
                    listener.Listen(10);
                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection...");

                        Socket handler = listener.Accept();
                        Console.WriteLine("Connection from {0}", handler.RemoteEndPoint);
                        data = null;
                        bytes = new byte[1024];
                        while (true)
                        {
                            int bytesRec = handler.Receive(bytes);
                            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                            if (data.IndexOf("<end>") > -1)
                                break;

                            if (data.IndexOf("<eof>") > -1)
                            {
                                Console.WriteLine(data);
                                data = null;
                            }
                        }

                        Console.WriteLine("Client {0} has closed the connection.", handler.RemoteEndPoint);

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Console.WriteLine("\nPress ENTER to continue...");
                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string read = Console.ReadLine();
            if (read == "server")
                Server.StartListening();
            else if (read == "client")
                Client.StartClient();

            Console.ReadLine();
        }
    }
}
