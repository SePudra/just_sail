# Over the Hill — Just Sail İçin İlham ve Uyarlama Stratejisi

*Over the Hill*'in (Funselektor) sanat yönetimi, kamera dili ve atmosferik sistemleri; Unity 6 ve URP altyapısını kullanan **Just Sail** projeniz için muazzam bir ilham ve gelişim yol haritası sunmaktadır.

Her iki oyun da doğayla baş başa kalma, huzurlu keşif ve fizik tabanlı araç kontrolü temalarını paylaşmaktadır.

---

## 1. Görsel Tarzın Uyarlanması: Just Sail'i "Canlı Bir Diorama" Yapmak

Şu anda Just Sail geniş açılı standart bir üçüncü şahıs kamera kullanmaktadır. Over the Hill'in diorama sihrini yakalamak için:

```
[MEVCUT DURUM]                              [OVER THE HILL İLHAMLI HEDEF]
• 60° Standart Kamera                      • 32° Telefoto Dar Açı (Minyatür etkisi)
• Sert Keskin Ufuk Çizgisi                 • Tilt-Shift Derinlik Bulanıklığı (Bokeh)
• Standart PBR Kontrastı                   • Lifted Blacks (Analog 35mm film hissi)
• Yalnızca Deniz Yansımaları               • Zengin Pastel Renk Armonisi & Altın Saat
```

### 1.1 Kamera ve Odak Ayarı (Cinemachine)
* **Kamera FOV:** `Field of View` değerini **$30^{\circ} - 35^{\circ}$** arasına çekin; kamerayı tekneden biraz daha uzağa ve yukarıya konumlandırın.
* **Volume Derinlik Bulanıklığı (Depth of Field):**
  * Mod: **Gaussian** veya **Bokeh**.
  * `Focus Distance`: Tekneye olan mesafe (örn. 15m).
  * `Focal Length`: 85mm.
  * Bu ayar, yelkenlinizin sanki bir akvaryum veya masaüstü cam fanus içinde yüzen minik, sevimli bir el yapımı model gibi algılanmasını sağlayacaktır.

---

## 2. Atmosfer ve Büyü: Kutup Işıkları (Aurora) Altında Gece Seyri

Just Sail şu anda Sakin Koy ve Fener Kayalığı çevresinde gündüz odaklı bir prototiptir. Gece atmosferine *Over the Hill*'in en vurucu özelliği eklenebilir:

* **Kutup Işıkları (Aurora Borealis):**
  * Gece olduğunda gökyüzünde hafif zümrüt yeşili ve lavanta rengi dalgalanan ışık perdeleri açılır.
  * Bu yeşil ışık okyanus dalgalarının tepelerinde ve teknenin beyaz yelkeninde parıldar.
  * Fenerin sıcak sarı ışığı ile gökyüzünün soğuk yeşil aurorası muazzam bir renk kontrastı oluşturur.

---

## 3. Fotoğrafçılık ve Deniz Yaban Hayatı: "Denizcinin Seyir Defteri"

*Over the Hill*'deki Polaroid sistemi, Just Sail için biçilmiş kaftandır:

```mermaid
flowchart LR
    Sailing[Denizde Seyir] --> SpotLife[Yunus Sürüsü / Balina Kuyruğu / Deniz Kaplumbağası]
    SpotLife --> PolaroidMode[Polaroid Vizörünü Aç]
    PolaroidMode --> TakePhoto[Deklanşöre Bas: Beyaz Çerçeveli Kart Fırlar]
    TakePhoto --> Logbook[Kaptanın Seyir Defterine (Logbook) Yapıştır]
    Logbook --> Rewards[Sail Point / Yeni Yelken Deseni Ödülü]
```

* **Deniz Canlıları:** Teknenin baş bodoslamasında yarışan yunuslar, uzakta su püskürten kambur balinalar veya Fener Kayalığı'nda tüneyen martılar.
* **Fiziksel Albüm:** Oyuncunun çektiği fotoğraflar, hafif yamuk yerleştirilmiş Polaroid kartları halinde kaptan köşkünde açılan bir ahşap seyir defterinde toplanır.

---

## 4. İskele Bağlama ve Halat Fiziği (Obi Rope Uyarlaması)

*Over the Hill* nasıl aracı ağaca bağlamak için vinç kullanıyorsa; Just Sail de aynı mekaniği **İskele Palamar Halatı (Mooring Line)** için kullanabilir:

* Koya yanaştığınızda tekneden iskeleye bir halat fırlatılır.
* Halat gerildikçe tekne yumuşakça iskeleye yanaşır ve dalgalarda iskeleye çarpmadan tatlı tatlı salınır.
* Bu mekanik, oyuncuya gerçek bir denizci olduğu hissini taktil (fiziksel) olarak yaşatacaktır.
