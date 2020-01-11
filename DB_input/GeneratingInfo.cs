using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Structs;

namespace Client
{
    class GeneratingInfo
    {
        Model[] models;
        Airplane[] airplanes;
        Seat[] seats;
        Airport[] airports;
        Flight[] flights;
        FullRow[] information;

        public GeneratingInfo()
        {
            Refresh();
        }

        //public GeneratingInfo(int count_flight = 50, int count_airports = 10, int count_airplanes = 20, int count_models = 10)
        //{
        //    Refresh(count_flight, count_airports, count_airplanes, count_models);
        //}

        public void Refresh(int count_flights = 50, int count_airports = 10, int count_airplanes = 20, int count_models = 10)
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
                    if (models[j].name.Equals(name))
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
                code = city.Substring(0, 3).ToUpper();
                
                for (int j = 0; j < i ; j++)
                {
                    if (airports[j].code.Equals(code))
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

            flights = new Flight[count_flights];
            for(int i = 0; i < count_flights; i++)
            {
                int i_airplane = x.Next(count_airplanes);
                int i_airport_in = x.Next(count_airports);
                int i_airport_out = x.Next(count_airports);
                while (i_airport_in == i_airport_out)
                    i_airport_out = x.Next(count_airports);
                DateTime dateTime = DateTime.Today.AddYears(-3).AddDays(x.Next(365 * 3)).AddMinutes(x.Next(1440));
                string str_date_time = dateTime.ToString("yyyy-MM-dd HH:mm");           //("yyyy-MM-dd HH:mm:ss.fff");
                flights[i] = new Flight(i_airplane, i_airport_in, i_airport_out, str_date_time);
            }

            long count_seats = 0;
            for (int i = 0; i < count_airplanes; i++)
            {
                count_seats += airplanes[i].Capacity();
            }
            seats = new Seat[count_seats];
            long pos_seats = 0;
            for (int i = 0; i < airplanes.Length; i++)
            {
                int length = airplanes[i].length;
                int width = airplanes[i].width;
                for (int j = 0; j < length; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        string code_seat = (char)('A' + k) + (j + 1).ToString("D2");
                        seats[pos_seats] = new Seat(pos_seats, code_seat, i);
                        pos_seats++;
                    }
                }
            }
            
            int count_pass = 0;
            for (int i = 0; i < count_flights; i++)
            {
                count_pass += airplanes[flights[i].i_airplane].Capacity();
            }

            information = new FullRow[count_pass];
            int pos_info = 0;
            for (int i = 0; i < count_flights; i++)
            {
                //Для заполнения:
                string name_model = models[airplanes[flights[i].i_airplane].i_model].name;
                long number_airplane = flights[i].i_airplane;
                string code_seat = "";
                long id_seat = 0;
                string code_airport_in = airports[flights[i].i_airport_in].code;
                string city_airport_in = airports[flights[i].i_airport_in].city;
                string country_airport_in = airports[flights[i].i_airport_in].country;
                string code_airport_out = airports[flights[i].i_airport_out].code;
                string city_airport_out = airports[flights[i].i_airport_out].city;
                string country_airport_out = airports[flights[i].i_airport_out].country;
                long number_flight = i;
                string date_time_flight = flights[i].date_time;
                long number_passenger = 0;

                for (int j = 0; j < seats.Length; j++)
                {
                    if (flights[i].i_airplane == seats[j].i_airplane)
                    {
                        code_seat = seats[j].code;
                        id_seat = seats[j].id;
                        number_passenger = pos_info + 1;
                        information[pos_info++] = new FullRow(name_model, number_airplane, code_seat, id_seat,
                            code_airport_in, city_airport_in, country_airport_in,
                            code_airport_out, city_airport_out, country_airport_out,
                            number_flight, date_time_flight, number_passenger);
                    }
                }
            }
        }

        public FullRow[] GetInfo()
        {
            return information;
        }

        public string[] GetQueryInsert()
        {
            string[] queries = new string[information.Length];
            for (int i = 0; i < information.Length; i++)
            {
                queries[i] = @"INSERT INTO [main].[all] 
                            ([name_model], [number_airplane], [code_seat], [id_seat], [code_airport_in], [city_airport_in], [country_airport_in], [code_airport_out], [city_airport_out], [country_airport_out], [number_flight], [date_time_flight], [number_passenger]) 
                            VALUES ('" + information[i].name_model + "', '" + information[i].number_airplane + "', '" + information[i].code_seat + "', '" + information[i].id_seat +
                            "', '" + information[i].code_airport_in + "', '" + information[i].city_airport_in + "', '" + information[i].country_airport_in +
                            "', '" + information[i].code_airport_out + "', '" + information[i].city_airport_out + "', '" + information[i].country_airport_out +
                            "', '" + information[i].number_flight + "', '" + information[i].date_time_flight + "', '" + information[i].number_passenger + "');";
            }
            return queries;
        }
    }
}
