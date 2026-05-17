using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using Newtonsoft.Json;

namespace Snake_Чернышков
{
    class Program
    {
       
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int localPort = 5001;
        public static int MaxSpeed = 15;

        static void Main(string[] args)
        {

        }

        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                    IPAddress.Parse(User.IPAddress),
                    int.Parse(User.Port));
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelGames.Find(x => x.IdSnake == User.IdSnake)));

                    sender.Send(bytes, bytes.Length, endPoint);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отправил данные пользователю: {User.IPAddress}:{User.Port}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }
        }
        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(localPort);
            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                Console.WriteLine("Команды сервера:");
                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    string[] dataMessage = returnData.Split('|');
                    var viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получил команду: " + returnData);

                    if (dataMessage[0] == "/start")
                    {
                        Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress}:{viewModelUserSettings.Port}");
                        remoteIPAddress.Add(viewModelUserSettings);
                        viewModelUserSettings.IdSnake = AddSnake();
                        viewModelGames[viewModelUserSettings.IdSnake].IdSnake = viewModelUserSettings.IdSnake;
                    }
                    else
                    {
                        int IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress && x.Port == viewModelUserSettings.Port);
                        if (IdPlayer != -1)
                        {
                            switch (dataMessage[0])
                            {
                                case "Up": if (viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Down) viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Up; break;
                                case "Down": if (viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Up) viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Down; break;
                                case "Left": if (viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Right) viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Left; break;
                                case "Right": if (viewModelGames[IdPlayer].SnakesPlayers.direction != Snakes.Direction.Left) viewModelGames[IdPlayer].SnakesPlayers.direction = Snakes.Direction.Right; break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.Message);
            }
        }


    }
}
