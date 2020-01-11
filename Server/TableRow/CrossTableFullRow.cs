namespace Server.TableRow
{
    public class CrossTableFullRow
    {
        public TableModelRow model;
        public TableAirplaneRow airplane;
        public TableSeatRow seat;
        public TableAirportRow airport_in;
        public TableAirportRow airport_out;
        public TableFlightRow flight;
        public TablePassengerRow passenger;

        public CrossTableFullRow(TableModelRow model, TableAirplaneRow airplane, TableSeatRow seat, TableAirportRow airport_in, TableAirportRow airport_out, TableFlightRow flight, TablePassengerRow passenger)
        {
            this.model = model;
            this.airplane = airplane;
            this.seat = seat;
            this.airport_in = airport_in;
            this.airport_out = airport_out;
            this.flight = flight;
            this.passenger = passenger;
        }

        public override string ToString()
        {
            return model.ToString() + ", " + airplane.ToString() + ", " + seat.ToString() + ", " + airport_in.ToString() + ", " + airport_out.ToString() + ", " + flight.ToString() + ", " + passenger.ToString();
        }
    }
}
