namespace Client.Structs
{
    struct Seat
    {
        public long id;
        public string code;
        public int i_airplane;

        public Seat(long id, string code, int i_airplane)
        {
            this.id = id;
            this.code = code;
            this.i_airplane = i_airplane;
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, {2, -10}", id, code, i_airplane);
        }
    }
}
