using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.TableRow;

namespace Server
{
    public class ParserRow
    {
        FullRow fullRow;

        CrossTableFullRow tableRows;

        public ParserRow(string name_model, long number_airplane, string code_seat, long id_seat,
            string code_airport_in, string city_airport_in, string country_airport_in,
            string code_airport_out, string city_airport_out, string country_airport_out,
            long number_flight, string date_time_flight, long number_passenger)
        {
            fullRow = new FullRow(
                name_model,
                number_airplane,
                code_seat,
                id_seat,
                code_airport_in,
                city_airport_in,
                country_airport_in,
                code_airport_out,
                city_airport_out,
                country_airport_out,
                number_flight,
                date_time_flight,
                number_passenger);

            tableRows = new CrossTableFullRow(
                new TableModelRow(name_model),
                new TableAirplaneRow(number_airplane, name_model),
                new TableSeatRow(id_seat, code_seat, number_airplane),
                new TableAirportRow(code_airport_in, city_airport_in, country_airport_in),
                new TableAirportRow(code_airport_out, city_airport_out, country_airport_out),
                new TableFlightRow(number_flight, date_time_flight, number_airplane, code_airport_in, code_airport_out),
                new TablePassengerRow(number_passenger, number_flight, id_seat));
        }

        public ParserRow(TableModelRow model, TableAirplaneRow airplane, TableSeatRow seat, TableAirportRow airport_in, TableAirportRow airport_out, TableFlightRow flight, TablePassengerRow passenger)
        {
            tableRows = new CrossTableFullRow(
                model,
                airplane,
                seat,
                airport_in,
                airport_out,
                flight,
                passenger);

            fullRow = new FullRow(
                tableRows.model.name,
                tableRows.airplane.number,
                tableRows.seat.code,
                tableRows.seat.id,
                tableRows.airport_in.code,
                tableRows.airport_in.city,
                tableRows.airport_in.country,
                tableRows.airport_out.code,
                tableRows.airport_out.city,
                tableRows.airport_out.country,
                tableRows.flight.number,
                tableRows.flight.date_time,
                tableRows.passenger.number);
        }

        public ParserRow(FullRow info)
        {
            this.fullRow = info;

            tableRows = new CrossTableFullRow(
                new TableModelRow(info.name_model),
                new TableAirplaneRow(info.number_airplane, info.name_model),
                new TableSeatRow(info.id_seat, info.code_seat, info.number_airplane),
                new TableAirportRow(info.code_airport_in, info.city_airport_in, info.country_airport_in),
                new TableAirportRow(info.code_airport_out, info.city_airport_out, info.country_airport_out),
                new TableFlightRow(info.number_flight, info.date_time_flight, info.number_airplane, info.code_airport_in, info.code_airport_out),
                new TablePassengerRow(info.number_passenger, info.number_flight, info.id_seat));
        }

        public ParserRow(CrossTableFullRow tables)
        {
            this.tableRows = tables;

            fullRow = new FullRow(
                tables.model.name,
                tables.airplane.number,
                tables.seat.code,
                tables.seat.id,
                tables.airport_in.code,
                tables.airport_in.city,
                tables.airport_in.country,
                tables.airport_out.code,
                tables.airport_out.city,
                tables.airport_out.country,
                tables.flight.number,
                tables.flight.date_time,
                tables.passenger.number);
        }

        public CrossTableFullRow GetCrossTableFullRow()
        {
            return tableRows;
        }

        public FullRow GetFullRow()
        {
            return fullRow;
        }
    }
}
