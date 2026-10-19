# Just Sail — Sanat Yönü ve Sunum

Sürüm: 0.5 · 16 Ekim 2026 · Video referanslı sonbahar kıyısı; görsel kabul bekliyor

## Görsel dil

**Stylized Low-Poly 3D + Minimalist Art Direction + Atmospheric Lighting.** Kullanıcının dört tanıtım videosuyla güncel hedef sonbahar kıyısı oldu: sıcak turuncu ot örtüsü, çam/huş grupları, geniş gri kaya yüzeyleri, mavi yansıtıcı su ve krem yelken. Önceki seyrek Ege kompozisyonu bu hedefi karşılamadı (FB-005). Henüz kullanıcı tarafından onaylanmış oyun render'ı yok.

Ana kare Ekim videosunun 67,5. saniyesindeki göl kıyısıdır; geniş manzara, ot/ışık ve su yakın planı yardımcıdır. Seçimler ve kaynak zamanları `Verification/VisualStudy/selected.json` içinde. “Sabah” Just Sail'in saat 08.00 ışık hedefidir; tanıtım videosunun gerçek oyun saati veya oynanış kamerası olduğu varsayılmaz. Her tur aynı poz/saat/dalga anındaki gerçek Unity karesiyle karşılaştırılır; fark ve sonraki düzeltme `Verification/VisualStudy/iterations.md` içinde tutulur.

Over the Hill araç/manzara ilişkisi, Sail Forth stilize seyir ve Wind Waker suyu teknik yaklaşım için referanstır; asset veya görüntü kopyalanmaz. Şekiller az ve anlamlı detay taşır. Kayalarda düz yüzeyler, ağaçlarda geniş yaprak kütleleri; komşu her üçgene rastgele renk verilmez. Mat ahşap/kumaş, az metal vurgu ve ortak materyaller kullanılır. Fotoğraf dokuları, yoğun normal ayrıntısı ve parlak plastik görünüm hedef değildir. Suya zorunlu üçgen görünüş dayatılmaz.

## Renk paleti

sRGB başlangıç renkleri; aydınlatılmış piksel sonucu veya kesin ayar değildir.

| Rol | Renk | Rol | Renk |
| --- | --- | --- | --- |
| Açık deniz | `#24547D` | Sığ su | `#6AA4AB` |
| Köpük | `#D7EBDE` | Kaya | `#97909A` |
| Ot örtüsü | `#FF7D3B`–`#FF9957` | Çam | `#466B38` |
| Yelken | `#F7ECD1` | Ahşap | `#946044` |
| Tekne gövdesi | `#863F35` | Huş yaprakları | Altın / pas turuncusu |

## Su

- Altı yönlü Gerstner dalgası: 64–2,7 m dalga boyları; geniş solugan, orta dalga ve küçük yüzey hareketi. Yakında sıklaşan tekne merkezli mesh silüeti taşır. FFT kullanılmıyor.
- Shader ve fizik aynı dalga parametreleri/zamanını kullanır. Gerstner yatay displacement varsa dünya XZ konumuyla mesh başlangıç koordinatı farkı örneklemede çözülür.
- Açık turkuaz sığlık → orta turkuaz → derin mavi geçişi geniş bir derinlik aralığına yayılır. Rengi belirleyen taban derinliği komşu örneklerle yumuşatılır; köpük, dalga ve fizik kesin derinliği kullanır. Yansıma kontrastı ve üst üste binen sis suyu sert renk lekelerine bölmez (FB-004). Palet günün ışığıyla değişir.
- Geniş yumuşak güneş parlaması; küçük beyaz noktalar ve yoğun yansımalar yüzeye hâkim olmaz.
- Tepe köpüğü yatay yüzey dönüşümünün gerçek Jacobian determinantından çıkar; seyrek kalır ve mesafeyle azalır. Ada collider'larından fırınlanan deniz tabanı dokusu kıyı sönümünü, sığlığı ve hareketli kıyı köpüğünü besler.
- Derinlik soğurması, sınırlı sığ su kırılması, Fresnel ve tepe arka ışığı kullanılır. FB-005 ile 1024² planar kıyı yansıması eklendi; dalga normaliyle hafif bozulur, mavi süzme ve yumuşak kenar geçişiyle yüzeye bağlanır. Gökyüzü/bulut yansıması doku dışında devam eder. SSR yoktur. Sakin hava dalga ölçeği .42; fırtınada 1.65, CPU/GPU ortaktır.
- Hızla değişen, dönüşü takip eden ve zamanla dağılan tekne izi. Durgun teknede küçük hareket; aşırı sıçrama/parçacık yok.
- Dalga, kıyı köpüğü ve iz aynı hareketi destekler. Su dikişi, köpük tekrarı, titreşim ve trail kopması kontrol edilir.

