namespace Thesis.Relinq.Tests.Models
{
    /*  kod_grupy     | integer
        kod_przed_sem | integer
        kod_uz        | integer
        max_osoby     | smallint
        rodzaj_zajec  | character
        termin        | character
        sala          | character varying  */

    public class Grupa
    {
        public int KodGrupy { get; set; }
        public int KodPrzedSem { get; set; }
        public int KodUz { get; set; }
        public short MaxOsoby { get; set; }
        public char RodzajZajec { get; set; }
        public string Termin { get; set; }
        public string Sala { get; set; }
    }
}