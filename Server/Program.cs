using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Npgsql;
using Server.TableRow;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static int port = 8005; // порт для приема входящих запросов

        static void Main(string[] args)
        {
            //Код подключения по сокетам взят с сайта
            //https://metanit.com/sharp/net/3.2.php

            // получаем адреса для запуска сокета
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            //List<string> str_data = new List<string>();
            StringBuilder builder = new StringBuilder();
            
            // создаем сокет
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                //while (true)
                //{
                //    Socket handler = listenSocket.Accept();
                //    // получаем сообщение
                //    StringBuilder builder = new StringBuilder();
                //    int bytes = 0; // количество полученных байтов
                //    byte[] data = new byte[256]; // буфер для получаемых данных

                //    do
                //    {
                //        bytes = handler.Receive(data);
                //        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                //    }
                //    while (handler.Available > 0);

                //    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                //    // отправляем ответ
                //    string message = "ваше сообщение доставлено";
                //    data = Encoding.Unicode.GetBytes(message);
                //    handler.Send(data);
                //    // закрываем сокет
                //    handler.Shutdown(SocketShutdown.Both);
                //    handler.Close();
                //}


                Socket handler = listenSocket.Accept();
                // получаем сообщение
                //StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байтов
                byte[] data = new byte[2048]; // буфер для получаемых данных
                string str;
                
                do
                {
                    bytes = handler.Receive(data);
                    str = Encoding.Unicode.GetString(data, 0, bytes);
                    //str_data.Add(str);
                    builder.Append(str);
                }
                //while (!str.Equals("end"));
                while (handler.Available > 0);

                //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                // отправляем ответ
                string message = "доставлено " + builder.Length;
                data = Encoding.Unicode.GetBytes(message);
                handler.Send(data);
                // закрываем сокет
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка {0}. Приложение будет закрыто.", ex.Message);
                Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }












            //Код работы с JSON взят с 
            //https://котодомик.рф/2015/02/18/json_csharp/
            //но лучше найти офф. документацию

            //сюда получить данные
            List<FullRow> rows = new List<FullRow>();
            rows = JsonConvert.DeserializeObject<List<FullRow>>(builder.ToString());

            Console.WriteLine("Конвертация строк");
            List<CrossTableFullRow> tableRows = new List<CrossTableFullRow>();
            foreach (FullRow row in rows)
            {
                tableRows.Add(new ParserRow(row).GetCrossTableFullRow());
            }

            Console.WriteLine("Конвертация в таблицы");
            ParserTable tables = new ParserTable(tableRows);

            Console.WriteLine("Запись данных в БД PostgreSQL");
            WritePostgreSQL(tables);

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для подтверждения...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        static public void WritePostgreSQL(ParserTable tables)
        {
            //Удаление предыдущей базы данных (проще, чем чистить все данные)
            string connectionStringDefault = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=masterkey;Database=postgres;";
            string queryCreateDB =
                @"CREATE DATABASE outputdatabase
                WITH
                OWNER = postgres
                ENCODING = 'UTF8'
                LC_COLLATE = 'Russian_Russia.1251'
                LC_CTYPE = 'Russian_Russia.1251'
                TABLESPACE = pg_default
                CONNECTION LIMIT = -1;";
            int sucsess = 0;
            while (sucsess != 1)
            {
                Console.WriteLine("Подключение к базе PostgreSQL...");
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringDefault))
                using (NpgsqlCommand command = new NpgsqlCommand("DROP DATABASE outputdatabase;", connection))
                {
                    try
                    {
                        connection.Open();

                        command.ExecuteNonQuery();
                        Console.WriteLine("Старые данные в базе PostgreSQL стерты");
                        connection.Close();
                        sucsess = 1;
                    }
                    catch (Exception ex)
                    {
                        connection.Close();
                        if (ex.Data["SqlState"].Equals("55006"))
                        {
                            sucsess--;
                            if (sucsess > -3)
                            {
                                Console.WriteLine("Пожалуйста, закройте базу данных и нажмите любую клавишу для подтверждения...");
                                Console.ReadKey(true);
                            }
                            else
                            {
                                Console.WriteLine("База данных открыта в данный момент. Приложение будет закрыто.");
                                Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                                Console.ReadKey(true);
                                Environment.Exit(0);
                            }
                        }
                        else if (ex.Data["SqlState"].Equals("3D000"))
                        {
                            //ничего, такой базы данных не существует. Этого я и добивалась
                            sucsess = 1;
                        }
                        else if (ex.Data["SqlState"].Equals("28P01"))
                        {
                            Console.WriteLine("Неправильно указан логин или пароль. Приложение будет закрыто.");
                            Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                            Console.ReadKey(true);
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.WriteLine("Произошла ошибка {0}. Приложение будет закрыто.", ex.Message);
                            Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                            Console.ReadKey(true);
                            Environment.Exit(0);
                        }
                    }
                }
            }

            //Создание БД заново
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringDefault))
                using (NpgsqlCommand command = new NpgsqlCommand(queryCreateDB, connection))
                {
                    connection.Open();

                    command.ExecuteNonQuery();

                    connection.Close();

                    Console.WriteLine("Создана новая БД PostgreSQL");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при создании БД PostgreSQL " + ex.Message);
                Console.WriteLine("Приложение будет закрыто.");
                Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            //Наполнение БД
            string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=masterkey;Database=outputdatabase;";
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    //Создание таблиц
                    using (NpgsqlCommand command = new NpgsqlCommand(TableModelRow.GetCreateQuery(), connection))
                    {
                        try { command.ExecuteNonQuery(); }
                        catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Model \n" + ex.Message); }
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(TableAirplaneRow.GetCreateQuery(), connection))
                    {
                        try { command.ExecuteNonQuery(); }
                        catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Airplane \n" + ex.Message); }
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(TableAirportRow.GetCreateQuery(), connection))
                    {
                        try { command.ExecuteNonQuery(); }
                        catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Airport \n" + ex.Message); }
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(TableSeatRow.GetCreateQuery(), connection))
                    {
                        try { command.ExecuteNonQuery(); }
                        catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Seat \n" + ex.Message); }
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(TableFlightRow.GetCreateQuery(), connection))
                    {
                        try { command.ExecuteNonQuery(); }
                        catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Flight \n" + ex.Message); }
                    }
                    using (NpgsqlCommand command = new NpgsqlCommand(TablePassengerRow.GetCreateQuery(), connection))
                    {
                        try { command.ExecuteNonQuery(); }
                        catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Passenger \n" + ex.Message); }
                    }
                    Console.WriteLine("Таблицы БД PostgreSQL успешно созданы");

                    //Заполнение БД данными
                    List<TableModelRow> tableModel = tables.tableModel;
                    List<TableAirplaneRow> tableAirplane = tables.tableAirplane;
                    List<TableSeatRow> tableSeat = tables.tableSeat;
                    List<TableAirportRow> tableAirport = tables.tableAirport;
                    List<TableFlightRow> tableFlight = tables.tableFlight;
                    List<TablePassengerRow> tablePassenger = tables.tablePassenger;

                    int count = 0;
                    foreach (TableModelRow row in tableModel)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
                        {
                            try { command.ExecuteNonQuery(); count++; }
                            catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Model " + row.ToString() + "\n" + ex.Message); }
                        }
                    }
                    Console.WriteLine("В таблицу Model добавлено {0} записей", count);

                    count = 0;
                    foreach (TableAirplaneRow row in tableAirplane)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
                        {
                            try { command.ExecuteNonQuery(); count++; }
                            catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Airplane " + row.ToString() + "\n" + ex.Message); }
                        }
                    }
                    Console.WriteLine("В таблицу Airplane добавлено {0} записей", count);

                    count = 0;
                    foreach (TableSeatRow row in tableSeat)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
                        {
                            try { command.ExecuteNonQuery(); count++; }
                            catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Seat " + row.ToString() + "\n" + ex.Message); }
                        }
                    }
                    Console.WriteLine("В таблицу Seat добавлено {0} записей", count);

                    count = 0;
                    foreach (TableAirportRow row in tableAirport)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
                        {
                            try { command.ExecuteNonQuery(); count++; }
                            catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Airport " + row.ToString() + "\n" + ex.Message); }
                        }
                    }
                    Console.WriteLine("В таблицу Airport добавлено {0} записей", count);

                    count = 0;
                    foreach (TableFlightRow row in tableFlight)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
                        {
                            try { command.ExecuteNonQuery(); count++; }
                            catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Flight " + row.ToString() + "\n" + ex.Message); }
                        }
                    }
                    Console.WriteLine("В таблицу Flight добавлено {0} записей", count);

                    count = 0;
                    foreach (TablePassengerRow row in tablePassenger)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
                        {
                            try { command.ExecuteNonQuery(); count++; }
                            catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Passenger " + row.ToString() + "\n" + ex.Message); }
                        }
                    }
                    Console.WriteLine("В таблицу Passenger добавлено {0} записей", count);

                    Console.WriteLine("Все данные добавлены");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при работе с БД PostgreSQL " + ex.Message);
                Console.WriteLine("Приложение будет закрыто.");
                Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }
        }

        //static public void ReadPostgreSQL()
        //{
        //    //string query = "SELECT * FROM public.model;";

        //    //using (NpgsqlDataReader reader = command.ExecuteReader())
        //    //{
        //    //    reader.Read();
        //    //    object item = reader[0];
        //    //    Console.WriteLine(item.ToString());
        //    //}
        //}
    }
}
