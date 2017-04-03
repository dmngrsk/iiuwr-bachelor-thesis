namespace Thesis.Relinq.UnitTests.Models
{
    /*  kod_grupy     | integer
        kod_przed_sem | integer
        kod_uz        | integer
        max_osoby     | smallint
        rodzaj_zajec  | character
        termin        | character
        sala          | character varying  */

    public class grupa
    {
        public int kod_grupy { get; set; }
        public int kod_przed_sem { get; set; }
        public int kod_uz { get; set; }
        public short max_osoby { get; set; }
        public char rodzaj_zajec { get; set; }
        public string termin { get; set; }
        public string sala { get; set; }
    }
}