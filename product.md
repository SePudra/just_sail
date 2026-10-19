# Just Sail — Ürün Tanımı

Sürüm: 0.4 · 16 Ekim 2026 · M2b görsel ve keşif uyarlaması

## Oyun ve deneyim

Just Sail, teknenin ana karakter olduğu, low-poly adalar arasında serbestçe dolaşılan tek oyunculu 3D deniz keşif oyunudur. Oyuncu kıyı, koy ve ufuk silüetlerini takip eder; yolculuk tek başına keyif vermelidir. Over the Hill'in araçla manzara arasında kurduğu bağ ilhamdır; tekne, dünya ve sunum özgün üretilir.

Yarış, leaderboard, geri sayım, süre baskısı veya zorunlu ana görev yoktur. Yeni tekneler açmak isteğe bağlıdır; başlangıç teknesi kasıtlı sıkıcı yapılmaz. Akış: denize açıl → merak ettiğin kıyıya git → dolaş/yavaşla → istersen iskeleye uğra → devam et. Beklemek veya eski tekneye dönmek kayıp sayılmaz.

## Platform ve araç seçimi

Windows PC, çevrimdışı, klavye/fare; ilk inceleme yerel Windows build'iyle. Unity 6.6 **6000.6.6f1**, Universal 3D/URP; Blender **5.2.2 LTS**. URP sürümü şablondan alınır. İlk UI Türkçe; gamepad daha sonra. Ticari model kararlaştırılmadı; prototipte reklam, satın alma veya hesap yok.

## M1 — İlk oynanabilir teslim

| Alan | Gerekli özellikler |
| --- | --- |
| Tekne | Bir özgün küçük açık yelkenli; itiş, dümen, su direnci, dalgada yüzme, sınırlı yatış ve yelken tepkisi |
| Kontrol | Denizcilik bilgisi gerektirmeyen sürüş; ters rüzgârda tamamen durma yok; kıyı/iskele çarpışması ve güvenli suya kurtarma |
| Kamera | Yumuşak üçüncü şahıs takip, fareyle çevreye bakış ve sınırlı zoom |
| Dünya | İki farklı ada, bir yaklaşılabilir ahşap iskele ve çevrelerinde açık su; ada ayrıntıları design.md'de |
| Keşif | Martılar/yunuslar, puansız fotoğraf vizörü ve kalıcı seyir defteri; yavaş yaklaşmada fizik tabanlı palamar, gece iskelede sabahı bekleme |
| Su | Sığlık rengi, hareketli yüzey, kıyı köpüğü ve hız/dönüşe tepki veren tekne izi |
| Atmosfer | M2 ile 24 dakikalık gün/gece, güneş/ay/yıldızlar, hareketli bulutlar, açık/kapalı/fırtınalı/gezici hava; dalga/rüzgâr/yağmur/ses/ıslaklık birlikte değişir |
| Ana menü | Denize Açıl, Ayarlar, Çıkış |
| Pause | Devam Et, Ayarlar, Tekneyi Kurtar, Ana Menü, Oyundan Çık; Esc ve HUD düğmesiyle erişim |
| Ayarlar | Ana ses, desteklenen çözünürlük/ekran modu, kalite ve kamera hassasiyeti kalıcıdır; ışık/hava/gün döngüsü/fırtına gücü oturumluk deneme kontrolleridir |
| HUD | Kısa kontrol yardımı, menü erişimi, gerekiyorsa küçük rüzgâr göstergesi. Hız/puan paneli ve harita varsayılan değil |

Menüde fizik ve ortak dalga zamanı durur, imleç serbest olur. Ayarlardan geri dönüş pause'a gelir; ana menüye dönüş pause durumunu temizler. Odak kaybında duraklatılır; geri gelince Devam Et gerekir. Çözünürlük değişiminde önceki ayara dönme olanağı bulunur. Kalıcı yolculuk kaydı yokken Devam düğmesi veya uygulanmamış puan göstergesi sunulmaz.

İlk sahne sınırlıdır; teslim notunda açıklanır. Dünya kenarı görünür duvar olarak sunulmaz. İskeleye yanaşılabilir, ancak NPC/yükseltme etkileşimi M1 şartı değildir.

