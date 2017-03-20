namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class NamedParameter
    {
        public NamedParameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public object Value { get; set; }
    }
}