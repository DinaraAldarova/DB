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

namespace DB_input
{
    class Program
    {
        static void Main(string[] args)
        {
            //Добавить обработку исключений при подключении к базе данных

            //WriteSQLite();
            //Console.ReadLine();

            //List<object[]> res = ReadSQLite();
            //foreach (object[] item in res)
            //{
            //    foreach (object i in item)
            //    {
            //        Console.Write("{0, -10}  ", i.ToString());
            //    }
            //    Console.WriteLine();
            //}
            //Console.ReadLine();

            WritePostgreSQL();
            Console.ReadLine();
        }

        static public void WriteSQLite()
        {
            string baseName = "inputdatabase.db3";

            SQLiteConnection.CreateFile(baseName);
            using (SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite"))
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
            }
        }

        static public List<object[]> ReadSQLite()
        {
            List<object[]> result = new List<object[]>();
            string baseName = "inputdatabase.db3";

            using (SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite"))
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                try
                {
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            "select name_model, number_airplane, code_seat, " +
                            "code_airport_in, city_airport_in, country_airport_in, " +
                            "code_airport_out, city_airport_out, country_airport_out, " +
                            "number_flight, date_time_flight, number_passenger from [all];";
                        command.CommandType = CommandType.Text;
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                object[] col = new object[12];

                                col[0] = reader["name_model"];
                                col[1] = reader["number_airplane"];
                                col[2] = reader["code_seat"];
                                col[3] = reader["code_airport_in"];
                                col[4] = reader["city_airport_in"];
                                col[5] = reader["country_airport_in"];
                                col[6] = reader["code_airport_out"];
                                col[7] = reader["city_airport_out"];
                                col[8] = reader["country_airport_out"];
                                col[9] = reader["number_flight"];
                                col[10] = reader.GetDateTime(reader.GetOrdinal("date_time_flight"));
                                col[11] = reader["number_passenger"];

                                result.Add(col);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при чтении SQLite: " + e.Message);
                }
            }
            return result;
        }

        static public void WritePostgreSQL()
        {
            string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=masterkey;Database=outputdatabase;";
            string query = "SELECT * FROM public.model;";
            
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                connection.Open();
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    object item = reader[0];
                    Console.WriteLine(item.ToString());
                }
            }
        }
    }
}
