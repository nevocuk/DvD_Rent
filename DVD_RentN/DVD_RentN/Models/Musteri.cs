using System.ComponentModel.DataAnnotations;

namespace DVD_Rent.Models
{
    public class Musteri
    {
        [Key]
        public int musteri_id { get; set; }

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalı")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No 11 rakamdan oluşmalı")]
        public string? tc_kimlik_no { get; set; }

        [Required(ErrorMessage = "İsim zorunludur")]
        public string? isim { get; set; }

        [Required(ErrorMessage = "Soyisim zorunludur")]
        public string? soyisim { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "Telefon 11 haneli olmalı")]
        public string? telefon { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur")]
        public string? cinsiyet { get; set; }

        [Required(ErrorMessage = "Üye tarihi zorunludur")]
        public DateTime uye_tarihi { get; set; }
    }
}