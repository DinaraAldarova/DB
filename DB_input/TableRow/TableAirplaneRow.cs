using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_input.TableRow
{
    public class TableAirplaneRow
    {
        public long number;
        public string name_model;

        public TableAirplaneRow(long number, string name_model)
        {
            this.number = number;
            this.name_model = name_model;
        }

        public TableAirplaneRow(long number, TableModelRow model)
        {
            this.number = number;
            this.name_model = model.name;
        }

        public string GetSQL()
        {
            return "INSERT INTO public.airplane (\"number\", name_model) VALUES('"
                + number + "'::bigint, '" + name_model + "'::text) returning \"number\";";
        }

        public static string GetCreateQuery()
        {
            return @"CREATE TABLE public.airplane
                (
                    ""number"" bigint NOT NULL,
                    name_model text COLLATE pg_catalog.""default"" NOT NULL,
                    CONSTRAINT airplane_pkey PRIMARY KEY(""number""),
                    CONSTRAINT fk_airplane_model FOREIGN KEY(name_model)
                        REFERENCES public.model(name) MATCH SIMPLE
                        ON UPDATE NO ACTION
                        ON DELETE NO ACTION
                        NOT VALID
                )

                TABLESPACE pg_default;

                ALTER TABLE public.airplane
                    OWNER to postgres;

                COMMENT ON CONSTRAINT fk_airplane_model ON public.airplane
                    IS 'khjk';";
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}", number, name_model);
        }

        public override bool Equals(object obj)
        {
            return obj is TableAirplaneRow row &&
                   number == row.number &&
                   name_model == row.name_model;
        }
    }
}
