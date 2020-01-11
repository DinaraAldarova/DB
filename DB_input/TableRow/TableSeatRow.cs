using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_input.TableRow
{
    public class TableSeatRow
    {
        public long id;
        public string code;
        public long number_airplane;

        public TableSeatRow(long id, string code, long number_airplane)
        {
            this.id = id;
            this.code = code;
            this.number_airplane = number_airplane;
        }

        public TableSeatRow(long id, string code, TableAirplaneRow airplane)
        {
            this.id = id;
            this.code = code;
            this.number_airplane = airplane.number;
        }

        public string GetSQL()
        {
            return "INSERT INTO public.seat (id, number_airplane, code) VALUES('" +
                id + "'::bigint, '" + number_airplane + "'::bigint, '" + code + "'::text);";
        }

        public static string GetCreateQuery()
        {
            return @"CREATE TABLE public.seat
                (
                    id bigint NOT NULL,
                    number_airplane bigint,
                    code text COLLATE pg_catalog.""default"",
                    CONSTRAINT seat_pkey PRIMARY KEY(id),
                    CONSTRAINT fk_seat_airplane FOREIGN KEY(number_airplane)
                        REFERENCES public.airplane(""number"") MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID
                )

                TABLESPACE pg_default;

                ALTER TABLE public.seat
                    OWNER to postgres;";
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, {2, -10}", id, code, number_airplane);
        }

        public override bool Equals(object obj)
        {
            return obj is TableSeatRow row &&
                   id == row.id &&
                   code == row.code &&
                   number_airplane == row.number_airplane;
        }
    }
}
