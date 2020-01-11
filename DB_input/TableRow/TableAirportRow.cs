using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_input.TableRow
{
    public class TableAirportRow
    {
        public string code;
        public string city;
        public string country;

        public TableAirportRow(string code, string city, string country)
        {
            this.code = code;
            this.city = city;
            this.country = country;
        }

        public string GetSQL()
        {
            return "INSERT INTO public.airport (code, city, country) VALUES('"
                + code + "'::text, '" + city + "'::text, '" + country + "'::text);";
        }

        public static string GetCreateQuery()
        {
            return @"CREATE TABLE public.airport
                (
                    code text COLLATE pg_catalog.""default"" NOT NULL,
                    city text COLLATE pg_catalog.""default"",
                    country text COLLATE pg_catalog.""default"",
                    CONSTRAINT airport_pkey PRIMARY KEY(code)
                )

                TABLESPACE pg_default;

                ALTER TABLE public.airport
                    OWNER to postgres;";
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, {2, -10}", code, city, country);
        }

        public override bool Equals(object obj)
        {
            return obj is TableAirportRow row &&
                   code == row.code &&
                   city == row.city &&
                   country == row.country;
        }
    }
}