Öncelik renk, hareket ve tekne/kıyı ilişkisi. Kullanıcı “daha canlı” derse bunu kendiliğinden “daha gerçekçi” diye yorumlama.

## Tekne ve hareket hissi

Güncel model yaklaşık 5,2 m kabinli yelkenli: bordo gövde, krem üst borda/yelken, ayrı güverte kaplamaları, pencere ve lombozlar, oturma alanı, usturmaça, can simidi, metal donanım ve arma. Blender kaynağı `ArtSource/JustSail_ReferenceAssets.blend`. Rigid parçalar materyal paylaşacak şekilde birleştirilir; yelkenler hareket için ayrı kalır. Huş taçları birkaç dal kütlesi ve ince yaprak geometrisinden, çamlar daralan dal katmanlarından oluşur.

- Altı probda suya gömülen hacim ve suya göre hızdan kaldırma/sönüm kuvveti hesaplanır. Ön/arka ve sağ/sol dalgaya ayrı tepki verir; ortak CPU/GPU dalga alanı yatay displacement terslenerek örneklenir.
- İleri atalet korunur; yanal su direnci yana kaymayı sınırlar. Düşük hızda dümen kontrolü kalır.
- Rüzgâr/yelken tepkisi hafif; erişilebilir kontrolün kuralları product.md'dedir.
- Tekne yatışı görünür, kamera ufku aynı ölçüde sallanmaz. Fizik sabit adım, render interpolasyonu ve geç kamera takibiyle çalışır.
- Dinamik teknede basit/compound collider; kıyı temasında ceza yerine kurtarma olanağı. Kuvvet ve sönüm değerleri Inspector'dan ayarlanır, ölçek görülmeden kesinleştirilmez.

## Ada kompozisyonları

| Ada | Silüet ve yaklaşım |
| --- | --- |
| Sakin Koy | Sonbahar otları, yakın huş/çam grupları, su hattında büyük kaya kümeleri, yamaç çıkıntıları ve sürüklenmiş dallar. İskeleye açık yaklaşma boşluğu korunur. |
| Fener Kayalığı | Daha yüksek/asimetrik gri kaya, çam kuşağı ve tepede fener. Diğer adadan uzaktan ayrılır; iç mekân yok. |

İlk deneme uzaklığı 400–700 m; hız, sis ve ölçülen seyir süresiyle ayarlanır. Bir adanın arkasından çıkarken diğerinin açılması hedeflenir. Bunlar tasarlanmış ilk sahnelerdir; prosedürel üretim iddiası yoktur.

## Işık ve kamera

| Deneme ışığı | Hedef |
| --- | --- |
| Öğle | Açık/dengeli; yelken ve köpük patlamıyor |
| Kapalı, hafif sisli | Soluk yeşil-mavi; kıyı okunuyor, her şey griye dönmüyor |
| Akşam | Sıcak kayalar, serin gölgeler; tekne ve koy girişi görünür |

Ana güneş, ortam ışığı, kontrollü gölge ve mesafe sisiyle başla. URP'de ek hacimsel sis gereksinimini sahnede kanıtlamadan paket ekleme. Tonemapping palete göre; bloom düşük. Film grain, chromatic aberration, motion blur ve güçlü vignette başlangıçta kapalı.

M2 atmosferi 24 dakikalık döngüde şafak, gündüz, akşam ve ay ışığını geçirir; gökyüzü ve su paletleri birlikte değişir. Bulutlar rüzgârla hareket eden hacim örneklemesidir. Gezici fırtınanın mesafesi dalga büyüklüğünü, sis, yağmur, ışık, yüzey ıslaklığı ve ambiyansı yumuşakça etkiler. Sis URP mesafe sisidir; hacimsel sis/god ray uygulanmış sayılmaz. Yüksek kalitede dört gölge kaskadı 15/50/180/600 m aralıklarını hedefler; düşük kalitelerde menzil kısalır. Hasar, su alma ve pusula sapması yoktur.

34° perspektif takip kamerası, başlangıç uzaklığı 40 m, eğim 5°; yakın kıyı/tekne ilişkisini gösterir. Tekerlek aralığı 22–55 m; fareyle yüksek diorama veya alçak gökyüzü görünümü seçilebilir. Karşılaştırmada direk ve su hattı kesilmez. Serbest bakıştan dönüş öngörülebilir; yakın kıyıda geometri içine girmez.

Neutral tonemapping, kontrollü kontrast ve sıcak vurgu/serin gölge kullanılır. Görsel karşılaştırmalar odak bulanıklığı kapalı yapılır; C ile isteğe bağlı hafif Bokeh açılır, eski kullanıcı tercihi korunur. URP SSAO temas derinliği sağlar; HBAO paketi kullanılmaz. Yakın kıyıyı soldurmayan analitik sis uzaklığı ayırır; gerçek hacimsel Buto/god ray değildir. Gökyüzü ve su aynı zümrüt/lavanta aurora alanını paylaşır; açık gecelerde görülür, fırtına/kapalı havada söner; üç gecelik dizide bir gece görünmez.

