# EnvanterApp — Envanter Yönetim Sistemi

Kurum içi envanter süreçlerini desteklemek için geliştirilmiş bir envanter yönetim web uygulaması.

## Özellikler
- Kullanıcı girişi ve kimlik doğrulama
- Rol tabanlı yetkilendirme: Süper Admin / Admin / Employee
- Envanter verilerinin yönetimi için temel CRUD işlemleri
- SQL Server ile veritabanı işlemleri
- Entity Framework Core ile veri erişim katmanı
- ASP.NET Core MVC mimarisi (Controller + Razor Views)

## Kullanılan Teknolojiler
- C#
- ASP.NET Core MVC
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Identity

## Kurulum ve Çalıştırma
1. Projeyi klonlayın:
   ```bash
   git clone https://github.com/vlylmz/EnvanterApp.git
Visual Studio ile açın.
appsettings.json dosyasında veritabanı bağlantı bilgisini kendi ortamınıza göre güncelleyin.
Migration ve veritabanını oluşturun:
   dotnet ef database update
Uygulamayı çalıştırın:
   dotnet run
Notlar
İlk kurulumda rol ve başlangıç kullanıcıları oluşturulacak şekilde yapılandırma yapılmıştır.
Geliştirme ortamı için bağlantı ayarlarını kendi SQL Server kurulumunuza göre düzenleyin.
