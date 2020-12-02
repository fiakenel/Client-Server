using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static int port = 8005;
        static string address = "127.0.0.1";

        static void Main(string[] args)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                byte[] data;
                StringBuilder builder = new StringBuilder();
                int bytes = 0;

                do
                {
                    data = new byte[256];
                    bytes = socket.Receive(data, data.Length, 0); //Receive size of password
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (socket.Available > 0);

                int passSize = int.Parse(builder.ToString());
                builder.Clear();

                Console.WriteLine("Довжина пароля: " + passSize);
                Console.Write("Введіть 1 якщо бажаєте спробувати вгадати пароль самостійно" +
                              " або 2 для автоматичного підбору: ");
                int.TryParse(Console.ReadLine(), out int answer);

                if (answer == 1)
                {
                    socket.Send(Encoding.Unicode.GetBytes("user"));//Send type of guessing
                    if (UserGuessing(socket))
                        return;
                }
                else if (answer == 2)
                {
                    socket.Send(Encoding.Unicode.GetBytes("auto"));//Send type of guessing
                    AutoGuessing(socket, passSize);
                    do
                    {
                        data = new byte[256];
                        bytes = socket.Receive(data, data.Length, 0); //Receive number of attempts
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (socket.Available > 0);
                    
                    Console.WriteLine("Кількість спроб: " + builder);
                    builder.Clear();
                }
                else
                {
                    throw new Exception("Невідома команда");
                }
                

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void AutoGuessing(Socket socket, int size)
        {
            Console.Write("Введіть час (в секундах) скільки буде тривати підбір: ");
            int.TryParse(Console.ReadLine(), out int sec);
            //var finish = DateTime.Now.Ticks + (long)sec * 10000000;
            socket.Send(Encoding.Unicode.GetBytes(sec.ToString()));
            
            for(long i = 0 ;/*DateTime.Now.Ticks < finish*/; i++)
            {
                string guess = Generate(i);
                while (guess.Length < size)
                {
                    guess = "0" + guess;
                }
                socket.Send(Encoding.Unicode.GetBytes(guess));//Send auto guess
                
                Console.WriteLine($"Спроба {i+1}: " + guess);
                var builder = new StringBuilder();
                do
                {
                    var data = new byte[256];
                    var bytes = socket.Receive(data, data.Length, 0); //Receive info about pass
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (socket.Available > 0);
                
                if (builder.ToString() == "Yes")
                {
                    Console.WriteLine("Пароль вгадано!");
                    return;
                }
                else if(builder.ToString() == "!")
                {
                    Console.WriteLine("Час вичерпано.");
                    return;
                }
            }
        }

        static string Generate(long num)
        {
            string res = "";
            
            if (num == 0)
            {
                res = "0";
            }
            while (num > 0)
            {
                int i = (int)(num < 62 ? num : num % 62);
                if (i < 10)
                {
                    res = i + res;
                }
                else if (i < 36)
                {
                    res = ((char)(i + 55)) + res;
                }
                else
                {
                    res = ((char)(i + 61)) + res;
                }

                num = num / 62;
            }

            

            return res;
        }
        static bool UserGuessing(Socket socket)
        {
            for (int i = 1; i > 0; i++)
            {
                Console.Write($"Спроба {i}:");
                string guess = Console.ReadLine();

                socket.Send(Encoding.Unicode.GetBytes(guess)); //Send user guess

                var builder = new StringBuilder();
                do
                {
                    var data = new byte[256];
                    var bytes = socket.Receive(data, data.Length, 0); //Receive info about pass
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (socket.Available > 0);

                if (builder.ToString() == "Yes")
                {
                    Console.WriteLine("Пароль вгадано!");
                    return true;
                }
                else
                    Console.WriteLine("Пароль не вгадано");

            }

            return false;
        }
    }
}