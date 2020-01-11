using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.TableRow
{
    public class TableFlightRow
    {
        public long number;
        public string date_time;
        public long number_airplane; 
        public string code_airport_in;
        public string code_airport_out;

        public TableFlightRow(long number, string date_time, long number_airplane, string code_airport_in, string code_airport_out)
        {
            this.number = number;
            this.date_time = date_time;
            this.number_airplane = number_airplane;
            this.code_airport_in = code_airport_in;
            this.code_airport_out = code_airport_out;
        }

        public TableFlightRow(long number, string date_time, TableAirplaneRow airplane, TableAirportRow airport_in, TableAirportRow airport_out)
        {
            this.number = number;
            this.date_time = date_time;
            this.number_airplane = airplane.number;
            this.code_airport_in = airport_in.code;
            this.code_airport_out = airport_out.code;
        }

        public string GetSQL()
        {
            return "INSERT INTO public.flight (number, number_airplane, date_time, code_airport_in, code_airport_out) VALUES('" +
                number + "'::bigint, '" + number_airplane + "'::bigint, '" + date_time + "'::timestamp, '" +
                code_airport_in + "'::text, '" + code_airport_out + "'::text);";

        }

        public static string GetCreateQuery()
        {
            return @"CREATE TABLE public.flight
                (
                    ""number"" bigint NOT NULL,
                    date_time timestamp,
                    code_airport_in text COLLATE pg_catalog.""default"",
                    code_airport_out text COLLATE pg_catalog.""default"",
                    number_airplane bigint,
                    CONSTRAINT flight_pkey PRIMARY KEY(""number""),
                    CONSTRAINT fk_flight_airplane FOREIGN KEY(number_airplane)
                        REFERENCES public.airplane(""number"") MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID,
                    CONSTRAINT fk_flight_airport_in FOREIGN KEY(code_airport_in)
                        REFERENCES public.airport(code) MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID,
                    CONSTRAINT fk_flight_airport_out FOREIGN KEY(code_airport_out)
                        REFERENCES public.airport(code) MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID,
                    CONSTRAINT check_airport_code CHECK (code_airport_in <> code_airport_out)
                        NOT VALID
                )

                TABLESPACE pg_default;

                ALTER TABLE public.flight
                    OWNER to postgres;";
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, {2, -10}, {3, -10}, {4, -10}", number, date_time, number_airplane, code_airport_in, code_airport_out);
        }

        public override bool Equals(object obj)
        {
            return obj is TableFlightRow row &&
                   number == row.number &&
                   date_time == row.date_time &&
                   number_airplane == row.number_airplane &&
                   code_airport_in == row.code_airport_in &&
                   code_airport_out == row.code_airport_out;
        }
    }
}