Adalar asimetrik yükselti ve geniş, eğim/yükseklik bazlı renk alanlarıyla üretilir. Fotoğraf dokusu eklenmez. Tekne ve çevre materyallerinde yerel yüksekliğe bağlı sade renk geçişi; ağaçlarda dalga saati ve rüzgârla tutarlı kökten salınım kullanılır. Dokuz martı ve üç yunus özgün prosedürel mesh'lerdir; kuşlar süzülür/kanat çırpar, yunuslar yakında seyreden tekneye eşlik edip su yüzeyine çıkar.

## Keşif etkileşimleri

F vizörü açar; sol tık/Space gerçek oyun karesini JPEG olarak kaydeder. Konum, oyun saati ve kadrajda görülen canlı türü notuyla krem çerçeveli bir kart belirir. Tab veya pause menüsü seyir defterini açar; sayfalı kartlardan fotoğraf büyütülür. Albüm seyri durdurur; fotoğraf çekimi durdurmaz. Fotoğraflar uygulamanın yerel kayıt dizininde oturumlar arasında korunur. Tür etiketi görüş hacmi/mesafe kontrolüdür; tam görüntü tanıma veya kusursuz engel kontrolü iddiası yoktur. Fotoğrafa puan, zorunlu koleksiyon veya yükseltme ödülü bağlanmaz.

İskeleye yavaş yaklaşınca E ile palamar bağlanır; aynı tuş çözer. Fizik SpringJoint üzerinden, görünen sarkık halat ayrı çizilir; Obi'nin parçacık/halat-çevre çarpışması simülasyonu değildir. Bağlı tekne dalgada serbestçe yüzer; kurtarma halatı bırakır. Gece bağlıyken R ile üç saniyelik gökyüzü geçişinde sabah beklenebilir. Karaya çıkma veya kamp kurma gerektirmez.

## UI ve ses

uGUI Canvas + TextMeshPro; ortak stilli az sayıda öğe, ürün UI'sında OnGUI yok. Ekranlar ve düğme işlevleri [product.md](product.md) içinde tanımlıdır.

- Kırık beyaz metin, koyu petrol panel, küçük sıcak vurgu; okunaklı sans serif. Türkçe karakter atlası doğrulanır.
- Menülerde yeterli panel kontrastı; normal metin için en az 4,5:1 hedefi ölçülür. Durum yalnızca renkle anlatılmaz.
- Hover/basılı/seçili/devre dışı durumları ayrıdır. Dekoratif grafik input'u engellemez. UI manzarayı kaplamaz.
- Gerçek yükleme durumu; sahte yüzde yok. Kontrol yardımı menüden yeniden okunabilir. Ölçek/etkileşim kontrolü management.md'dedir.
- Deniz, hızla ilişkili su kesme, hafif rüzgâr/ahşap/yelken; iskelede seyrek martı/çevre sesleri. Kısa döngüler belli olmaz; UI sesi baskın değildir.
- Pause'ta ambiyans kısılır/durur, menü sesleri duyulur. Ana ses hepsini etkiler. İlk prototip müziksiz çalışabilir; lisansı belirsiz ses/müzik eklenmez.

## Asset üretimi

1 Unity birimi = 1 m; tekne burnu Unity'de +Z. Blender kaynakları/betikleri saklanır; FBX başlangıç dışa aktarım tercihidir. Ölçek, pivot, yön, normaller ve materyal eşlemesi Unity içinde kontrol edilir. Blender prosedürel shader'larının aynen taşındığı varsayılmaz; malzemeler URP'de kurulur. LOD/instancing tekrar sayısı ve ölçüme göre eklenir.

Görsel teslim kontrolleri [management.md](management.md), kullanıcı yorumları [feedback.md](feedback.md) içindedir.

## Araştırma kaynakları

[Over the Hill](https://www.funselektor.com/overthehill), [Sail Forth](https://store.steampowered.com/app/1031460/Sail_Forth/?l=english), [Daniel Ilett: stilize su](https://danielilett.com/2020-04-05-tut5-3-urp-stylised-water/), [Catlike Coding: Waves](https://catlikecoding.com/unity/tutorials/flow/waves/), [Steven Smith: su katmanları](https://www.stevensmith-art.com/blog/vertex-color-blending-in-unreal-5-rmbyz), [Red Blob: ada üretimi](https://www.redblobgames.com/maps/terrain-from-noise/), [Unity 6.6 Manual](https://docs.unity.com/en-us/engine/6000.6/manual).

Eski Unity/Unreal örnekleri kavram kaynağıdır; 6.6 API'si olarak kopyalanmaz. Video/transkriptlere erişilemedi; izlenmiş sayılmazlar.
