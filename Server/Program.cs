using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static int port = 8005;
        
        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenSocket.Bind(ipPoint);

                listenSocket.Listen(10);

                Console.Write("Задайте пароль для сервера (пароль має містити лише латиницю та цифри):");
                string password = Console.ReadLine();
                int n = password.Length;
                
                while (true)
                {
                    Console.WriteLine("Очікую на клієнта...");
                    Socket handler = listenSocket.Accept();
                    Console.WriteLine("Клієнт підключився до серверу.");
                    
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];

                    handler.Send(Encoding.Unicode.GetBytes(n.ToString()));//Send size of password
                    
                    do
                    {
                        data = new byte[256];
                        bytes = handler.Receive(data);//Receive type of guessing
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    if (builder.ToString() == "user")
                    {
                        bool isGuessed = false;
                        while (!isGuessed)
                        {
                            isGuessed = Guessing(handler, password);
                        }
                    }
                    else
                    {
                        builder.Clear();
                        do
                        {
                            data = new byte[256];
                            bytes = handler.Receive(data);//Receive time
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (handler.Available > 0);
                        
                        var finish = DateTime.Now.Ticks + (long) int.Parse(builder.ToString()) * 10000000;
                        
                        bool isGuessed = false;
                        int count = 0;
                        while (!isGuessed )
                        {
                            isGuessed = Guessing(handler, password);
                            count++;
                            if (DateTime.Now.Ticks > finish)
                            {
                                handler.Send(Encoding.Unicode.GetBytes("!"));
                                count++;
                                break;
                            }
                        }
                        handler.Send(Encoding.Unicode.GetBytes(count.ToString()));//Send number of attempts
                    }
                    

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static bool Guessing(Socket handler, string password)
        {
            
            var builder = new StringBuilder();
                do
                {
                    var data = new byte[256];
                    var bytes = handler.Receive(data);//Receive guess
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (handler.Available > 0);

                if (builder.ToString() == password)
                {
                    handler.Send(Encoding.Unicode.GetBytes("Yes"));//Send info that pass is correct
                    return true;
                }
                else if (builder.ToString() == "!")
                {
                    return true;
                }
                else
                {
                    handler.Send(Encoding.Unicode.GetBytes("No"));//Send info that pass is incorrect
                    return false;
                }
        }
    }
}