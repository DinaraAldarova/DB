using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Client
{
    class Program
    {
        static int port = 8080;
        static string address = "192.168.100.8";

        static void Main(string[] args)
        {
            try
            {
                //Для подключения к SQLite, создается один раз
                SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");

                Console.WriteLine("Сгенерировать новую БД SQLite? (y/n)");
                ConsoleKeyInfo keyInfo = Console.ReadKey(false);
                if (keyInfo.KeyChar == 'y' || keyInfo.KeyChar == 'у' || keyInfo.KeyChar == 'д')
                {
                    //генерация новой БД
                    Console.WriteLine("Запись данных в БД SQLite");
                    WriteSQLite(factory);
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey(true);
                }

                //чтение БД
                Console.WriteLine("Чтение данных из БД SQLite");
                List<FullRow> rows = ReadSQLite(factory);
                string str_data = JsonConvert.SerializeObject(rows);

                // подключаемся к удаленному хосту
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                Console.WriteLine("Шифрование данных");

                // получаем публичный ключ RSA
                byte[] data_rsa = new byte[2048]; // буфер для ответа
                int bytes = socket.Receive(data_rsa, data_rsa.Length, 0);
                string serial_rsa = Encoding.Unicode.GetString(data_rsa, 0, bytes);
                RSAParameters rsaParams = JsonConvert.DeserializeObject<RSAParameters>(serial_rsa);

                //шифруем симметричным шифрованием данные
                Rijndael cipher = Rijndael.Create();
                string encrypted = Encrypt(str_data, cipher.Key, cipher.IV);
                //string decrypted = Decrypt(encrypted, cipher.Key, cipher.IV);

                //шифруем RSA ключ симметричного шифрования
                byte[] encrypted_key = RSAEncrypt(cipher.Key, rsaParams);
                //byte[] decrypted_key = RSAEncrypt(encrypted_key, RSA.ExportParameters(true));

                //удаляем из памяти экземпляр класса симметричного шифрования
                byte[] iv = cipher.IV;
                cipher.Dispose();

                Console.WriteLine("Пересылка данных");

                //отправляем зашифрованный ключ симметричного шифрования
                socket.Send(encrypted_key);
                
                // получаем подтверждение
                byte[] data = new byte[256]; // буфер для ответа
                bytes = socket.Receive(data, data.Length, 0);
                string answer = Encoding.Unicode.GetString(data, 0, bytes);
                if (!answer.Equals("ready"))
                    throw new Exception("Получены некорректные данные");

                //отправляем инициализующий вектор симметричного шифрования в открытом виде
                socket.Send(iv);

                //получаем подтверждение
                data = new byte[256]; // буфер для ответа
                bytes = socket.Receive(data, data.Length, 0);
                answer = Encoding.Unicode.GetString(data, 0, bytes);
                if (!answer.Equals("ready"))
                    throw new Exception("Получены некорректные данные");

                //отправляем размер данных
                byte[] data_rows = Encoding.Unicode.GetBytes(encrypted);
                string message = "отправка " + data_rows.Length;
                data = Encoding.Unicode.GetBytes(message);
                socket.Send(data);

                // получаем подтверждение
                data = new byte[256]; // буфер для ответа
                bytes = socket.Receive(data, data.Length, 0);
                answer = Encoding.Unicode.GetString(data, 0, bytes);
                if (!answer.Equals("ready"))
                    throw new Exception("Получены некорректные данные");

                //отправляем зашифрованные данные
                socket.Send(data_rows);

                // получаем количество доставленных байт
                data = new byte[256]; // буфер для ответа
                bytes = socket.Receive(data, data.Length, 0);
                answer = Encoding.Unicode.GetString(data, 0, bytes);

                //отображаем отчет
                string[] res = answer.Split(' ');
                if (res.Length != 2 || !res[0].Equals("доставлено"))
                    throw new Exception("Получены некорректные данные");
                int count = int.Parse(res[1]);
                Console.WriteLine("Отправлено {0} байт.\nДоставлено {1} байт", data_rows.Length, count);

                // закрываем сокет
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка {0}. Приложение будет закрыто.", ex.Message);
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
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