using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DVD_Rent.Models;
using Microsoft.EntityFrameworkCore;

namespace DVD_Rent.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext db;

    public HomeController(AppDbContext context)
    {
        db = context;
    }

    public ActionResult Index()
    {
        var populerFilmler = db.Film
            .FromSqlRaw(@"
                SELECT TOP 5
                    f.film_id,
                    f.ad,
                    f.ulke,
                    f.yil,
                    f.tur,
                    f.ozet,
                    f.oyuncular,
                    f.kopya_sayisi,
                    COUNT(k.kiralama_id) AS kiralama_sayisi
                FROM Film f
                LEFT JOIN Kiralama k ON f.film_id = k.film_id
                GROUP BY f.film_id, f.ad, f.ulke, f.yil, f.tur, f.ozet, f.oyuncular, f.kopya_sayisi
                ORDER BY kiralama_sayisi DESC
            ")
            .ToList();

        var azTercihEdilenFilmler = db.Film
            .FromSqlRaw(@"
                SELECT TOP 5
                    f.film_id,
                    f.ad,
                    f.ulke,
                    f.yil,
                    f.tur,
                    f.ozet,
                    f.oyuncular,
                    f.kopya_sayisi,
                    COUNT(k.kiralama_id) AS kiralama_sayisi
                FROM Film f
                LEFT JOIN Kiralama k ON f.film_id = k.film_id
                GROUP BY f.film_id, f.ad, f.ulke, f.yil, f.tur, f.ozet, f.oyuncular, f.kopya_sayisi
                ORDER BY kiralama_sayisi ASC
            ")
            .ToList();

        // Günlük kiralanan film kopya sayısı
        var gunlukKiralamaOzet = new List<dynamic>();
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = @"
                SELECT 
                    CAST(kiralama_tarihi AS DATE) AS Gun, 
                    COUNT(*) AS ToplamFilmSayisi
                FROM Kiralama
                GROUP BY CAST(kiralama_tarihi AS DATE)
                ORDER BY Gun DESC";
            db.Database.OpenConnection();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    gunlukKiralamaOzet.Add(new {
                        Gun = reader.GetDateTime(0),
                        ToplamFilmSayisi = reader.GetInt32(1)
                    });
                }
            }
            db.Database.CloseConnection();
        }

        ViewBag.PopulerFilmler = populerFilmler;
        ViewBag.AzTercihEdilenFilmler = azTercihEdilenFilmler;
        ViewBag.GunlukKiralamaOzet = gunlukKiralamaOzet;

        return View();
    }

    // Kiralama işlemlerinin listelenmesi ve yönetimi için ana metot.
    // Kiralama tablosundaki tüm kayıtları çeker ve ViewBag ile filmleri de ekler. 
    public ActionResult Kiralama()
    {
        var kiralamalar = db.Kiralama
            .FromSqlRaw("SELECT * FROM Kiralama")
            .ToList();

        var filmler = db.Film
            .FromSqlRaw("SELECT * FROM Film")
            .ToList();

        ViewBag.Filmler = filmler;

        return View(kiralamalar);
    }

    public ActionResult Musteriler()
    {
        var musteri = db.Musteri
            .FromSqlRaw("SELECT * FROM Musteri")
            .ToList();
        return View(musteri);
    }

    public IActionResult MusteriEkle()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MusteriEkle(Musteri musteri)
    {
        // TC kimlik no kontrolü
        var sqlTc = "SELECT * FROM Musteri WHERE tc_kimlik_no = {0}";
        var mevcutTc = db.Musteri.FromSqlRaw(sqlTc, musteri.tc_kimlik_no).AsEnumerable().FirstOrDefault();
        if (mevcutTc != null)
        {
            ModelState.AddModelError("tc_kimlik_no", "Bu TC Kimlik No zaten kayıtlı.");
            return View(musteri);
        }

        // Telefon kontrolü
        var sqlTel = "SELECT * FROM Musteri WHERE telefon = {0}";
        var mevcutTel = db.Musteri.FromSqlRaw(sqlTel, musteri.telefon).AsEnumerable().FirstOrDefault();
        if (mevcutTel != null)
        {
            ModelState.AddModelError("telefon", "Bu telefon numarası zaten kayıtlı.");
            return View(musteri);
        }

        if (ModelState.IsValid)
        {
            db.Database.ExecuteSqlRaw(
                "INSERT INTO Musteri (tc_kimlik_no, isim, soyisim, telefon, cinsiyet, uye_tarihi) VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                musteri.tc_kimlik_no, musteri.isim, musteri.soyisim, musteri.telefon, musteri.cinsiyet, musteri.uye_tarihi);
            return RedirectToAction("Musteriler");
        }
        return View(musteri);
    }

    public IActionResult MusteriDuzenle(int id)
    {
        var musteri = db.Musteri
            .FromSqlRaw("SELECT * FROM Musteri WHERE musteri_id = {0}", id)
            .FirstOrDefault();
        if (musteri == null) return NotFound();
        return View(musteri);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MusteriDuzenle(Musteri musteri)
    {
        // TC kimlik no kontrolü
        var sqlTc = "SELECT * FROM Musteri WHERE tc_kimlik_no = {0} AND musteri_id <> {1}";
        var mevcutTc = db.Musteri.FromSqlRaw(sqlTc, musteri.tc_kimlik_no, musteri.musteri_id).AsEnumerable().FirstOrDefault();
        if (mevcutTc != null)
        {
            ModelState.AddModelError("tc_kimlik_no", "Bu TC Kimlik No zaten kayıtlı.");
            return View(musteri);
        }

        // Telefon kontrolü
        var sqlTel = "SELECT * FROM Musteri WHERE telefon = {0} AND musteri_id <> {1}";
        var mevcutTel = db.Musteri.FromSqlRaw(sqlTel, musteri.telefon, musteri.musteri_id).AsEnumerable().FirstOrDefault();
        if (mevcutTel != null)
        {
            ModelState.AddModelError("telefon", "Bu telefon numarası zaten kayıtlı.");
            return View(musteri);
        }

        if (ModelState.IsValid)
        {
            db.Database.ExecuteSqlRaw(
                "UPDATE Musteri SET tc_kimlik_no = {0}, isim = {1}, soyisim = {2}, telefon = {3}, cinsiyet = {4}, uye_tarihi = {5} WHERE musteri_id = {6}",
                musteri.tc_kimlik_no, musteri.isim, musteri.soyisim, musteri.telefon, musteri.cinsiyet, musteri.uye_tarihi, musteri.musteri_id);
            return RedirectToAction("Musteriler");
        }
        return View(musteri);
    }

    public IActionResult MusteriSil(int id)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM Kiralama WHERE musteri_id = {0}", id);
        db.Database.ExecuteSqlRaw("DELETE FROM Musteri WHERE musteri_id = {0}", id);
        return RedirectToAction("Musteriler");
    }

    public ActionResult Film()
    {
        var filmler = db.Film
            .FromSqlRaw(@"
                SELECT 
                    f.film_id,
                    f.ad,
                    f.ulke,
                    f.yil,
                    f.tur,
                    f.ozet,
                    f.oyuncular,
                    f.kopya_sayisi,
                    COUNT(k.kiralama_id) AS kiralama_sayisi
                FROM Film f
                LEFT JOIN Kiralama k ON f.film_id = k.film_id
                GROUP BY f.film_id, f.ad, f.ulke, f.yil, f.tur, f.ozet, f.oyuncular, f.kopya_sayisi
            ")
            .ToList();

        return View(filmler);
    }

    public IActionResult FilmEkle()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FilmEkle(Film film)
    {
        if (ModelState.IsValid)
        {
            db.Database.ExecuteSqlRaw(
                "INSERT INTO Film (ad, ulke, yil, tur, ozet, oyuncular, kopya_sayisi) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                film.ad, film.ulke, film.yil, film.tur, film.ozet, film.oyuncular, film.kopya_sayisi);
            return RedirectToAction("Film");
        }
        return View(film);
    }

    public IActionResult FilmDuzenle(int id)
    {
        var film = db.Film
            .FromSqlRaw(@"
            SELECT 
                f.film_id,
                f.ad,
                f.ulke,
                f.yil,
                f.tur,
                f.ozet,
                f.oyuncular,
                f.kopya_sayisi,
                (SELECT COUNT(*) FROM Kiralama WHERE film_id = f.film_id) AS kiralama_sayisi
            FROM Film f
            WHERE f.film_id = {0}", id)
            .FirstOrDefault();
        if (film == null) return NotFound();
        return View(film);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FilmDuzenle(Film film)
    {
        if (ModelState.IsValid)
        {
            db.Database.ExecuteSqlRaw(
                "UPDATE Film SET ad = {0}, ulke = {1}, yil = {2}, tur = {3}, ozet = {4}, oyuncular = {5}, kopya_sayisi = {6} WHERE film_id = {7}",
                film.ad, film.ulke, film.yil, film.tur, film.ozet, film.oyuncular, film.kopya_sayisi, film.film_id);
            return RedirectToAction("Film");
        }
        return View(film);
    }

    public IActionResult FilmSil(int id)
    {
        db.Database.ExecuteSqlRaw("DELETE FROM Kiralama WHERE film_id = {0}", id);
        db.Database.ExecuteSqlRaw("DELETE FROM Film WHERE film_id = {0}", id);
        return RedirectToAction("Film");
    }

    // Yeni kiralama ekleme ekranı için gerekli müşteri, film ve aktif kiralama sayısı verilerini hazırlar.
    // Aktif kiralama sayısı, müşterinin şu anda iade etmediği kiralama adedini gösterir.
    public IActionResult KiralamaEkle()
    {
        ViewBag.Musteriler = db.Musteri.FromSqlRaw("SELECT * FROM Musteri").ToList();
        ViewBag.Filmler = db.Film.FromSqlRaw("SELECT * FROM Film").ToList();

        // Her müşteri için aktif (iade edilmemiş) kiralama sayısını hesaplar.
        var aktifKiralamaSayilari = new Dictionary<int, int>();
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = @"
                SELECT musteri_id, COUNT(*) AS sayi
                FROM Kiralama
                WHERE iade_edildi_mi = 0
                GROUP BY musteri_id";
            db.Database.OpenConnection();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int musteriId = reader.GetInt32(0);
                    int sayi = reader.GetInt32(1);
                    aktifKiralamaSayilari[musteriId] = sayi;
                }
            }
            db.Database.CloseConnection();
        }

        ViewBag.AktifKiralamaSayilari = aktifKiralamaSayilari;

        return View();
    }

    // Yeni kiralama ekleme işlemi. Müşterinin aktif kiralama limiti ve filmin stok durumu kontrol edilir.
    // Eğer müşteri 3'ten fazla aktif kiralamaya sahipse veya film stokta yoksa işlem yapılmaz.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult KiralamaEkle(int musteri_id, int film_id, int gunSayisi)
    {
        // Müşterinin aktif kiralama sayısı kontrolü
        int aktifKiralamaSayisi = 0;
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM Kiralama WHERE musteri_id = @musteri_id AND iade_edildi_mi = 0";
            var param = command.CreateParameter();
            param.ParameterName = "@musteri_id";
            param.Value = musteri_id;
            command.Parameters.Add(param);

            db.Database.OpenConnection();
            aktifKiralamaSayisi = Convert.ToInt32(command.ExecuteScalar());
            db.Database.CloseConnection();
        }

        if (aktifKiralamaSayisi >= 3)
        {
            // Müşteri limiti doluysa hata mesajı döner.
            ViewBag.Hata = "Bu müşteri şu anda maksimum 3 film kiralamış durumda. Lütfen önce iade yapınız.";
            ViewBag.Musteriler = db.Musteri.FromSqlRaw("SELECT * FROM Musteri").ToList();
            ViewBag.Filmler = db.Film.FromSqlRaw("SELECT * FROM Film").ToList();
            return View();
        }
        // Film stok kontrolü. Eğer stok yoksa kiralama yapılamaz.
        int kopyaSayisi = 0;
        using (var command = db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "SELECT kopya_sayisi FROM Film WHERE film_id = @film_id";
            var param = command.CreateParameter();
            param.ParameterName = "@film_id";
            param.Value = film_id;
            command.Parameters.Add(param);

            db.Database.OpenConnection();
            var result = command.ExecuteScalar();
            kopyaSayisi = result != null ? Convert.ToInt32(result) : 0;
            db.Database.CloseConnection();
        }

        if (kopyaSayisi <= 0)
        {
            // Film stokta yoksa hata mesajı döner.
            ViewBag.Hata = "Seçilen filmde kiralanabilir kopya yok.";
            ViewBag.Musteriler = db.Musteri.FromSqlRaw("SELECT * FROM Musteri").ToList();
            ViewBag.Filmler = db.Film.FromSqlRaw("SELECT * FROM Film").ToList();
            return View();
        }

        if (gunSayisi < 1)
        { 
            //Kiralama süresi en az 1 gün olmalı.
            ViewBag.Hata = "Kiralanacak gün sayısı en az 1 olmalıdır.";
            ViewBag.Musteriler = db.Musteri.FromSqlRaw("SELECT * FROM Musteri").ToList();
            ViewBag.Filmler = db.Film.FromSqlRaw("SELECT * FROM Film").ToList();
            return View();
        }

        // Kiralama işlemi veritabanına eklenir.
        db.Database.ExecuteSqlRaw(
            "INSERT INTO Kiralama (musteri_id, film_id, kiralama_tarihi, iade_tarihi, gecikti_mi, iade_edildi_mi, ucret) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
            musteri_id, film_id, DateTime.Now, DateTime.Now.AddDays(gunSayisi), false, false, gunSayisi * 5);

        // Film stoktan bir adet düşülür.
        db.Database.ExecuteSqlRaw(
            "UPDATE Film SET kopya_sayisi = kopya_sayisi - 1 WHERE film_id = {0}", film_id);

        // Filmin toplam kiralama sayısı güncellenir.
        db.Database.ExecuteSqlRaw(
            @"UPDATE Film 
          SET kiralama_sayisi = (SELECT COUNT(*) FROM Kiralama WHERE film_id = {0}) 
          WHERE film_id = {0}", film_id);

        return RedirectToAction("Kiralama");
    }

    // Kiralama iade işlemi. İade tarihi ve gecikme kontrolü yapılır, ceza hesaplanır ve stok güncellenir.
    [HttpPost]
    public IActionResult KiralamaIade(int kiralama_id)
    {
        var bugun = DateTime.Now;

        // İade edilecek kiralama kaydı çekilir.
        var kiralama = db.Kiralama.FromSqlRaw("SELECT * FROM Kiralama WHERE kiralama_id = {0}", kiralama_id).FirstOrDefault();
        if (kiralama == null) return NotFound();

        int gecikmeGun = 0;
        decimal ceza = 0;
        // Gecikme kontrolü ve ceza hesaplama
        if (kiralama.iade_tarihi.HasValue && bugun > kiralama.iade_tarihi.Value && !kiralama.iade_edildi_mi)
        {
            gecikmeGun = (bugun.Date - kiralama.iade_tarihi.Value.Date).Days;
            if (gecikmeGun > 0)
            {
                ceza = gecikmeGun * 10;
            }
        }

        // Kiralama kaydı iade edildi olarak güncellenir, ceza eklenir.
        db.Database.ExecuteSqlRaw(
            @"UPDATE Kiralama 
          SET iade_edildi_mi = 1, iade_tarihi = {0}, gecikti_mi = {1}, ucret = ucret + {2}
          WHERE kiralama_id = {3}",
            bugun, gecikmeGun > 0 ? 1 : 0, ceza, kiralama_id);

        // Film stokuna bir adet eklenir.
        db.Database.ExecuteSqlRaw(
            "UPDATE Film SET kopya_sayisi = kopya_sayisi + 1 WHERE film_id = {0}", kiralama.film_id);

        // Filmin toplam kiralama sayısı tekrar güncellenir.
        db.Database.ExecuteSqlRaw(
            @"UPDATE Film 
          SET kiralama_sayisi = (SELECT COUNT(*) FROM Kiralama WHERE film_id = {0}) 
          WHERE film_id = {0}", kiralama.film_id);

        return RedirectToAction("Kiralama");
    }

    
}