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
using System.Security.Cryptography;

namespace Server
{
    class Program
    {
        static int port = 8080; // порт для приема входящих запросов
        static string address = "192.168.100.8"; // адрес сервера

        static void Main(string[] args)
        {
            //Код подключения по сокетам взят с сайта
            //https://metanit.com/sharp/net/3.2.php

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // связываем сокет с локальной точкой, по которой будем принимать данные
                listenSocket.Bind(ipPoint);

                // начинаем прослушивание
                listenSocket.Listen(10);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    //подключаем нового пользователя
                    Socket handler = listenSocket.Accept();
                    try
                    {
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;          // количество полученных байт
                        int bytes_total = 0;    // количество ожидаемых в сумме байт
                        int bytes_received = 0; // количество полученных в сумме байт
                        byte[] data = new byte[2048]; // буфер для получаемых данных
                        handler.ReceiveTimeout = 2000;

                        //Create a new RSACryptoServiceProvider object.
                        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);

                        //Export the key information to an RSAParameters object.
                        //Pass false to export the public key information or pass
                        //true to export public and private key information.
                        RSAParameters RSAParams = RSA.ExportParameters(false);
                        string rsaKey = JsonConvert.SerializeObject(RSAParams);

                        //отправляем публичный ключ RSA
                        data = Encoding.Unicode.GetBytes(rsaKey);
                        handler.Send(data);

                        //получаем ключ симметричного шифрования, зашифрованный RSA
                        byte[] encrypted_key = new byte[256];
                        bytes = handler.Receive(encrypted_key);

                        //дешифруем ключ симметричного шифрования приватным ключом RSA
                        //byte[] encrypted_key = RSAEncrypt(key_simm, RSA.ExportParameters(false));
                        byte[] decrypted_key = RSADecrypt(encrypted_key, RSA.ExportParameters(true));
                        
                        //удаляем из памяти экземпляр класса асиметричного шифрования
                        RSA.Dispose();

                        //подтверждаем готовность
                        data = Encoding.Unicode.GetBytes("ready");
                        handler.Send(data);

                        //получаем инициализующий вектор шифрования
                        byte[] iv = new byte[16];
                        bytes = handler.Receive(iv);

                        //подтверждаем готовность
                        data = Encoding.Unicode.GetBytes("ready");
                        handler.Send(data);

                        //получаем длину сообщения
                        data = new byte[2048];
                        bytes = handler.Receive(data);
                        string[] res = Encoding.Unicode.GetString(data, 0, bytes).Split(' ');
                        if (res.Length != 2 || !res[0].Equals("отправка"))
                            throw new Exception("Получены некорректные данные");
                        bytes_total = int.Parse(res[1]);

                        //подтверждаем готовность
                        data = Encoding.Unicode.GetBytes("ready");
                        handler.Send(data);

                        // получаем сообщение
                        data = new byte[2048];
                        do
                        {
                            bytes = handler.Receive(data);
                            bytes_received += bytes;
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (bytes_received < bytes_total);

                        // отправляем количество полученных байт
                        string message = "доставлено " + bytes_received;
                        data = Encoding.Unicode.GetBytes(message);
                        handler.Send(data);

                        // закрываем сокет
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();

                        //дешифруем
                        //string encrypted = Encrypt(str_data, cipher.Key, cipher.IV);
                        string decrypted = Decrypt(builder.ToString(), decrypted_key, iv);

                        //Код работы с JSON взят с 
                        //https://котодомик.рф/2015/02/18/json_csharp/
                        //но лучше найти офф. документацию

                        List<FullRow> rows = JsonConvert.DeserializeObject<List<FullRow>>(decrypted);
                        Console.WriteLine("Конвертация строк");
                        List<CrossTableFullRow> tableRows = new List<CrossTableFullRow>();
                        foreach (FullRow row in rows)
                        {
                            tableRows.Add(new ParserRow(row).GetCrossTableFullRow());
                        }
                        ParserTable tables = new ParserTable(tableRows);

                        Console.WriteLine("Запись данных в БД PostgreSQL");
                        WritePostgreSQL(tables);
                    }
                    finally
                    {
                        try
                        {
                            // закрываем сокет еще раз, вдруг он еще открыт
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка {0}. Приложение будет закрыто.", ex.Message);
                Console.WriteLine("Нажмите любую клавишу для подтверждения...");
                Console.ReadKey(true);
                Environment.Exit(0);
            }

            //Console.WriteLine();
            //Console.WriteLine("Нажмите любую клавишу для подтверждения...");
            //Console.ReadKey(true);
            //Environment.Exit(0);
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

                    Console.WriteLine("Все данные добавлены\n");
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

        static public string Encrypt(string text, byte[] key, byte[] iv)
        {
            //Код взят со страницы
            //https://habr.com/ru/post/254909/
            //и офф. документации
            //https://docs.microsoft.com/ru-ru/dotnet/api/system.security.cryptography.rijndael?view=netcore-2.1

            Rijndael cipher = Rijndael.Create();
            cipher.Key = key;
            cipher.IV = iv;

            ICryptoTransform t = cipher.CreateEncryptor();
            //string text = "some_text_to_encrypt";
            byte[] textInBytes = Encoding.Unicode.GetBytes(text);
            byte[] result = t.TransformFinalBlock(textInBytes, 0, textInBytes.Length);
            return Convert.ToBase64String(result);
        }

        static public string Decrypt(string text, byte[] key, byte[] iv)
        {
            //Код взят со страницы
            //https://habr.com/ru/post/254909/
            //и офф. документации
            //https://docs.microsoft.com/ru-ru/dotnet/api/system.security.cryptography.rijndael?view=netcore-2.1

            Rijndael cipher = Rijndael.Create();
            cipher.Key = key;
            cipher.IV = iv;

            ICryptoTransform t = cipher.CreateDecryptor();
            //string text = "some_text_to_encrypt";
            byte[] textInBytes = Convert.FromBase64String(text);
            byte[] result = t.TransformFinalBlock(textInBytes, 0, textInBytes.Length);
            return Encoding.Unicode.GetString(result);
        }

        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding = false)
        {
            //from
            //https://docs.microsoft.com/ru-ru/dotnet/api/system.security.cryptography.rsacryptoserviceprovider?view=netcore-3.1

            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding = false)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }
    }
}
