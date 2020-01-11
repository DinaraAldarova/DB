using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.TableRow;

namespace Server
{
    public class ParserTable
    {
        public List<TableModelRow> tableModel;
        public List<TableAirplaneRow> tableAirplane;
        public List<TableSeatRow> tableSeat;
        public List<TableAirportRow> tableAirport;
        public List<TableFlightRow> tableFlight;
        public List<TablePassengerRow> tablePassenger;

        public ParserTable(List<CrossTableFullRow> tables)
        {
            tableModel = new List<TableModelRow>();
            tableAirplane = new List<TableAirplaneRow>();
            tableSeat = new List<TableSeatRow>();
            tableAirport = new List<TableAirportRow>();
            tableFlight = new List<TableFlightRow>();
            tablePassenger = new List<TablePassengerRow>();

            foreach (CrossTableFullRow fullRow in tables)
            {
                if (tableModel.FindIndex(x => x.Equals(fullRow.model)) == -1)
                {
                    //если такого элемента еще нет
                    tableModel.Add(fullRow.model);
                }

                if (tableAirplane.FindIndex(x => x.Equals(fullRow.airplane)) == -1)
                {
                    //если такого элемента еще нет
                    tableAirplane.Add(fullRow.airplane);
                }

                if (tableSeat.FindIndex(x => x.Equals(fullRow.seat)) == -1)
                {
                    //если такого элемента еще нет
                    tableSeat.Add(fullRow.seat);
                }

                if (tableAirport.FindIndex(x => x.Equals(fullRow.airport_in)) == -1)
                {
                    //если такого элемента еще нет
                    tableAirport.Add(fullRow.airport_in);
                }

                if (tableAirport.FindIndex(x => x.Equals(fullRow.airport_out)) == -1)
                {
                    //если такого элемента еще нет
                    tableAirport.Add(fullRow.airport_out);
                }

                if (tableFlight.FindIndex(x => x.Equals(fullRow.flight)) == -1)
                {
                    //если такого элемента еще нет
                    tableFlight.Add(fullRow.flight);
                }

                if (tablePassenger.FindIndex(x => x.Equals(fullRow.passenger)) == -1)
                {
                    //если такого элемента еще нет
                    tablePassenger.Add(fullRow.passenger);
                }
            }
        }
    }
}
