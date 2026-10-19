# Just Sail — Geri Bildirim Kaydı

Sürüm: 0.2 · 16 Ekim 2026

## Kullanım

Kullanıcı yorumunu, yapılan işteki eksikliği, düzeltmeyi ve sonucu kaydet. İşe başlarken yalnızca görevle ilgili açık kayıtları oku; kapanmış geçmişi tekrar yükleme. Dosya kendisi işlem yapmaz; güncellemeleri Codex uygular.

- Ürün/kontrol/kapsam değişirse [product.md](product.md), sanat/his değişirse [design.md](design.md), süreç/teslim değişirse [management.md](management.md) ilgili bölümünden güncellenir. Kararı burada tekrar uzun uzun anlatmak yerine belge/bölüm ve kayıt kimliği yazılır.
- Kullanıcının açık son talimatı önceki belgeden önceliklidir. Açık değişiklik talebi için yeniden onay isteme; yorumun anlamını uydurma.
- Önemli her konuya artan FB kimliği ver. Küçük yorumda birkaç satır yeterli; ilgisiz alanları doldurma. Sıradan her kod değişikliğini kayıt yapma.
- Neden bilinmiyorsa hipotez yaz. Teknik doğrulamayı kullanıcı beğenisiyle karıştırma; sessizliği kabul sayma. Bilinmeyen build/kanıt bilgisini uydurma.
- Kullanıcı beğenmezse aynı kaydı yeniden aç; önceki denemenin sonucunu silme. Kalıcı ders ilgili hedef belgeye taşınır; bütün geçmişin sürekli okunması gerekmez.

## Kısa şablon

### FB-XXX — Konu

Tarih · İlgili alan/build (biliniyorsa) · Durum

- **Yaptığım:** Gerçek teslim ve önemli karar.
- **Kullanıcının yorumu:** Kısa alıntı veya sadık özet.
- **Neden yetersiz:** Somut neden/kanıt; bilinmiyorsa sınanacak hipotez.
- **Düzeltme:** Değişiklik ve görülebilir kabul ölçütü.
- **Sonuç / belge etkisi:** Yapılan kontrol, kanıt ve değişen belge/bölüm. Yapılmadıysa belirt.
- **Ders:** Tekrarlanmasını önleyecek kısa kural; gerekiyorsa ilgili belgeye taşı.

Durumlar: Açık, Uygulandı, Kullanıcı değerlendirmesinde, Kabul edildi, Ertelendi. Kabul yalnızca açık kullanıcı değerlendirmesiyle; ertelemede gerekçe/aşama yazılır.

## Açık kayıtlar

### FB-005 — Teknik uyarlama görsel benzerlik sağlamadı

16 Ekim 2026 · Video referanslı sanat çalışması · Kullanıcı değerlendirmesinde

- **Yaptığım:** Kamera, odak, atmosfer ve keşif özelliklerini mevcut seyrek adaların üzerine ekledim.
- **Kullanıcının yorumu:** Over the Hill'e hâlâ benzemediğini söyledi; dört intro videosundan kareler seçip gerçek oyun görüntüsüyle tekrar tekrar karşılaştırmamı, gemi ve assetleri geliştirmemi istedi.
- **Neden yetersiz:** Hedefin sıcak ot örtüsü, yoğun ve farklı silüetli bitkileri, geniş kaya kütleleri ve kıyı yansıması kurulamamıştı. Teknik özelliklerin varlığı görsel benzerlik yerine kullanıldı.
- **Düzeltme:** Video kareleri/zamanları kaydedildi; yeni Blender teknesi ve bitki/kaya modelleri, sonbahar kıyısı, ot örtüsü ve planar yansıma üretildi. Sabit poz/saat/dalga anında karşılaştırma ve tur başına öz eleştiri `Verification/VisualStudy/iterations.md` içinde.
- **Sonuç / belge etkisi:** On ana görsel tur, ardından yakın modelde AO düzeltmesi yapıldı. Son Windows paketinde 17 su/atmosfer/fizik kontrolü geçti; ilk yağmur hatası ve ham raporu silinmedi. Yan yana kare, yakın tekne, gerçek hareket kaydı ve `windows-report.json` `Verification/VisualStudy/` içinde. Güncel hedef design.md'ye işlendi. Bitki/kaya yerleşimi ve ince yüzey yapısı referanstan hâlâ ayrılıyor; birebir sonuç veya kullanıcı kabulü iddiası yok.
- **Ders:** Ana kadrajın şekil/ışık/renk ilişkisini karşılaştır; özellik listesi veya test sayısını görsel başarı sayma.

### FB-004 — Suyun renk geçişleri sert

16 Ekim 2026 · M2b su görünümü · Kullanıcı değerlendirmesinde

