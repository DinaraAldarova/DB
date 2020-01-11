namespace Client.Structs
{
    struct Model
    {
        public string name;

        public Model(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return string.Format("{0, -10}", name);
        }
    }
}
