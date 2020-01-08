using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_input
{
    struct Model
    {
        public string name;

        public Model(string name)
        {
            this.name = name;
        }
    }
    
    struct Airplane
    {
        public int i_model;
        public Int64 id;
        public int length;
        public int width;

        public Airplane(int i_model, long id, int length, int width)
        {
            this.i_model = i_model;
            this.id = id;
            this.length = length;
            this.width = width;
        }

        public int Capacity()
        {
            return length * width;
        }
    }

    struct Airport
    {
        public string code;
        public string city;
        public string country;

        public Airport(string code, string city, string country)
        {
            this.code = code;
            this.city = city;
            this.country = country;
        }
    }

    struct Flight
    {
        public int i_airplane;
        public int i_airport_in;
        public int i_airport_out;
        public string date_time;

        public Flight(int i_airplane, int i_airport_in, int i_airport_out, string date_time)
        {
            this.i_airplane = i_airplane;
            this.i_airport_in = i_airport_in;
            this.i_airport_out = i_airport_out;
            this.date_time = date_time;
        }
    }

    struct AllInfo
    {
        public string name_model;
        public Int64 number_airplane;
        public string code_seat;
        public string code_airport_in;
        public string city_airport_in;
        public string country_airport_in;
        public string code_airport_out;
        public string city_airport_out;
        public string country_airport_out;
        public Int64 number_flight;
        public string date_time_flight;
        public Int64 number_passenger;

        public AllInfo(string name_model, long number_airplane, string code_seat, string code_airport_in, string city_airport_in, string country_airport_in, string code_airport_out, string city_airport_out, string country_airport_out, long number_flight, string date_time_flight, long number_passenger)
        {
            this.name_model = name_model;
            this.number_airplane = number_airplane;
            this.code_seat = code_seat;
            this.code_airport_in = code_airport_in;
            this.city_airport_in = city_airport_in;
            this.country_airport_in = country_airport_in;
            this.code_airport_out = code_airport_out;
            this.city_airport_out = city_airport_out;
            this.country_airport_out = country_airport_out;
            this.number_flight = number_flight;
            this.date_time_flight = date_time_flight;
            this.number_passenger = number_passenger;
        }
    }

    class GeneratingInfo
    {
        Model[] models;
        Airplane[] airplanes;
        Airport[] airports;
        Flight[] flights;
        AllInfo[] information;

        public GeneratingInfo()
        {
            Refresh();
        }

        public GeneratingInfo(int count_flight = 100, int count_airports = 10, int count_airplanes = 20, int count_models = 10)
        {
            Refresh(count_flight, count_airports, count_airplanes, count_models);
        }

        public void Refresh(int count_flight = 100, int count_airports = 10, int count_airplanes = 20, int count_models = 10)
        {
            Random x = new Random();
            models = new Model[count_models];
            for(int i = 0; i < count_models; i++)
            {
                string name = "";
                switch (x.Next(3))
                {
                    case 0:
                        name = "Airbus" + " " + (char)('A' + x.Next(10)) + (x.Next(100, 1000) / 10);
                        break;
                    case 1:
                        name = "SSJ" + "-" + x.Next(100, 1000);
                        break;
                    case 2:
                        name = "Boeng" + "-" + x.Next(100, 1000);
                        break;
                    default:
                        throw new Exception("Генератор-то выдал и верхний предел тоже");
                }
                for (int j = 0; j < i; j++)
                {
                    if (models[j].name == name)
                    {
                        //Если такое название модели уже было, то в конец добавить букву
                        if (name[name.Length - 1] >= '0' && name[name.Length - 1] <= '9' || name[name.Length - 1] == 'Z')
                            name = name + (char)('A' + x.Next(26));
                        else
                            name = name.Substring(0, name.Length - 1) + (char)(name[name.Length - 1] + 1);
                    }
                }
                models[i] = new Model(name);
            }
                        
            airplanes = new Airplane[count_airplanes];
            for(int i = 0; i < count_airplanes; i++)
            {
                airplanes[i] = new Airplane(x.Next(count_models), i, x.Next(10, 20), x.Next(3, 5));
            }

            airports = new Airport[count_airports];
            for(int i = 0; i < count_airports; i++)
            {
                string country = "";
                string city = "";
                string code = "";
                switch (x.Next(20))
                {
                    case 0:
                    case 1:
                        city = "Moskow";
                        country = "Russia";
                        break;
                    case 2:
                    case 3:
                        city = "Peterburg";
                        country = "Russia";
                        break;
                    case 4:
                        city = "Murmansk";
                        country = "Russia";
                        break;
                    case 5:
                    case 6:
                        city = "Stockholm";
                        country = "Sweden";
                        break;
                    case 7:
                        city = "Gothenburg";
                        country = "Sweden";
                        break;
                    case 8:
                    case 9:
                        city = "Oslo";
                        country = "Norway";
                        break;
                    case 10:
                        city = "Bergen";
                        country = "Norway";
                        break;
                    case 11:
                    case 12:
                        city = "Helsinki";
                        country = "Finland";
                        break;
                    case 13:
                        city = "Turku";
                        country = "Finland";
                        break;
                    case 14:
                        city = "Tampere";
                        country = "Finland";
                        break;
                    case 15:
                        city = "Oulu";
                        country = "Finland";
                        break;
                    case 16:
                    case 17:
                        city = "Tallin";
                        country = "Estonia";
                        break;
                    case 18:
                    case 19:
                        city = "Riga";
                        country = "Latvia";
                        break;
                    default:
                        break;
                }
                code = city.Substring(0, 3);
                
                for (int j = 0; j < i ; j++)
                {
                    if (airports[j].code == code)
                    {
                        if (code.Length == 3)
                        {
                            code = code + 'A';
                        }
                        else
                        {
                            code = code.Substring(0, 3) + (char)(code[3] + 1);
                        }
                    }
                }
                airports[i] = new Airport(code, city, country);
            }

            flights = new Flight[count_flight];
            for(int i = 0; i < count_flight; i++)
            {
                int i_airplane = x.Next(count_airplanes);
                int i_airport_in = x.Next(count_airports);
                int i_airport_out = x.Next(count_airports);
                while (i_airport_in == i_airport_out)
                    i_airport_out = x.Next(count_airports);
                DateTime dateTime = DateTime.Today;
                dateTime.AddYears(-3);
                dateTime.AddDays(x.Next(365 * 3));
                dateTime.AddMinutes(x.Next(1440));
                string str_date_time = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                flights[i] = new Flight(i_airplane, i_airport_in, i_airport_out, str_date_time);
            }

            int count_info = 0;
            for (int i = 0; i < count_flight; i++)
            {
                count_info += airplanes[flights[i].i_airplane].Capacity();
            }

            information = new AllInfo[count_info];
            int pos = 0;
            for (int i = 0; i < count_flight; i++)
            {
                int length = airplanes[flights[i].i_airplane].length;
                int width  = airplanes[flights[i].i_airplane].width;

                //Для заполнения:
                string name_model = models[airplanes[flights[i].i_airplane].i_model].name;
                Int64 number_airplane = flights[i].i_airplane;
                string code_seat = "";
                string code_airport_in = airports[flights[i].i_airport_in].code;
                string city_airport_in = airports[flights[i].i_airport_in].city;
                string country_airport_in = airports[flights[i].i_airport_in].country;
                string code_airport_out = airports[flights[i].i_airport_out].code;
                string city_airport_out = airports[flights[i].i_airport_out].city;
                string country_airport_out = airports[flights[i].i_airport_out].country;
                Int64 number_flight = i;
                string date_time_flight = flights[i].date_time;
                Int64 number_passenger = 0;


                for (int j = 0; j < length; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        code_seat = (char)('A' + k) + j.ToString("D2");
                        number_passenger = pos + 1;
                        information[pos++] = new AllInfo(name_model, number_airplane, code_seat,
                            code_airport_in, city_airport_in, country_airport_in,
                            country_airport_out, city_airport_out, country_airport_out,
                            number_flight, date_time_flight, number_passenger);
                    }
                }
            }
        }

        public AllInfo[] GetInfo()
        {
            return information;
        }

        public string[] GetQueryInsert()
        {
            string[] queries = new string[information.Length];
            for (int i = 0; i < information.Length; i++)
            {
                queries[i] = @"INSERT INTO [main].[all] 
                            ([name_model], [number_airplane], [code_seat], [code_airport_in], [city_airport_in], [country_airport_in], [code_airport_out], [city_airport_out], [country_airport_out], [number_fligft], [date_time_flight], [number_passenger]) 
                            VALUES ('" + information[i].name_model + "', '" + information[i].number_airplane + "', '" + information[i].code_seat +
                            "', '" + information[i].code_airport_in + "', '" + information[i].city_airport_in + "', '" + information[i].country_airport_in +
                            "', '" + information[i].code_airport_out + "', '" + information[i].city_airport_out + "', '" + information[i].country_airport_out +
                            "', '" + information[i].number_flight + "', '" + information[i].date_time_flight + "', '" + information[i].number_passenger + "');";
            }
            return queries;
        }
    }
}
