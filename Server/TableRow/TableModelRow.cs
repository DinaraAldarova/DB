using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.TableRow
{
    public class TableModelRow
    {
        public string name;

        public TableModelRow(string name_model)
        {
            this.name = name_model;
        }

        public string GetSQL()
        {
            return "INSERT INTO public.model (name) VALUES('"
                + name + "'::text);";
        }

        public static string GetCreateQuery()
        {
            return @"CREATE TABLE public.model
                (
                    name text COLLATE pg_catalog.""default"" NOT NULL,
                    CONSTRAINT model_pkey PRIMARY KEY(name)
                )

                TABLESPACE pg_default;

                ALTER TABLE public.model
                    OWNER to postgres;";
        }

        public override string ToString()
        {
            return string.Format("{0, -10}", name);
        }

        public override bool Equals(object obj)
        {
            return obj is TableModelRow row &&
                   name == row.name;
        }
    }
}
