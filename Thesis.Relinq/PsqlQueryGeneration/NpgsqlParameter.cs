namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class NpgsqlParameter
    {
        public NpgsqlParameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}