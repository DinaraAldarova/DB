namespace DB_input.Structs
{
    struct Airport
    {
        public string code;
        public string city;
        public string country;

        public Airport(string code, string city, string country)
        {
            this.code = code;
            this.city = city;
            this.country = country;
        }

        public override string ToString()
        {
            return string.Format("{0, -10}, {1, -10}, {2, -10}", code, city, country);
        }
    }
}