### Başlangıç kontrolleri

| Girdi | İşlev |
| --- | --- |
| W / S | İtiş/yelken gücü hedefini artır/azalt; bırakınca hedef korunur |
| A / D | Dümen |
| Sağ fare + hareket | Çevreye bakış |
| Tekerlek | Kamera uzaklığı |
| Esc / menü düğmesi | Pause erişimi / oyuna dönüş |
| F; sol tık / Space | Fotoğraf vizörünü aç/kapat; fotoğraf çek |
| Tab | Seyir defterini aç/kapat; büyük fotoğraftayken önce deftere döner |
| C | Yumuşak odak etkisini aç/kapat |
| E | Yakın iskeleye yavaş yaklaşırken bağlan; bağlıyken çöz |
| R | Gece palamar bağlıyken sabahı bekle |

S gücü azaltır; tekneyi anında geri götürmez. Düşük hızda yanaşma kontrolü test edilir; manevra yardımı gerekirse karar kaydedilir.

## Sonraki kapsam

M2b'de fotoğraf albümü yerel ve kalıcıdır; tekne konumu/ilerleme kaydı henüz yoktur. Prosedürel/sonsuz dünya, streaming, ikinci tekne, Sail Point, NPC diyalogları ve yükseltme alışverişi sonraki kapsamdadır. Yaya keşif, yüzme/dalış, multiplayer, savaş, hayatta kalma kaynakları, tam yelken simülasyonu, su alma/batma, mağaza yayını ve çevrimiçi servisler de ilk kapsamın dışındadır. Fırtına atmosfer ve seyir hissini etkiler; cezalandırıcı bir hayatta kalma sistemi değildir.

### Tekne ilerlemesi

Nihai başlangıç küçük bot/sal olabilir; M1'de yelkenli kullanmak bu seçimi kesinleştirmez. Küçük yelkenli ve kamaralı gezi teknesinden sonra katamaran/motor yat farklı his veren dallar olabilir. Sıra/sayı henüz kesin değil. Yeni tekne eskisini anlamsızlaştırmaz; açılan eski tekneler iskelede yeniden seçilebilir.

### Sail Point taslağı

- Seyirde kat edilen yatay mesafeden birikir; trick, manevra veya hız bonusu yoktur. Dikey dalga hareketi, pause, yükleme ve kurtarma/teleport mesafesi sayılmaz.
- Toplam birikim eşiklerinde tekne açılır; puan harcanmaz. Eşikler/oranlar henüz belirlenmedi.
- Aynı rotayı tekrar etmek cezalandırılmaz; zorunlu yeni alan bonusu yoktur. En hızlı teknenin diğerlerini baskılaması ve boşta biriktirme gerçek testle değerlendirilir.
- Sürekli büyük sayaç yerine iskelede görünür. Usta gereksinimi kısa anlatır; başlangıçta malzeme toplama şartı yoktur.

### Ada dünyası taslağı

Seed tabanlı, geri dönüldüğünde aynı kalan dünya. Tasarlanmış ada aileleri; kurallı kıyı, boyut, bitki ve yerleşim varyasyonları. İskeleler yeterli derinlik ve açık yaklaşma koridoruna sahip korunaklı kıyılara yerleştirilir. Boğaz/koy/silüetler yolculuğu değiştirir; açık denizde sessiz aralıklar korunur.

## Değerlendirme

Kullanıcı sürüşün keyfini, suyun stilini, adaların merak uyandırmasını, kamera rahatlığını ve HUD'un müdahalesini oynayarak değerlendirir. İlk hedef sorunsuz 10 dakikalık serbest dolaşımdır. M1 teknik testleri geçmesine rağmen görsel yönden reddedildi. SoT ve Over the Hill uyarlamalarının kısa Windows kontrolleri teknik kanıttır; yeni görsel sonuç ve sürüş keyfi kullanıcı değerlendirmesini bekler. M1'in uzun testi yeni fizik sistemine kanıt sayılmaz. Ölçüm ve teslim kontrolleri [management.md](management.md), sanat yönü [design.md](design.md), yorum ve düzeltme süreci [feedback.md](feedback.md) içindedir.
