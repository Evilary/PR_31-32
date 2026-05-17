using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.IO;

namespace Snake_Чернышков
{
    class Program
    {
       
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int localPort = 5001;
        public static int MaxSpeed = 15;


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

        public static int AddSnake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();
            viewModelGamesPlayer.SnakesPlayers = new Snakes()
            {
                Points = new List<Snakes.Point>() {
            new Snakes.Point() { X = 30, Y = 10 },
            new Snakes.Point() { X = 20, Y = 10 },
            new Snakes.Point() { X = 10, Y = 10 },
        },
                direction = Snakes.Direction.Start
            };
            viewModelGamesPlayer.Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }

        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);

                List<ViewModelGames> RemoveSnakes = viewModelGames.FindAll(x => x.SnakesPlayers.GameOver);
                if (RemoveSnakes.Count > 0)
                {
                    foreach (ViewModelGames DeadSnake in RemoveSnakes)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Отключился пользователь: {remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).IPAddress}:{remoteIPAddress.Find(x => x.IdSnake == DeadSnake.IdSnake).Port}");
                        remoteIPAddress.RemoveAll(x => x.IdSnake == DeadSnake.IdSnake);
                    }
                    viewModelGames.RemoveAll(x => x.SnakesPlayers.GameOver);
                }

                foreach (ViewModelUserSettings User in remoteIPAddress)
                {
                    Snakes Snake = viewModelGames.Find(x => x.IdSnake == User.IdSnake).SnakesPlayers;
                    for (int i = Snake.Points.Count - 1; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            Snake.Points[i] = Snake.Points[i - 1];
                        }
                        else
                        {
                            int Speed = 10 + (int)(Snake.Points.Count / 10);
                            if (Speed > MaxSpeed) Speed = MaxSpeed;

                            if (Snake.direction == Snakes.Direction.Start)
                            {
                            }
                            else if (Snake.direction == Snakes.Direction.Down)
                            {
                                Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X, Y = Snake.Points[0].Y + Speed };
                            }
                            else if (Snake.direction == Snakes.Direction.Up)
                            {
                                Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X, Y = Snake.Points[0].Y - Speed };
                            }
                            else if (Snake.direction == Snakes.Direction.Right)
                            {
                                Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X + Speed, Y = Snake.Points[0].Y };
                            }
                            else if (Snake.direction == Snakes.Direction.Left)
                            {
                                Snake.Points[0] = new Snakes.Point() { X = Snake.Points[0].X - Speed, Y = Snake.Points[0].Y };
                            }
                        }
                    }

                    if (Snake.Points[0].X < 0 || Snake.Points[0].X > 783 || Snake.Points[0].Y < 0 || Snake.Points[0].Y > 410)
                    {
                        Snake.GameOver = true;
                    }

                    if (Snake.direction != Snakes.Direction.Start)
                    {
                        for (int i = 1; i < Snake.Points.Count; i++)
                        {
                            if (Snake.Points[0].X >= Snake.Points[i].X - 5 && Snake.Points[0].X <= Snake.Points[i].X + 5 && Snake.Points[0].Y >= Snake.Points[i].Y - 5 && Snake.Points[0].Y <= Snake.Points[i].Y + 5)
                            {
                                Snake.GameOver = true;
                                break;
                            }
                        }
                    }

                    if (Snake.Points[0].X >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X - 15 && Snake.Points[0].X <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.X + 15 && Snake.Points[0].Y >= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y - 15 && Snake.Points[0].Y <= viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points.Y + 15)
                    {
                        viewModelGames.Find(x => x.IdSnake == User.IdSnake).Points = new Snakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));

                        int X = Snake.Points[Snake.Points.Count - 1].X;
                        int Y = Snake.Points[Snake.Points.Count - 1].Y;
                        Snake.Points.Add(new Snakes.Point(X, Y));

                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = Snake.Points.Count
                        });

                        Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                        viewModelGames.Find(x => x.IdSnake == User.IdSnake).Top = Leaders.FindIndex(x => x.Points == Snake.Points.Count && x.Name == User.Name) + 1;
                    }

                    if (Snake.GameOver)
                    {
                        Leaders.Add(new Leaders()
                        {
                            Name = User.Name,
                            Points = Snake.Points.Count
                        });
                        SaveLeaders();
                    }
                }
                Send();
            }
        }

        public static void SaveLeaders()
        {
            string json = JsonConvert.SerializeObject(Leaders);
            StreamWriter SW = new StreamWriter("./leaders.txt");
            SW.WriteLine(json);
            SW.Close();
        }

        public static void LoadLeaders()
        {
            if (File.Exists("./leaders.txt"))
            {
                StreamReader SR = new StreamReader("./leaders.txt");
                string json = SR.ReadLine();
                SR.Close();
                if (!string.IsNullOrEmpty(json))
                {
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                }
                else
                {
                    Leaders = new List<Leaders>();
                }
            }
            else
            {
                Leaders = new List<Leaders>();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                Thread tTime = new Thread(Timer);
                tTime.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }







    }
}
