using System.ComponentModel.DataAnnotations;

namespace DVD_Rent.Models
{
    public class Film
    {
        public Film()
        {
            kopya_sayisi = 2;
        }
        [Key]
        public int film_id { get; set; }
        public string? ad { get; set; }
        public string? ulke { get; set; }
        public int yil { get; set; }
        public string? tur { get; set; }
        public string? ozet { get; set; }
        public string? oyuncular { get; set; }
        public int kopya_sayisi { get; set; }
        public int? kiralama_sayisi { get; set; }
    }
}
