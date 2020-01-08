using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace DB_input
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseName = "inputdatabase.db3";

            //SQLiteConnection.CreateFile(baseName);
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");

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
	                        [name_model]	        INTEGER,
	                        [number_airplane]	    INTEGER,
	                        [code_seat]	            TEXT,
	                        [code_airport_in]	    TEXT,
	                        [city_airport_in]	    TEXT,
	                        [country_airport_in]    TEXT,
	                        [code_airport_out]	    TEXT,
	                        [city_airport_out]	    TEXT,
	                        [country_airport_out]   TEXT,
	                        [number_fligft]	        INTEGER,
	                        [date_time_flight]	    TEXT,
	                        [number_passenger]	    INTEGER
                        )";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                        Console.WriteLine("Таблица создана");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при создании таблицы " + e.Message);
                }
                int count = 0;
                try
                {
                    GeneratingInfo info = new GeneratingInfo();
                    string[] queries = info.GetQueryInsert();
                    foreach (string query in queries)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = query;
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                            count++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при добавлении записей " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Добавлено " + count + " записей");
                }
            }
            
            Console.ReadLine();
        }
    }
}
