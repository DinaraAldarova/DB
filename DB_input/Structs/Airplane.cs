namespace DB_input.Structs
{
    struct Airplane
    {
        public int i_model;
        public long id;
        public int length;
        public int width;

        public Airplane(int i_model, long id, int length, int width)
        {
            this.i_model = i_model;
            this.id = id;
            this.length = length;
            this.width = width;
        }

        public int Capacity()
        {
            return length * width;
        }

        public override string ToString()
        {
            return string.Format("№{0, -5}, модель №{1, -5}, длина {2, -3}, ширина {3, -3}", id, i_model, length, width);
        }
    }
}