- **Yaptığım:** Derinlik rengi, dalga aydınlatması, gökyüzü yansıması ve iki sis katmanını birleştirdim.
- **Kullanıcının yorumu:** “Suyun renk geçişi çok kötü duruyor”; geçişlerin iyileştirilmesini istedi.
- **Neden yetersiz:** Sığ/derin renk karışımı dar aralıkta değişiyor; güçlü normal/yansıma kontrastı ve ek sis yüzeyi açık/koyu lekelere ayırıyordu. Önce/sonra oyun görüntülerinde bu fark görülebiliyor.
- **Düzeltme:** Orta turkuaz ton ve geniş derinlik karışımı; yalnızca renk derinliğinde komşu örnek filtresi; daha sakin yansıma, parlama ve sis birleşimi. Dalga çekirdeği/yüzdürme değişmedi.
- **Sonuç / belge etkisi:** Gündüz açık su, iskele kıyısı ve akşam seyri incelendi; shader/C# derlemesi, kısa hareket kontrolü ve Windows build geçti. Görüntüler `Verification/WaterColour/` içinde. design.md Su bölümü güncellendi; görsel kabul bekliyor.
- **Ders:** Suyun derinlik rengi ve yansıma kontrastını beraber değerlendir; ayrı ayrı güzel görünen katmanlar birlikte sertleşebilir.

### FB-003 — İlk prototipin görsel yönü reddedildi

16 Ekim 2026 · M1 reddi / M2 SoT uyarlaması · Kullanıcı değerlendirmesinde

- **Yaptığım:** Küçük yelkenli, iki ada, su ve menüler ürettim; uzun fizik testini teslimin merkezine koydum.
- **Kullanıcının yorumu:** Görsel sonucu çok kötü buldu; 1 saat 20 dakikalık üretimin sonucu karşılamadığını söyledi. Sea of Thieves analiziyle su/hava/gün sistemlerinin yeniden uyarlanmasını istedi.
- **Neden yetersiz:** İlk görüntüde su düz ve tekrarlı çizgilerle temsil edildi; ada/su/ışık ilişkisi hedeflenen seviyeye ulaşmadı. Görsel hedef doğrulanmadan kapsam ve testler büyütüldü. Teknik başarı görsel kabul değildir.
- **Düzeltme:** SoT'a ait altı belgeden ortak dalga çekirdeği, doğru yatay örnekleme, kıyı sönümü, tepe köpüğü, altı prob ve yaşayan atmosfer uygulandı. Kullanıcı daha sonra Over the Hill raporlarını da serbestçe uyarlamayı istedi: kamera/renk/odak/çevre, aurora, canlılar, fotoğraf defteri ve palamar eklendi. İlk kadrajda ada tepelerinin kesilmesi ve geniş ekranda defterin taşması görsel kontrolde bulunup düzeltildi.
- **Sonuç / belge etkisi:** M2'de 17 kontrol geçti, ilk 9 cm CPU/GPU farkı 0,21 mm'ye indirildi. M2b son Windows paketinde 22 yeni işlev ve 17 su/fizik kontrolü geçti. Gerçek kareler, test albümü ve hareket kaydı `Verification/OverTheHill/` içinde; önceki M2 kanıtı oradaki `M2-archive/` altında korundu. Kapsam ve ölçüm sınırları management.md, sanat/his kararları design.md, yeni keşif kontrolleri product.md/README.md içinde güncellendi. Görsel sonuç kullanıcı tarafından henüz kabul edilmedi; bu kayıt kapanmadı.
- **Ders:** Sanat yönünü ölçülebilir görsel karşılaştırmayla kontrol et; derlenmeyi veya fizik testini görsel kalite yerine sunma.

### FB-002 — Belgeleri kısa ve görev odaklı kullan

16 Ekim 2026 · Dokümantasyon · Uygulandı

- **Yaptığım:** Dört ayrıntılı belge oluşturdum; bazı karar ve kontrol listeleri tekrarlandı.
- **Kullanıcının yorumu:** Token tüketimiyle ilgili açıklamadan sonra sadeleştirme önerisinin dosyalara uygulanmasını istedi.
- **Neden yetersiz:** Metin, günlük kullanım için gereğinden uzundu; bilgi tekrarları bakım ve yeniden okuma yükü oluşturuyordu. Gerçek token tasarrufu ölçülmedi.
- **Düzeltme:** Dört dosyayı kısalt; her bilginin tek kaynağı olsun; yalnızca ilgili bölümleri/açık kayıtları oku. Feedback şablonunu birkaç satıra indir.
- **Sonuç / belge etkisi:** Dört dosya v0.2 olarak sadeleştirildi; yerel bağlantılar kontrol edildi. Oyun kapsamı korundu; kod/asset değişmedi. Kullanıcının sonuç değerlendirmesi henüz yok.
- **Ders:** Yeni karar veya önemli sonuç yoksa belgeyi büyütme.

### FB-001 — Geri bildirimi üretim kararlarına bağla

16 Ekim 2026 · Süreç · Kullanıcı değerlendirmesinde

- **Yaptığım:** İlk üç belgede tekil yorum/düzeltme kaydı yoktu.
- **Kullanıcının yorumu:** Yapılan iş, kendi yorumu ve neden yetersiz kaldığının kaydedildiği; diğer belgeleri etkileyen feedback.md istedi.
- **Neden yetersiz:** Yorum → neden → düzeltme → yeniden değerlendirme zinciri tanımlanmamıştı.
- **Düzeltme / sonuç:** Bu dosya ve diğer belgelere bağlantıları eklendi; kayıt düzeni FB-002 ile kısaltıldı. Kullanıcının su animasyonu örneği gerçek oyun hatası olarak kaydedilmedi.
- **Ders:** Hedefi değiştiren yorumu ilgili belgeye işle; düzeltmeyi gerçek sonucuyla takip et.

## Kapanmış kayıtlar

Henüz yok. Kapanan kayıtlar burada kısa tutulur; ilgili bir sorun tekrarlanırsa yeniden okunur.
