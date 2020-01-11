namespace Client.Structs
{
    struct Flight
    {
        public int i_airplane;
        public int i_airport_in;
        public int i_airport_out;
        public string date_time;

        public Flight(int i_airplane, int i_airport_in, int i_airport_out, string date_time)
        {
            this.i_airplane = i_airplane;
            this.i_airport_in = i_airport_in;
            this.i_airport_out = i_airport_out;
            this.date_time = date_time;
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, аэропорт №{2, -3}, аэропорт №{3, -3}", date_time, i_airplane, i_airport_in, i_airport_out);
        }
    }
}
