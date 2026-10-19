# Just Sail — Üretim Yönetimi

Sürüm: 0.7 · 19 Ekim 2026 · Aşama: Askıya alındı — 19 Ekim 2026

## Belgeleri kullanma

| Bilgi | Tek kaynak |
| --- | --- |
| Oyun mantığı, kapsam, kontroller | [product.md](product.md) |
| Görsel hedef, su/fizik hissi, kamera, ses, UI stili | [design.md](design.md) |
| İş sırası, araçlar, doğrulama ve güncel durum | Bu dosya |
| Kullanıcı yorumları, düzeltmeler ve dersler | [feedback.md](feedback.md) |

- İşe başlarken güncel durumu ve görevle ilgili açık feedback kayıtlarını kontrol et. İlgili kararlar bağlamda zaten varsa değişmedikleri sürece tekrar okuma.
- Gerektiğinde başlık/anahtar kelime ara; yalnızca ilgili bölümü oku. Su işi için tüm ürün ve yönetim geçmişini yükleme. Tam okuma yeni bağlamda genel yönü öğrenmek veya kapsamlı belge düzenlemek için uygundur.
- Aynı kararı başka belgelere kopyalamak yerine kaynağına bağlantı ver. Bilgiyi başka arşivlere bölerek dosya sayısını büyütme.
- Belgeleri yalnızca karar, teslim durumu veya önemli feedback değiştiğinde güncelle. Her küçük kod değişikliğine belge raporu yazma.
- Kısa feedback yeterlidir; ilgisiz ve kapanmış geçmişi her görevde okuma. Süreç değişikliği: FB-002.

## Çalışma biçimi

Kullanıcı yaratıcı yönü ve oynayış hissini değerlendirir; Codex üretimi, araştırmayı, doğrulamayı ve gerekli belge güncellemelerini yürütür. Açık talimatları mevcut yetki kapsamında uygula; rutin geri alınabilir adımlar için tekrar onay isteme. Belirsiz veya kapsamı büyüten kararlarda somut seçenek sun. Gerekli küçük ve bağımsız işlerde GPT-6 Luna subagent kullanılabilir; çıkan iş ana yürütücü tarafından incelenir.

Önce küçük sahnede sonucu gör, sonra çoğalt. Planlanan, uygulanan, teknik olarak doğrulanan ve kullanıcı tarafından kabul edilen durumları ayır. Teslimde sonucu, kontrolleri, kalan sorunları ve sıradaki işi kısa bildir.

## Araçlar ve yöntem

| Araç | Doğrulanmış durum |
| --- | --- |
| Unity Hub | 3.22.3 kurulu |
| Unity Editor | 6000.6.6f1; proje, Personal lisans ve Windows derlemesi doğrulandı |
| Blender | 5.2.2 LTS; özgün FBX/.blend üretimi ve kaynak dosyasının UI'da açılması doğrulandı |
| Visual Studio Community | 2022 / 17.14.41 kurulu; Unity iş yükü bulunamadı |
| Git | Kullanıcının 19 Ekim 2026 talebiyle kaynaklar SePudra/just_sail reposuna yükleniyor; sonraki Git işlemleri görev kapsamında yapılır |

C# / HLSL ve desteklenen Editor API'leri; model üretiminde Blender Python/bpy, uygulama kontrolünde gerektiğinde Computer Use kullanılıyor. Yerel Unity CLI `Tools/UnityCLI/bin/unity.exe` (1.0.0-beta.13), Pipeline 0.8.0-exp.1 ile sahne düzenleme, derleme ve Play Mode doğrulaması yapıldı. URP 17.6.0, Input System 1.20.0, uGUI 2.6.0. Luna'nın ortam sesi ve küçük inceleme çıktıları ana yürütücü tarafından kontrol edildi.

### Kurulu beceriler

`find-skills` ile unity/blender/shader aramaları ve kaynak incelemesi yapıldı; 16 Ekim 2026'da yedi beceri `C:/Users/badur/.codex/skills/` altına kuruldu:

