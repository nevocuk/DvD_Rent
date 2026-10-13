# DVD Kiralama Yönetim Sistemi

Bir DVD kiralama dükkânının film, müşteri ve kiralama işlemlerini yöneten web uygulaması. ASP.NET Core MVC ve SQL Server ile, web programlama dersi projesi olarak geliştirildi.

## Özellikler

- **Gösterge paneli:** en çok ve en az kiralanan filmler, günlük kiralama sayıları
- **Film yönetimi:** ekleme, düzenleme, silme; kopya (stok) takibi
- **Müşteri yönetimi:** ekleme, düzenleme, silme; TC kimlik no ve telefon için format ve tekrar kontrolü
- **Kiralama ve iade:**
  - Bir müşteri aynı anda en fazla 3 film kiralayabilir
  - Stokta kopyası olmayan film kiralanamaz
  - Ücret günlük 5 TL; geç iadede gün başına 10 TL ceza eklenir
  - Kiralama ve iadede stok ile filmin kiralanma sayısı otomatik güncellenir

## Teknolojiler

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core 9 (SQL Server sağlayıcısı)
- SQL Server / LocalDB
- Razor görünümleri, Bootstrap

Sorgular parametreli olarak yazılmıştır (`FromSqlRaw` / `ExecuteSqlRaw` ile `{0}` yer tutucuları ve `DbParameter`).

## Kurulum

1. **.NET 9 SDK** ve **SQL Server LocalDB** kurulu olmalı (Visual Studio ile birlikte gelir).
2. Veritabanını ve örnek verileri oluşturun:
   ```
   sqlcmd -S "(localdb)\MSSQLLocalDB" -i veritabani/kurulum.sql
   ```
   SQL Server Management Studio kullanıyorsanız `veritabani/kurulum.sql` dosyasını açıp çalıştırmanız yeterli.
3. Farklı bir SQL Server kullanıyorsanız `DVD_RentN/DVD_RentN/appsettings.json` içindeki `DefaultConnection` değerini değiştirin.
4. Uygulamayı başlatın:
   ```
   dotnet run --project DVD_RentN/DVD_RentN
   ```

Örnek verilerin tamamı uydurmadır.

## Proje yapısı

```
DVD_RentN/DVD_RentN/
├── Controllers/HomeController.cs   # Tüm sayfalar ve iş kuralları
├── Models/                         # Film, Musteri, Kiralama, AppDbContext
├── Views/Home/                     # Film, müşteri ve kiralama ekranları
└── wwwroot/                        # CSS, JS, kütüphaneler
veritabani/kurulum.sql              # Tablolar ve örnek veriler
```
