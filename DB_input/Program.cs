using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Npgsql;
using Client.TableRow;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера

        static void Main(string[] args)
        {
            //Добавить обработку исключений при подключении к базе данных
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            //Console.WriteLine("Запись данных в БД SQLite");
            //WriteSQLite(factory);
            //Console.WriteLine("Нажмите любую клавишу для продолжения...");
            //Console.ReadKey(true);

            Console.WriteLine("Чтение данных из БД SQLite");
            List<FullRow> rows = ReadSQLite(factory);

            //string[] str_data = new string[rows.Count];
            //for (int i = 0; i < rows.Count; i++)
            //    str_data[i] = JsonConvert.SerializeObject(rows[i]);
            
            //str_data можно отправлять
            string str_data = JsonConvert.SerializeObject(rows);
            Console.WriteLine("Отправка данных");

            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);

                //Console.Write("Введите сообщение:");
                //string message = Console.ReadLine();
                //byte[] data = Encoding.Unicode.GetBytes(message);
                //socket.Send(data);

                //// получаем ответ
                //data = new byte[256]; // буфер для ответа
                //StringBuilder builder = new StringBuilder();
                //int bytes = 0; // количество полученных байт

                //do
                //{
                //    bytes = socket.Receive(data, data.Length, 0);
                //    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                //}
                //while (socket.Available > 0);
                //Console.WriteLine("ответ сервера: " + builder.ToString());

                //Сообщение ОЧЕНЬ большое
                byte[] data;
                data = Encoding.Unicode.GetBytes(str_data);
                socket.Send(data);

                //data = Encoding.Unicode.GetBytes("end");
                //socket.Send(data);

                // получаем ответ
                data = new byte[256]; // буфер для ответа
                string answer;
                int bytes = 0; // количество полученных байт

                bytes = socket.Receive(data, data.Length, 0);
                answer = Encoding.Unicode.GetString(data, 0, bytes);

                string[] res = answer.Split(' ');
                int count = 0;
                if (res.Length == 2)
                {
                    if (res[0].Equals("доставлено") && int.TryParse(res[1], out count))
                        Console.WriteLine("Отправлено {0} символов.\nДоставлено {1} символов", str_data.Length, count);
                    else
                        Console.WriteLine("Ответ не распознан");
                }
                else
                    Console.WriteLine("Некорректный ответ");

                // закрываем сокет
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
























            //Console.WriteLine("Конвертация строк");
            //List<CrossTableFullRow> tableRows = new List<CrossTableFullRow>();
            //foreach (FullRow row in rows)
            //{
            //    tableRows.Add(new ParserRow(row).GetCrossTableFullRow());
            //}

            //Console.WriteLine("Конвертация в таблицы");
            //ParserTable tables = new ParserTable(tableRows);

            //Console.WriteLine("Запись данных в БД PostgreSQL");
            //WritePostgreSQL(tables);

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для подтверждения...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        static public void WriteSQLite(SQLiteFactory factory)
        {
            string baseName = "inputdatabase.db3";

            int sucsess = 0;
            while (sucsess != 1)
            {
                Console.WriteLine("Создание нового файла БД SQLite");
                try
                {
                    SQLiteConnection.CreateFile(baseName);
                    sucsess = 1;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("этот файл используется другим процессом"))
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
                }
            }
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            @"CREATE TABLE [all] (
	                        [name_model]	        TEXT,
	                        [number_airplane]	    INTEGER,
	                        [code_seat]	            TEXT,
                            [id_seat]	            INTEGER,
	                        [code_airport_in]	    TEXT,
	                        [city_airport_in]	    TEXT,
	                        [country_airport_in]    TEXT,
	                        [code_airport_out]	    TEXT,
	                        [city_airport_out]	    TEXT,
	                        [country_airport_out]   TEXT,
	                        [number_flight]	        INTEGER,
	                        [date_time_flight]	    TEXT,
	                        [number_passenger]	    INTEGER
                        )";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                        Console.WriteLine("Таблица создана");
                    }

                    int count = 0;
                    try
                    {
                        Console.WriteLine("Генерация информации...");
                        GeneratingInfo info = new GeneratingInfo();
                        string[] queries = info.GetQueryInsert();
                        Console.WriteLine("Всего будет вставлено {0} записей", queries.Length);
                        int step = 50;
                        string line = new string('_', queries.Length / step + 1);
                        Console.WriteLine(line);
                        foreach (string query in queries)
                        {
                            using (SQLiteCommand command = new SQLiteCommand(connection))
                            {
                                command.CommandText = query;
                                command.CommandType = CommandType.Text;
                                command.ExecuteNonQuery();
                                count++;
                                if (count % step == 0)
                                {
                                    Console.Write("+");
                                }
                            }
                        }
                        Console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Ошибка при добавлении записей SQLite: " + e.Message);
                    }
                    finally
                    {
                        Console.WriteLine("Добавлено " + count + " записей SQLite");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при создании таблицы SQLite " + e.Message);
                }
                connection.Close();
            }
        }

        static public List<FullRow> ReadSQLite(SQLiteFactory factory)
        {
            List<FullRow> result = new List<FullRow>();
            string baseName = "inputdatabase.db3";

            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();
                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            "select name_model, number_airplane, code_seat, id_seat, " +
                            "code_airport_in, city_airport_in, country_airport_in, " +
                            "code_airport_out, city_airport_out, country_airport_out, " +
                            "number_flight, date_time_flight, number_passenger from [all];";
                        command.CommandType = CommandType.Text;
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                FullRow info = new FullRow(
                                reader.GetString(reader.GetOrdinal("name_model")),
                                reader.GetInt64(reader.GetOrdinal("number_airplane")),
                                reader.GetString(reader.GetOrdinal("code_seat")),
                                reader.GetInt64(reader.GetOrdinal("id_seat")),
                                reader.GetString(reader.GetOrdinal("code_airport_in")),
                                reader.GetString(reader.GetOrdinal("city_airport_in")),
                                reader.GetString(reader.GetOrdinal("country_airport_in")),
                                reader.GetString(reader.GetOrdinal("code_airport_out")),
                                reader.GetString(reader.GetOrdinal("city_airport_out")),
                                reader.GetString(reader.GetOrdinal("country_airport_out")),
                                reader.GetInt64(reader.GetOrdinal("number_flight")),
                                reader.GetString(reader.GetOrdinal("date_time_flight")),
                                reader.GetInt64(reader.GetOrdinal("number_passenger")));
                                result.Add(info);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при чтении SQLite: " + e.Message);
                }
                connection.Close();
            }
            return result;
        }

        //static public void WritePostgreSQL(ParserTable tables)
        //{
        //    //Удаление предыдущей базы данных (проще, чем чистить все данные)
        //    string connectionStringDefault = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=masterkey;Database=postgres;";
        //    string queryCreateDB =
        //        @"CREATE DATABASE outputdatabase
        //        WITH
        //        OWNER = postgres
        //        ENCODING = 'UTF8'
        //        LC_COLLATE = 'Russian_Russia.1251'
        //        LC_CTYPE = 'Russian_Russia.1251'
        //        TABLESPACE = pg_default
        //        CONNECTION LIMIT = -1;";
        //    int sucsess = 0;
        //    while (sucsess != 1)
        //    {
        //        Console.WriteLine("Подключение к базе PostgreSQL...");
        //        using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringDefault))
        //        using (NpgsqlCommand command = new NpgsqlCommand("DROP DATABASE outputdatabase;", connection))
        //        {
        //            try
        //            {
        //                connection.Open();

        //                command.ExecuteNonQuery();
        //                Console.WriteLine("Старые данные в базе PostgreSQL стерты");
        //                connection.Close();
        //                sucsess = 1;
        //            }
        //            catch (Exception ex)
        //            {
        //                connection.Close();
        //                if (ex.Data["SqlState"].Equals("55006"))
        //                {
        //                    sucsess--;
        //                    if (sucsess > -3)
        //                    {
        //                        Console.WriteLine("Пожалуйста, закройте базу данных и нажмите любую клавишу для подтверждения...");
        //                        Console.ReadKey(true);
        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("База данных открыта в данный момент. Приложение будет закрыто.");
        //                        Console.WriteLine("Нажмите любую клавишу для подтверждения...");
        //                        Console.ReadKey(true);
        //                        Environment.Exit(0);
        //                    }
        //                }
        //                else if (ex.Data["SqlState"].Equals("3D000"))
        //                {
        //                    //ничего, такой базы данных не существует. Этого я и добивалась
        //                    sucsess = 1;
        //                }
        //                else if (ex.Data["SqlState"].Equals("28P01"))
        //                {
        //                    Console.WriteLine("Неправильно указан логин или пароль. Приложение будет закрыто.");
        //                    Console.WriteLine("Нажмите любую клавишу для подтверждения...");
        //                    Console.ReadKey(true);
        //                    Environment.Exit(0);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("Произошла ошибка {0}. Приложение будет закрыто.", ex.Message);
        //                    Console.WriteLine("Нажмите любую клавишу для подтверждения...");
        //                    Console.ReadKey(true);
        //                    Environment.Exit(0);
        //                }
        //            }
        //        }
        //    }

        //    //Создание БД заново
        //    try
        //    {
        //        using (NpgsqlConnection connection = new NpgsqlConnection(connectionStringDefault))
        //        using (NpgsqlCommand command = new NpgsqlCommand(queryCreateDB, connection))
        //        {
        //            connection.Open();

        //            command.ExecuteNonQuery();

        //            connection.Close();

        //            Console.WriteLine("Создана новая БД PostgreSQL");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Ошибка при создании БД PostgreSQL " + ex.Message);
        //        Console.WriteLine("Приложение будет закрыто.");
        //        Console.WriteLine("Нажмите любую клавишу для подтверждения...");
        //        Console.ReadKey(true);
        //        Environment.Exit(0);
        //    }

        //    //Наполнение БД
        //    string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=masterkey;Database=outputdatabase;";
        //    try
        //    {
        //        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        //        {
        //            connection.Open();

        //            //Создание таблиц
        //            using (NpgsqlCommand command = new NpgsqlCommand(TableModelRow.GetCreateQuery(), connection))
        //            {
        //                try { command.ExecuteNonQuery(); }
        //                catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Model \n" + ex.Message); }
        //            }
        //            using (NpgsqlCommand command = new NpgsqlCommand(TableAirplaneRow.GetCreateQuery(), connection))
        //            {
        //                try { command.ExecuteNonQuery(); }
        //                catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Airplane \n" + ex.Message); }
        //            }
        //            using (NpgsqlCommand command = new NpgsqlCommand(TableAirportRow.GetCreateQuery(), connection))
        //            {
        //                try { command.ExecuteNonQuery(); }
        //                catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Airport \n" + ex.Message); }
        //            }
        //            using (NpgsqlCommand command = new NpgsqlCommand(TableSeatRow.GetCreateQuery(), connection))
        //            {
        //                try { command.ExecuteNonQuery(); }
        //                catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Seat \n" + ex.Message); }
        //            }
        //            using (NpgsqlCommand command = new NpgsqlCommand(TableFlightRow.GetCreateQuery(), connection))
        //            {
        //                try { command.ExecuteNonQuery(); }
        //                catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Flight \n" + ex.Message); }
        //            }
        //            using (NpgsqlCommand command = new NpgsqlCommand(TablePassengerRow.GetCreateQuery(), connection))
        //            {
        //                try { command.ExecuteNonQuery(); }
        //                catch (Exception ex) { Console.WriteLine("Ошибка при создании таблицы Passenger \n" + ex.Message); }
        //            }
        //            Console.WriteLine("Таблицы БД PostgreSQL успешно созданы");

        //            //Заполнение БД данными
        //            List<TableModelRow> tableModel = tables.tableModel;
        //            List<TableAirplaneRow> tableAirplane = tables.tableAirplane;
        //            List<TableSeatRow> tableSeat = tables.tableSeat;
        //            List<TableAirportRow> tableAirport = tables.tableAirport;
        //            List<TableFlightRow> tableFlight = tables.tableFlight;
        //            List<TablePassengerRow> tablePassenger = tables.tablePassenger;

        //            int count = 0;
        //            foreach (TableModelRow row in tableModel)
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
        //                {
        //                    try { command.ExecuteNonQuery(); count++; }
        //                    catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Model " + row.ToString() + "\n" + ex.Message); }
        //                }
        //            }
        //            Console.WriteLine("В таблицу Model добавлено {0} записей", count);

        //            count = 0;
        //            foreach (TableAirplaneRow row in tableAirplane)
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
        //                {
        //                    try { command.ExecuteNonQuery(); count++; }
        //                    catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Airplane " + row.ToString() + "\n" + ex.Message); }
        //                }
        //            }
        //            Console.WriteLine("В таблицу Airplane добавлено {0} записей", count);

        //            count = 0;
        //            foreach (TableSeatRow row in tableSeat)
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
        //                {
        //                    try { command.ExecuteNonQuery(); count++; }
        //                    catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Seat " + row.ToString() + "\n" + ex.Message); }
        //                }
        //            }
        //            Console.WriteLine("В таблицу Seat добавлено {0} записей", count);

        //            count = 0;
        //            foreach (TableAirportRow row in tableAirport)
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
        //                {
        //                    try { command.ExecuteNonQuery(); count++; }
        //                    catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Airport " + row.ToString() + "\n" + ex.Message); }
        //                }
        //            }
        //            Console.WriteLine("В таблицу Airport добавлено {0} записей", count);

        //            count = 0;
        //            foreach (TableFlightRow row in tableFlight)
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
        //                {
        //                    try { command.ExecuteNonQuery(); count++; }
        //                    catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Flight " + row.ToString() + "\n" + ex.Message); }
        //                }
        //            }
        //            Console.WriteLine("В таблицу Flight добавлено {0} записей", count);

        //            count = 0;
        //            foreach (TablePassengerRow row in tablePassenger)
        //            {
        //                using (NpgsqlCommand command = new NpgsqlCommand(row.GetSQL(), connection))
        //                {
        //                    try { command.ExecuteNonQuery(); count++; }
        //                    catch (Exception ex) { Console.WriteLine("Ошибка при вставке в Passenger " + row.ToString() + "\n" + ex.Message); }
        //                }
        //            }
        //            Console.WriteLine("В таблицу Passenger добавлено {0} записей", count);

        //            Console.WriteLine("Все данные добавлены");
        //            connection.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Ошибка при работе с БД PostgreSQL " + ex.Message);
        //        Console.WriteLine("Приложение будет закрыто.");
        //        Console.WriteLine("Нажмите любую клавишу для подтверждения...");
        //        Console.ReadKey(true);
        //        Environment.Exit(0);
        //    }
        //}

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