- [Unity-Technologies/skills](https://github.com/Unity-Technologies/skills), commit `9d282100540601cfddc4bbd69f36a239d7a6f55c`: `unity-cli`, `new-unity-project`, `urp-postprocessing`, `ui-ugui`.
- [gamedev-skills/awesome-gamedev-agent-skills](https://github.com/gamedev-skills/awesome-gamedev-agent-skills), commit `0a70cfc64672d512c5b5b6006b5dcb5cf90dbdbd`: `unity-csharp-scripting`, `unity-physics`, `shader-programming`.

Topluluk örnekleri Unity 6.3 tabanlı; 6.6 uyumu uygulamada doğrulanır. Beceri varsayımları kullanıcı kararlarının yerine geçmez. Mevcut `physics-tuning`, `computer-use:computer-use` ve `ponytail` gerektiğinde okunur. Blender adayları MCP bağımlılıkları/eski API örnekleri nedeniyle kurulmadı. Unity paketi veya Blender eklentisi bu hazırlıkta eklenmedi.

## Aşamalar ve güncel durum

| Aşama | İş | Durum |
| --- | --- | --- |
| M0 | Beceri kurulumu ve dört belge | Tamamlandı; FB-002 ile sadeleştirildi |
| M1 | İlk oynanabilir prototip | Teknik kontroller geçti; kullanıcı görsel sonucu reddetti (FB-003) |
| M2 | SoT su, yüzdürme ve atmosfer uyarlaması | Uygulandı; 17 yeni Windows kontrolü geçti, görsel kabul yok |
| M2b | Over the Hill görsel dil ve keşif uyarlaması | Uygulandı; 22 keşif/görsel işlev ve 17 su/fizik kontrolü geçti, kullanıcı kabulü yok |
| M2c | Dört introdan kare seçimi, sabit kadraj karşılaştırması, tekne/çevre revizyonu | On ana görsel tur ve AO düzeltmesi; son Windows paketinde 17 kontrol geçti; görsel farklar/kabul FB-005'te açık |
| M3 | Sail Point, ikinci tekne, iskele ustası, yolculuk kaydı | Sonraki kapsam |
| M4 | Seed tabanlı ada aileleri, dünya ve tekne çeşitliliği | Sonraki kapsam |

M1 iş sırası: URP projesi → ilk koy, tekne ve su → ortak dalga/yüzdürme, kontrol ve kamera → ikinci ada/iskele/ses → çalışan UI → görsel kontrol ve Windows build. Kapsamın tamamı product.md'dedir; bu sıra teslimden özellik çıkarmak için kullanılmaz.

M1 sonrasında önce kullanıcı geri bildirimleri ele alınır. Prototip değerlendirilmeden büyük ada/tekne koleksiyonu üretimine geçilmez. Streaming/origin shifting gibi ek sistemler ihtiyaç ölçülünce değerlendirilir.

**Önceki M2c işi:** FB-005 kapsamında kullanıcının dört intro videosundan görsel hedef çıkarılıyor; kareler, kaynak zamanları ve tur başına karşılaştırmalar `Verification/VisualStudy/` içinde. Gerçek Unity render'ları kullanılır; kurgulu video görüntüsü oynanış davranışı diye kabul edilmez. Güncel tekne/bitki/kaya kaynağı `ArtSource/JustSail_ReferenceAssets.blend`, sahne uygulayıcı `ReferenceArtPass.cs`. Görsel kabul kullanıcıya aittir.

### M2d — 19 Ekim 2026 bulut revizyonu

- Güncel kullanıcı isteği FB-006: oynanış eklemeden sanat yönü ve fizik/hareketi iyileştirme. Palet ve uygulanan davranışlar design.md'de.
- Mevcut `reference-03.png` / `reference-08.png` Unity kareleri incelendi. Kaynak tekne/çevre modelleri Blender 4.3.2 ile yeniden üretildi; yakın plan incelemesinde bulunan kıç kaplama açıklığı gövde köşeleriyle eşleştirilerek kapatıldı; FBX GUID'leri korundu, altı modelin malzemeleri adlarıyla eşlendi. 1.826 eski materyal sıra geçersiz kılması sahneden kaldırıldı; sahnenin 2.978 YAML belgesi ve materyal GUID hedefleri kontrol edildi.
- Unity 6000.6.6f1 Linux Editor buluta kuruldu; açılış **“No valid Unity Editor license found”**, çıkış 198 ile durdu. Önceki Windows makinesindeki lisans/performans sonuçları bu değişikliklere ait değildir. Bu turda yeni Unity render, sahne importu, Windows build veya gerçek PhysX sürüşü doğrulanmadı.
- Oyun ve Editor C# kaynakları ayrı assembly olarak gerçek Unity 6.6 / paket kütüphaneleriyle derlendi. Eski `FindFirstObjectByType` kullanımından deprecation uyarıları var. Altı shader'ın 20 vertex/fragment aşaması ve gölgeli varyantı DXC ile URP 17.6.0 include dosyalarına karşı derlendi; bu kontrol Unity shader importu / tüm platform varyantları değildir.
- Gerçek `SeaSurface.Evaluate` / `SampleSurface` metotları, açık su için üç dalga şiddetinde 2.100 noktada çalıştırıldı: ters örnekleme en fazla 0,000316 m; analitik normal sonlu fark sapması 0,00111; hız sapması 0,00402 m/s; en küçük determinant 0,4178. Seabed/PhysX/GPU çalışma zamanı testi sayılmaz.
- `Verification/CoastalPass/SourceChecks/report.json` ve loglar yeniden üretilebilir kaynak kanıtıdır. `boat-source.png` ve `shore-models-source.png` Blender model incelemeleridir; Unity oyun görüntüsü değildir. Yardımcı kıyı önizlemesi de Unity shader, ot ve render zincirini temsil etmez.
- Tekrar: `python ArtSource/verify_coastal.py --unity-data <Editor/Data> --dxc <dxc>`; model kontrolü: `blender -b --python ArtSource/preview_assets.py`. İndirilmiş araçlar kaynak projenin dışında tutulur.
- Lisanslı Unity'de `FirstVoyage` → Play → Denize Açıl → **Just Sail / Review / Capture coastal art** beş sabit kadrajı `Verification/CoastalPass/Unity` altına yazar. **Run ocean and buoyancy checks** mevcut gerçek oyun testini başlatır. Sonraki kontrol: 30/60/120 FPS'te salınım/dümen, fırtına ve pause/devam; yakın kıyıda kamera; yeni FBX import ve materyalleri; yansımanın ek gölge maliyeti. Bunlar yapılmış sayılmaz.

### M2c son doğrulama

- `Verification/VisualStudy/windows-report.json`: son release Windows paketinde 17/17, runtime hata 0; yağmur kontrolünde 2.280 canlı parçacık. 1600×900, RTX 3070 Ti Laptop GPU, 27,70 aktif saniye / 1.618 kare / ortalama 58,41 FPS; genel performans garantisi değildir.
- CPU/GPU yüzey konumu farkı en fazla 1,06 mm; ters yatay örnekleme 0,32 mm. Yeni araziye göre deniz tabanı tekrar fırınlandı; suyun ortak dalga çekirdeği korundu.
- İlk koşunun 16/17 raporu `windows-first-pass.json` olarak saklandı. Yağmur culling düzeltildi; opt-in senaryo kullanıcı klavyesinden/odak değişiminden ayrıldı. Normal oyunun pencere değişiminde duraklaması korundu. Son koşuda odak nedeniyle otomatik devam sayısı 0.
- `sailing.gif`: zaman damgalarıyla 48 gerçek Windows karesi, 5,43 s. Sabah karşılaştırması, geniş açı, akşam/gece ve tekne yakın planı aynı klasörde. Görsel benzerlik yüzdesi ölçülmedi, birebir sonuç/kullanıcı kabulü yok.
- `Verification/REA/` artık bu paketin su regresyonudur; önceki rapor `Verification/VisualStudy/baseline/previous-ocean-report.json`, M2b kopyası `Verification/OverTheHill/ocean-regression.json` içinde korunur. Normal inceleme oyunu açık; Git işlemi yapılmadı.

### M2b kapsam ve kaynak eşlemesi

`rea_info/over_the_hill/00`–`08` arasındaki dokuz raporun tamamı okundu. Raporlardaki paket/performans iddiaları bağımsız doğrulanmış bilgi sayılmadı; yerel Unity/URP kaynaklarıyla kendi uygulamamız geliştirildi. REA ve oyun ikilileri yeniden analiz edilmedi; ücretli paket/asset eklenmedi.

| Kaynak | Uyarlama |
| --- | --- |
| 00–01, görsel dil | 34° kamera, 38 m başlangıç mesafesi; tekneye bağlı ve kapatılabilir odak, sade geometri/renk alanları |
| 02, render | URP SSAO, Neutral, split toning, lifted blacks, düşük bloom/vignette; `OverTheHillAdaptation.cs` sahne kurucusu |
| 03, atmosfer | Deniz seviyesinde yoğunlaşan analitik sis, gökyüzü/su/ortam ışığında aurora; bağlıyken isteğe bağlı sabah geçişi |
| 04, çevre | Asimetrik ada yükseltileri, eğim/yükseklik renk geçişi, `Crafted.shader`, `TreeBreeze.cs` |
| 05, su | Mevcut SoT dalgası/yüzdürme korundu; aynı stil ve sis içinde kıyı derinliği/köpük/kostik bağlantısı |
| 06, fizik | `MooringLine.cs`: yavaş yaklaşma, SpringJoint ile palamar, görünür sarkma, çözme ve kurtarmada bırakma |
| 07–08, keşif | `WildlifeActor.cs`: dokuz martı/üç yunus; `VoyageJournal.cs`: gerçek fotoğraf, konum/saat/tür notu, kart/büyük görüntü, sayfalı kalıcı albüm |

Uyarlama tercihleri: kara aracı/süspansiyon/vinç, on-foot kamp ve fotoğraf puanı deniz keşfine taşınmadı. Fotoğraflar puansız kişisel kayıttır; NPC/tekne yükseltmesi eklenmedi. COZY/Buto/HBAO/Obi/FMOD/MicroVerse/TVE/Impostors/GPU Resident Drawer paketleri kullanılmıyor. Sis analitik yükseklik sisi, palamar tek eklemli fizik ve görsel eğridir; tam hacimsel saçılım veya parçacık halatı değildir. İki küçük ada için milyonlarca nesne/120 FPS iddiası yoktur.

İlk Editor kontrolü 20 maddede geçti, ancak görüntüde kamera kırpması ve geniş ekran defter taşması bulundu. Kadraj 10° başlangıç eğimiyle; defter, ekranı dolduran zemin ve ortalanan/ekrana sığan içerikle düzeltildi. Bu ilk rapor son teslim kanıtı değildir. Windows son kontrolü yüksek görüntü kalitesinde çalışır ve sonunda önceki kullanıcı ayarını geri yükler. Kanıtlar `Verification/OverTheHill/`, su regresyonu `Verification/REA/` içindedir.

### M2b doğrulama kaydı

- Son Windows release paketi 17:05'te derlendi. `Verification/OverTheHill/windows-report.json`: 22 kontrol geçti; kamera/odak, yaşayan çevre, aurora/hava, gerçek JPEG ve diskten albüm yükleme, karttan büyük görüntü, ekran sınırları, pause/devam, fizik palamarı, kurtarma ve sabahla birlikte gece takviminin ilerlemesi. Runtime hata yok.
- Son koşu 21,55 aktif saniye / 1.265 kare, ortalama 58,69 FPS. 1600×900, Yüksek kalite/renderScale 1,15, odak açık; RTX 3070 Ti Laptop GPU. Görüntü kaydı da içeren kısa ölçümdür; minimum FPS veya uzun süre kararlılığı kanıtı değildir.
- Aynı son pakette `ocean-regression.json`: 17 su/fizik kontrolü geçti, hata yok. Ortak CPU/GPU dalgası, gerçek ada tabanı, altı prob, seyir, pause, gün/gece, yağmur, gezici fırtına ve yüzdürme sınırları yeniden doğrulandı.
- `daylight.png`, `dawn.png`, `golden-hour.png`, `aurora.png`, `storm.png`, `viewfinder.png`, `photo-card.png`, `album.png`, `photo-detail.png`, `mooring.png` gerçek Windows kareleridir. `Motion/` 48 kare/zaman damgası, `sailing.gif` gerçek hareket önizlemesidir. `AlbumCheck/` yalnızca test albümüdür; oyuncunun yerel albümüne test fotoğrafı eklenmedi.
- Görüntüler incelenerek kadraj, defter taşması, farklı fotoğraf oranlarının hizası ve büyük fotoğrafta alttaki UI'nın görünmesi düzeltildi. UI testleri gerçek uGUI callback'lerini çalıştırır; tüm fare/klavye yollarının insan tarafından oynandığı iddiası yoktur. Son Player logunda yalnızca kullanılmayan Panini shader'ının stripping uyarısı var; Bokeh kullanılıyor ve görünür.
- Görsel kalite ve oynayış keyfi hâlâ kullanıcı kabulünü bekliyor. README/product/design ve FB-003 güncellendi; Git işlemi yapılmadı.

### M2 kapsam ve kaynak eşlemesi

`rea_info/00`–`05` arasındaki altı SoT belgesinin tamamı okundu. Raporlar doğrudan doğrulanabilir REA kanıt kimlikleri/adresleri içermediğinden teknik öneri olarak kullanıldı; SoT'un özgün kodunun birebir çözüldüğü veya aktarıldığı iddia edilmiyor. REA tekrar çalıştırılmadı.

| Kaynak | Unity uyarlaması |
| --- | --- |
| 00 — Mimari | Ortak dalga saati ve CPU/GPU parametre tablosu; mevcut URP projesinde bağımsız sistemler |
| 01 — Su | `SeaSurface.cs` / `OceanWaves.hlsl`: altı Gerstner dalgası, analitik normal/hız, ters yatay örnekleme, gerçek determinant; `OceanBake.cs`: ada tabanı ve kıyı sönümü; `Sea.shader`: soğurma, Fresnel, köpük/iz |
| 02 — Yüzdürme | `BoatController.cs`: altı prob, hacim bazlı kaldırma, suya göre sönüm, dalga sürüklemesi; rüzgâra tepki veren yelken ve baş serpintisi |
| 03 — Işık | Dört URP gölge kaskadı; sığlıkta hareketli kostik, tepede arka ışık ve ışığa göre dalga kontrastı |
| 04 — Hava | `VoyageAtmosphere.cs` / `Atmosphere.hlsl`: 24 dakika, güneş/ay/yıldız, hacim örneklemeli bulut, gezici fırtına, yağmur/ıslaklık/ışık/rüzgâr ve ses bağlantısı |
| 05 — Rehber | Altı maddelik çekirdek yol haritası uygulandı; mevcut el yazımı URP shader'ına bağlandı. Ayarlarda gün ve fırtına kontrolü var |

Rehberdeki yükseklikten türetilmiş sahte Jacobian gerçek determinantla değiştirildi; yatay Gerstner kayması dünya probunda tersleniyor. CPU deniz tabanı örneklemesi GPU'nun texel merkezli bilinear koordinatlarına eşitlendi. İlk tanı koşusu 9 cm CPU/GPU farkı bulmuştu; o rapor geçerli teslim kanıtı değildir.

M2 teslimindeki sınırlar: sahne tek oyunculu ve su üstündedir; ağ senkronizasyonu, sintine/hasar/batma, pusula sapması ve su altı kamera efektleri ürün kapsamına alınmadı. FFT, kıyıya dalga yönü kırılması, kilometrelik SDF gölgeleri, ekran uzayı temas gölgesi/SSAO, SSR, su birikintisi ve lens damlası bu aşamada uygulanmadı. M2b'de eklenen SSAO ve diğer Over the Hill değişiklikleri yukarıdaki kayıttadır; kalan teknikler uygulanmış sayılmaz.

M2'nin önceki kanıtları `Verification/OverTheHill/M2-archive/` altında korundu. `Verification/REA/` artık M2b sonrasındaki su regresyonudur; özeti `Verification/OverTheHill/ocean-regression.json` içine kopyalanır. M1 ölçümleri yeni sistemlere mal edilmez.

### M2 doğrulama kaydı

- Windows release build 16:26'da tamamlandı; yeni veri dosyalarıyla Editor dışında çalıştırıldı. `windows-report.json`: 17 kontrol geçti, runtime hata yok. CPU/GPU konum farkı en çok 0,000211 m, normal farkı 0,000297; üç dalga şiddetinde 192'şer örnek. Ters örnekleme hatası 0,000322 m altında, örneklenen en küçük determinant 0,477.
- Altı prob, gerçek ada collider'ından derinlik, seyir itişi, pause, gece yarısı geçişi, ay ışığı, fırtına dalgası/ıslaklığı, canlı yağmur ve gezici fırtınanın mekânsal etkisi doğrulandı. 27,36 saniye / 1.612 aktif kare; ortalama 58,93 FPS, yükseklik −1,46…1,76 m, en fazla yatış 19,05°. RTX 3070 Ti Laptop GPU, 1600×900. Kısa yakalama koşusudur; uzun süreli veya bütün donanımlar için performans kanıtı değildir. Kalite seviyesi raporda kaydedilmedi.
- `ui-check.json`: Editor'de gerçek UI callback'leriyle fırtına gücü, gün döngüsü, dört hava ve dört ışık seçeneği, üç kalite ve yüksek kalitenin 15/50/180/600 m kaskadları geçti. `settings-editor.png`: 1536×672 Game View'da tüm panel görünür. Yeni panelin Windows'ta fareyle tam etkileşim testi ayrıca yapılmış sayılmaz; Windows ana menüsü görüntülendi.
- `daylight.png`, `dawn.png`, `sunset.png`, `moonlight.png`, `storm.png` gerçek Windows kareleridir. Gün batımının erken geceye dönüşmesi düzeltildi; son karşılaştırma kareleri incelendi. `Motion/` 48 gerçek kare ve zaman damgaları; `sailing.gif` bunlardan 5,27 saniyelik önizlemedir (`make_preview.py`).
- Kullanılmayan DoF/Panini efektlerinin URP stripping uyarıları mevcut; bu efektler eklenmedi. Yeni sistem için 10 dakikalık test iddiası yok. Teknik sonuç kullanıcı görsel kabulü yerine geçmez.

### M1 doğrulama kaydı

- `Verification/editor-run.json`: gerçek Play Mode'da 16 kontrol geçti; sürüş/dönüş, pause, ayarlar, kurtarma ve ada collider'ı. 1.039 kare / 17,3 saniye; yükseklik −0,47…0,14 m, en fazla yatış 5,25°.
- Release Windows build üretildi (`JUST_SAIL_BUILD Succeeded 0`). Menü, yelkenli, iki ada ve farklı çözünürlüklerde ayarlar oyun içinden görüntülendi. Çözünürlük etiketi/klavye odağı, HUD kontrastı ve iskele temas hacmi düzeltildi; son build yeniden sınandı.
- `Verification/Standalone/report.json`: 19 kontrol geçti, runtime hata sayısı 0. 600,08 saniye / 37.801 kare; dalga zamanı 600,07 saniye ilerledi. Yükseklik −0,62…0,24 m, en fazla yatış 5,85°. Release build, Yüksek kalite, 1600×900, NVIDIA GeForce RTX 3070 Ti Laptop GPU; 30/120/60 FPS hedefli bölümlerin toplam ortalaması 62,99 FPS. Bu, tek donanım/açık su ölçümüdür; genel minimum FPS iddiası değildir.
- Test aracı açık su rotasını 3 kez yeniden konumlandırdı; oyun kurtarması kullanılmadı. Son koşuda odak nedeniyle duraklama olmadı. Önceki duraklayan ölçüm teslim kanıtı sayılmadı; test yalnızca en az 600 saniye dalga simülasyonu ilerlerse geçer (`stability-notes.txt`).
- `Verification/Standalone/restart-check.txt`: 7 kontrol geçti; ses, kamera, kalite ve ekran ayarları uygulama yeniden açılınca korundu. Ana menü → seyir → pause → gerçek uygulama çıkışı çalıştı. Son görüntüler `release-menu.png` ve `release-sailing.png`.
- Windows işlev ön testi: 15 kontrol geçti; çözünürlük onayı/geri alma/zaman aşımı, tam ekran, fare orbit/zoom ve gerçek kıyı/iskele çarpışması dahil (`Verification/Standalone/preflight.txt`). UI görüntüleri 1280×720, 1600×900, 2560×1080 ve Windows'un pencere boyutunu sınırladığı 2560×1230'da incelendi; 2560×1440 tam boyut iddiası yok.
- UI temel renklerinin hesaplanan kontrastı: krem/koyu panel 12,47:1; ikincil metin/panel 8,12:1. Oyun içi Menü düğmesine açık gökyüzünde okunması için koyu zemin eklendi. Kullanılmayan DoF/Panini shader'ları için URP stripping uyarıları var; bu efektler sahnede kullanılmıyor.
- Görsel yön ve ses/sürüş keyfi kullanıcı kabulü bekliyor. Teknik başarı bunların onayı sayılmaz.

## Teslim kontrolü

- Editor ve shader derlemesi temiz; Windows build Editor dışında açılıyor.
- Product.md'deki M1 özellikleri gerçek işlevleriyle mevcut; işlevsiz düğme yok.
- Fizik farklı kare hızlarında, durgun suda, dönüşte ve kıyı/iskele temasında sınanıyor; takılınca kurtarma çalışıyor.
- Pause/odak kaybı fizik ve ortak dalga zamanını durduruyor; devamda sıçrama veya takılı girdi yok. Menü tekneyi kontrol etmiyor.
- Ayarlar etkili ve yeniden açılışta korunuyor; ana menüye dönüş ve build'den çıkış çalışıyor.
- Aynı oyun kameralarıyla öğle, kapalı/sisli ve akşam görünüşleri inceleniyor: açık su, koya yaklaşma, iskele, iki ada ve menü. Hareketli görüntüde jitter, su dikişi, köpük tekrarı ve trail kopması kontrol ediliyor.
- Türkçe karakterler, UI kontrastı, 1280×720 / 1920×1080 / 2560×1440 ve ultrawide yerleşimi kontrol ediliyor.
- Test cihazı, çözünürlük, build türü ve FPS/frame time kaydediliyor. Ölçülmeyen performans başarılı sayılmıyor.

Görsel kaliteyi derlenme sonucu yerine oyun içi görüntü ve oynama ile değerlendir. Otomatik testleri ortak dalga örnekleme, pause veya ayar kaydı gibi hataya açık davranışlara gerektiğinde ekle; düşük etkili değişikliklere biçimsel test yazma.

## Kaynak ve geri dönüş

Unity kaynakları `JustSail/Assets/JustSail/`, Blender dosyaları/betikleri `ArtSource/`, çalıştırılabilir oyun `Builds/JustSail/` altında. Açılış ve kontroller [README.md](README.md) içinde.

- `.meta` dosyalarını assetlerle tut; türetilmiş klasörleri kaynaklardan ayır.
- Kullanıcının son talimatı: Git tamamen kullanıcıya aittir. Git komutu çalıştırma, depo başlatma, commit, stage, LFS veya Git ayarı yapma.
- Üretim betikleri kendi çıktılarını yeniler; kabul edilmiş kaynağı geri dönüşsüz ezme.
- Haricî assetin kaynak/lisansını kaydet; ücretli asset veya hizmet mevcut plana dahil değil.



### 19 Ekim 2026 — Tek klasörde birleştirme

`just-sail-improved/just_sail` kaynakları ana `C:/Projeler/just_sail` klasörüne aktarıldı. 438 kaynak dosyası SHA-256 ile doğrulandı; 35 değişmiş ve 7 yeni dosya korundu. Ana klasördeki REA, Tools, Verification ve Git verileri korundu. İki sürümün kaynak yedeği `Verification/Consolidation/before-consolidation.zip`; ayrıntı `merge-result.json`. Ana projede FirstVoyage açıldı, C# hatası yok, Play Mode kare/zaman ilerlemesi doğrulandı ve Windows build tamamlandı. Bu kısa kontrol tam fizik regresyonu veya görsel kabul değildir. Unity build sırasında URP runtime listesi üretti; testteki kalite tercihi etkisi geri alındı ve improved font kaynağı korundu. İkinci improved klasörü doğrulama sonrasında silindi; bundan sonra tek çalışma projesi `JustSail/`. GitHub bu birleştirmeyle güncellenmedi.

### Askıya alma — 19 Ekim 2026

Kullanıcı projeyi REA kapasite denemesi olarak başarılı buldu; görev, konuşma ve geniş dünya üretimine geçilmeden askıya alınmasını istedi. Improved birleştirmesi ve yansıma en-boy oranı düzeltmesi son kaynaklara dahildir. Kaynaklar ve REA raporları main dalında, tüm yerel proje dosyaları (build, araçlar, doğrulama/yedekler ve Unity önbelleği dahil) paused-2026-10-19 release arşivinde saklanır. GitHub kopyası doğrulandıktan sonra yerel proje klasörü kaldırılır. Kurulu Unity/Blender gibi ortak uygulamalar ve Downloads içindeki kullanıcı videoları proje klasörü dışında kaldığından silme kapsamına dahil değildir.
