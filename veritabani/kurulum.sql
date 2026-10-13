-- DVD Kiralama Yönetim Sistemi - veritabanı kurulumu
-- Çalıştırma: sqlcmd -S "(localdb)\MSSQLLocalDB" -i veritabani/kurulum.sql
-- Örnek verilerin tamamı uydurmadır; gerçek kişilere ait değildir.

IF DB_ID('DVD_RENTS') IS NULL
    CREATE DATABASE DVD_RENTS;
GO

USE DVD_RENTS;
GO

IF OBJECT_ID('dbo.Kiralama', 'U') IS NOT NULL DROP TABLE dbo.Kiralama;
IF OBJECT_ID('dbo.Musteri', 'U') IS NOT NULL DROP TABLE dbo.Musteri;
IF OBJECT_ID('dbo.Film', 'U') IS NOT NULL DROP TABLE dbo.Film;
GO

CREATE TABLE dbo.Film (
    film_id         INT IDENTITY(1,1) PRIMARY KEY,
    ad              NVARCHAR(200) NOT NULL,
    ulke            NVARCHAR(100) NULL,
    yil             INT NOT NULL,
    tur             NVARCHAR(100) NULL,
    ozet            NVARCHAR(MAX) NULL,
    oyuncular       NVARCHAR(500) NULL,
    kopya_sayisi    INT NOT NULL DEFAULT 2,
    kiralama_sayisi INT NULL DEFAULT 0
);

CREATE TABLE dbo.Musteri (
    musteri_id   INT IDENTITY(1,1) PRIMARY KEY,
    tc_kimlik_no NVARCHAR(11) NOT NULL UNIQUE,
    isim         NVARCHAR(50) NOT NULL,
    soyisim      NVARCHAR(50) NOT NULL,
    telefon      NVARCHAR(11) NOT NULL UNIQUE,
    cinsiyet     NVARCHAR(10) NOT NULL,
    uye_tarihi   DATETIME2 NOT NULL
);

CREATE TABLE dbo.Kiralama (
    kiralama_id     INT IDENTITY(1,1) PRIMARY KEY,
    film_id         INT NOT NULL REFERENCES dbo.Film(film_id),
    musteri_id      INT NOT NULL REFERENCES dbo.Musteri(musteri_id),
    kiralama_tarihi DATETIME2 NOT NULL,
    iade_tarihi     DATETIME2 NULL,
    gecikti_mi      BIT NOT NULL DEFAULT 0,
    iade_edildi_mi  BIT NOT NULL DEFAULT 0,
    ucret           DECIMAL(10,2) NOT NULL DEFAULT 0
);
GO

-- Örnek filmler
INSERT INTO dbo.Film (ad, ulke, yil, tur, ozet, oyuncular, kopya_sayisi, kiralama_sayisi) VALUES
(N'Esaretin Bedeli',  N'ABD',        1994, N'Dram',        N'Haksız yere hapse giren bir bankacının umut hikâyesi.', N'Tim Robbins, Morgan Freeman', 2, 2),
(N'Babam ve Oğlum',   N'Türkiye',    2005, N'Dram',        N'Bir babanın oğluyla yıllar sonra köyüne dönüşü.',       N'Fikret Kuşkan, Çetin Tekindor', 3, 1),
(N'Başlangıç',        N'ABD',        2010, N'Bilim Kurgu', N'Rüyalara girerek fikir çalan bir ekibin son işi.',       N'Leonardo DiCaprio, Elliot Page', 1, 1),
(N'Eşkıya',           N'Türkiye',    1996, N'Suç',         N'Otuz beş yıl sonra hapisten çıkan Baran''ın hikâyesi.', N'Şener Şen, Uğur Yücel', 2, 0),
(N'Ruhların Kaçışı',  N'Japonya',    2001, N'Animasyon',   N'Ruhlar dünyasına düşen küçük bir kızın macerası.',      N'Rumi Hiiragi, Miyu Irino', 1, 1);

-- Örnek müşteriler (uydurma TC ve telefon numaraları)
INSERT INTO dbo.Musteri (tc_kimlik_no, isim, soyisim, telefon, cinsiyet, uye_tarihi) VALUES
(N'11111111110', N'Ayşe',   N'Örnek',   N'05000000001', N'Kadin', '2025-09-01'),
(N'22222222220', N'Mehmet', N'Deneme',  N'05000000002', N'Erkek', '2025-09-15'),
(N'33333333330', N'Zeynep', N'Test',    N'05000000003', N'Kadin', '2025-10-02');

-- Örnek kiralamalar: iki iade edilmiş (biri gecikmeli), iki aktif
-- Ücret: günlük 5 TL; gecikmede günlük 10 TL ceza eklenir.
INSERT INTO dbo.Kiralama (film_id, musteri_id, kiralama_tarihi, iade_tarihi, gecikti_mi, iade_edildi_mi, ucret) VALUES
(1, 1, '2025-10-01', '2025-10-04', 0, 1, 15.00),
(1, 2, '2025-10-03', '2025-10-08', 1, 1, 30.00),
(3, 1, '2025-10-05', '2025-10-08', 0, 0, 15.00),
(5, 3, '2025-10-06', '2025-10-09', 0, 0, 15.00),
(2, 2, '2025-10-07', '2025-10-10', 0, 1, 15.00);
GO
