using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.TableRow
{
    public class TablePassengerRow
    {
        public long number;
        public long number_flight;
        public long id_seat;

        public TablePassengerRow(long number, long number_flight, long id_seat)
        {
            this.number = number;
            this.number_flight = number_flight;
            this.id_seat = id_seat;
        }

        public TablePassengerRow(long number, TableFlightRow flight, TableSeatRow seat)
        {
            this.number = number;
            this.number_flight = flight.number;
            this.id_seat = seat.id;
        }

        public string GetSQL()
        {
            return "INSERT INTO public.passenger (\"number\", number_flight, id_seat) VALUES ('" +
                number + "'::bigint, '" + number_flight + "'::bigint, '" + id_seat + "'::bigint);";
        }

        public static string GetCreateQuery()
        {
            return @"CREATE TABLE public.passenger
                (
                    ""number"" bigint,
                    number_flight bigint NOT NULL,
                    id_seat bigint NOT NULL,
                    CONSTRAINT passenger_pkey PRIMARY KEY (number_flight, id_seat),
                    CONSTRAINT fk_passenger_flight FOREIGN KEY (number_flight)
                        REFERENCES public.flight (""number"") MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID,
                    CONSTRAINT fk_passenger_seat FOREIGN KEY(id_seat)
                        REFERENCES public.seat(id) MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID
                )

                TABLESPACE pg_default;

                ALTER TABLE public.passenger
                    OWNER to postgres;";
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, {2, -10}", number, number_flight, id_seat);
        }

        public override bool Equals(object obj)
        {
            return obj is TablePassengerRow row &&
                   number == row.number &&
                   number_flight == row.number_flight &&
                   id_seat == row.id_seat;
        }
    }
}
