using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DVD_Rent.Models
{
    //Kiralama tablosu modeli. Her kiralama işlemi için bir kayıt tutulur.
    // Kiralama işlemlerinin tüm detayları bu modelde saklanır.
    public class Kiralama
    {

        [Key]
        public int kiralama_id { get; set; }

        public int film_id { get; set; }
        [ForeignKey("film_id")]
        public int musteri_id { get; set; }
        [ForeignKey("musteri_id")]
        
        public DateTime kiralama_tarihi { get; set; }
        public DateTime? iade_tarihi { get; set; }
        public bool gecikti_mi { get; set; }
        public bool iade_edildi_mi { get; set; }
        public decimal ucret { get; set; }
    }
}