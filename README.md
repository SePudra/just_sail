# Just Sail — İlk Yolculuk

**Durum: Askıya alındı — 19 Ekim 2026.** REA kapasitesini değerlendiren prototip çalışması tamamlandı; yeni oyun kapsamı planlanmıyor. Son su/yansıma düzeltmesi ve improved revizyonları bu kaynaklarda bulunur. Windows build ve bütün yerel proje dosyalarının arşivi: [Askıya alma arşivi](https://github.com/SePudra/just_sail/releases/tag/paused-2026-10-19). Arşivdeki tüm ZIP parçalarını aynı klasöre çıkart; her parça bağımsız ZIP dosyasıdır. `SHA256SUMS.json` dosya ve arşiv doğrulamasını içerir.

İki adalı görsel/his prototipi. Küçük bir yelkenliyle Sakin Koy ve Fener Kayalığı çevresinde dolaşılır. SoT'tan uyarlanan deniz/atmosferin yanında Over the Hill raporlarından geliştirilen dar açılı kamera, yumuşak odak, renk düzeni, rüzgârlı bitkiler, aurora, martılar/yunuslar, fotoğraf defteri ve palamar bulunur. Modeller özgün Blender/prosedürel üretimdir.

## Oynama

`Builds/JustSail/JustSail.exe` dosyasını aç; **Denize Açıl** düğmesine bas. Uygulamayı taşırken aynı klasördeki veri klasörü ve DLL'leri de birlikte taşı.

| Girdi | İşlev |
| --- | --- |
| W / S | Yelken gücünü artır / azalt; bırakınca hedef korunur |
| A / D | Dümen |
| Sağ fare + hareket | Çevreye bak |
| Tekerlek | Yaklaş / uzaklaş |
| Esc | Duraklat / devam et |
| F | Fotoğraf vizörünü aç / kapat |
| Sol tık / Space | Vizör açıkken fotoğraf çek |
| Tab | Fotoğraf defteri; kartlara tıklayarak büyüt |
| C | Yumuşak odak açık / kapalı |
| E | İskeleye yavaş ve yakınken palamar bağla / çöz |
| R | Gece palamar bağlıyken sabahı bekle |

Takılırsan pause menüsündeki **Tekneyi Kurtar** güvenli başlangıca döndürür. Başka pencereye geçince oyun duraklar. Ayarlardan ses, kamera hassasiyeti, görüntü kalitesi, çözünürlük ve ekran modu değişir.

Atmosferi karşılaştırmak için Ayarlar'da **Işık** (gündüz/şafak/akşam/gece), **Hava** (açık/kapalı/fırtına/gezici), **Gün döngüsü** ve **Fırtına gücü** bulunur. Normal gün 24 gerçek dakika sürer; menüde zaman durur. Atmosfer deneme ayarları oturumluk, diğer ayarlar kalıcıdır. Fırtına batma veya hasar getirmez.

Sağ fareyle alçaktan bakarak gece aurorasını görebilirsin. Fotoğraflar `%USERPROFILE%/AppData/LocalLow/Just Sail/Just Sail/SeyirDefteri/` altında tutulur. Albüm açıkken seyir durur; vizör açıkken tekne mevcut yelken gücüyle ilerlemeye devam eder. Fotoğraflara puan verilmez.

Bu sürüm iki adalı sınırlı bir sahnedir. Yaklaşık 1,8 km sınırının ötesinde tekne başlangıca döner. Sonsuz dünya, Sail Point, tekne yükseltme, NPC ve tekne konumu/ilerleme kaydı sonraki aşamadadır.

## 19 Ekim görsel / hareket revizyonu

Kıyı paleti, su yansıması/köpük, bitki/kaya modelleri, yelken bağlantıları ve tekne/kamera hareketi güncellendi. Değişiklikler mevcut `FirstVoyage` sahnesinde kullanılır; sahneyi yeniden oluşturmak gerekmez. Bulutta kaynak derlemesi ve sayısal kontroller yapıldı; Unity lisansı bulunmadığı için yeni oyun içi görsel/PhysX kontrolü ve Windows build bekliyor. Ayrıntılar [management.md](management.md#m2d--19-ekim-2026-bulut-revizyonu) içinde.

Lisanslı Unity'de Play → Denize Açıl sonrasında **Just Sail → Review → Capture coastal art** sabit sabah/öğle/akşam/gece/yakın tekne karelerini üretir. Aynı menüde **Run ocean and buoyancy checks** gerçek oyun testlerini başlatır.

## Üretim dosyaları

- Unity projesi: `JustSail/`, Unity **6000.6.6f1**, URP **17.6.0**.
- Sahne: `JustSail/Assets/JustSail/Scenes/FirstVoyage.unity`.
- Blender kaynağı: `ArtSource/JustSail_Assets.blend`; yeniden üretim: `ArtSource/make_assets.py`.
- Güncel tekne/çam/huş/kaya kaynakları: `ArtSource/JustSail_ReferenceAssets.blend`, `ArtSource/make_reference_assets.py`. Unity'de **Just Sail → Reference art → Apply autumn shore** güncel çevreyi uygular; mevcut sahne için tekrar çalıştırmak gerekmez.
- Kullanıcının dört tanıtım videosundan seçilen kareler ve yan yana iterasyonlar: `Verification/VisualStudy/`. Render'lar gerçek Unity sahnesidir; referans görüntüleri oyun yüzeylerine yapıştırılmaz.
- Unity'de **Just Sail → Build Windows** çalıştırılarak Windows sürümü yenilenir.
- **Create sailing scene** sahne üreticisidir; sahnedeki elle yapılan değişiklikleri yeniden oluşturur. Güncel sahneyi açmak için çalıştırmak gerekmez.
- Teknik doğrulama ve güncel durum: [management.md](management.md). Ürün: [product.md](product.md). Görsel hedef: [design.md](design.md). Kullanıcı yorumları: [feedback.md](feedback.md).

Görsel kalite ve sürüş hissi kullanıcı değerlendirmesini bekler. Kaynak repo: https://github.com/SePudra/just_sail.

## GitHub üzerinden kurulum

Unity Hub ile `JustSail/` klasörünü Unity **6000.6.6f1** kullanarak aç, paketlerin yüklenmesini bekle ve `Assets/JustSail/Scenes/FirstVoyage.unity` sahnesini aç. Build, Library, yerel CLI araçları ve video/referans doğrulama arşivi repoya dahil değildir. Yukarıdaki build ve doğrulama yolları yerel üretim çıktılarıdır. Oynamak için Editor Play veya **Just Sail → Build Windows** kullanılabilir.

