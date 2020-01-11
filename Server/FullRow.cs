using System;

namespace Server
{
    public class FullRow
    {
        public string name_model;
        public long number_airplane;
        public string code_seat;
        public long id_seat;
        public string code_airport_in;
        public string city_airport_in;
        public string country_airport_in;
        public string code_airport_out;
        public string city_airport_out;
        public string country_airport_out;
        public long number_flight;
        public string date_time_flight;
        public long number_passenger;

        public FullRow(string name_model, long number_airplane, string code_seat, long id_seat, string code_airport_in, string city_airport_in, string country_airport_in, string code_airport_out, string city_airport_out, string country_airport_out, long number_flight, string date_time_flight, long number_passenger)
        {
            this.name_model = name_model;
            this.number_airplane = number_airplane;
            this.code_seat = code_seat;
            this.id_seat = id_seat;
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

        public override string ToString()
        {
            return String.Format("{0, -10} {1, -10} {2, -10} {3, -10} {4, -10} {5, -10} " +
                "{6, -10} {7, -10} {8, -10} {9, -10} {10, -10} {11, -10}, {12, -10}",
                name_model, number_airplane, code_seat, id_seat,
                code_airport_in, city_airport_in, country_airport_in,
                country_airport_out, city_airport_out, country_airport_out,
                number_flight, date_time_flight, number_passenger);
        }
    }
}
