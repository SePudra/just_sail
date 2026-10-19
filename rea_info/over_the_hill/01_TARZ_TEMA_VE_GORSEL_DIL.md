# Over the Hill — Tarz, Tema ve Görsel Dil Analizi

*Over the Hill*'in en büyüleyici ve ayırt edici yönü; fotogerçekçiliğin getirdiği karmaşık görsel gürültüyü reddederek, **minimalist bir diorama estetiği, resimsi (painterly) renk teorisi ve nostaljik bir analog his** inşa etmesidir.

Bu doküman, geliştirici Funselektor'ün (*art of rally*, *Absolute Drift*) imza sanat felsefesini, kamera matematiğini, renk paletlerini ve malzeme dilini ayrıntılı olarak inceler.

---

## 1. Sanat Felsefesi: "Diorama ve Minyatür Minyonluğu"

Geleneksel açık dünya ve simülasyon oyunları oyuncuyu devasa bir dünyanın içine gömmeye çalışırken; *Over the Hill*, dünyayı bir **masaüstü maketi (diorama)** gibi sunar.

```
[GENİŞ AÇILI FOTOGERÇEKÇİLİK]       vs.       [FUNSELEKTOR DİORAMA ESTETİĞİ]
- 90° Geniş FOV (balıkgözü distorsiyon)        - 25°-35° Dar FOV (telefoto / izometrik sıkıştırma)
- Yüksek frekanslı pürüzlü dokular            - Pürüzsüz degrade renkler, temiz formlar
- Karmaşık, yorucu görsel gürültü             - Seçilmiş renk armonisi, dinlendirici sadelik
- Oyuncu devasa bir dünyada küçük             - Oyuncu canlı bir maket dünyasını izleyen gözlemci
```

### 1.1 Minyatür Hissinin Matematiksel Sırrı: Tilt-Shift ve Telefoto Lens
Oyundaki minyatür etkisini sağlayan temel araçlar REA analizinde ortaya çıkmıştır:
* **Dar Görüş Açısı (Telephoto Compression - FOV 25°-35°):** Cinemachine sanal kameralarında perspektif distorsiyonu minimuma indirilmiştir. Uzaktaki dağlar ve yakındaki çam ağaçları birbirine yakın ölçekte görünür (perspektif düzleşmesi).
* **Tilt-Shift Derinlik Alanı (Depth of Field):** `BeautifyDoFCameraSettingsParameter` analizinde görüldüğü üzere, odak düzlemi (Focal Plane) aracın veya ilgi noktasının üzerine çok dar bir bant halinde kilitlenir. Aracın hemen 5 metre önü ve arkası ipeksi bir bulanıklığa (bokeh) bürünür.
* **Sonuç:** İnsan beyni bu dar odak derinliğini sadece makro lenslerle minik oyuncakları fotoğraflarken deneyimlediği için, dev dağlar ve kamyonlar anında zarif, el yapımı ahşap/plastik figürinler gibi algılanır.

---

## 2. Renk Teorisi ve Işık Paletleri

*Over the Hill*, renkleri gelişigüzel değil, **duygusal birer anlatım aracı** olarak kullanır.

```
[SABAH SİSİ]          [ÖĞLE GÜNEŞİ]          [ALTIN SAAT]          [GECE & AURORA]
Soluk Şeftali / Gri   Doygun Adaçayı Yeşili  Sıcak Pişmiş Toprak   Koyu Çivit Mavisi
Pastel Eflatun Gökyüzü Canlı Çam Ormanı      Amber / Bal Işığı     Zümrüt Yeşil Aurora
Yumuşak Beyaz Pus     Net Kontrast           Lavanta Dağ Gölgeleri Sıcak Turuncu Ateş
```

### 2.1 Nostaljik ve Analog Renk Derecelendirmesi (Color Grading)
* **Kaldırılmış Siyahlar (Lifted Blacks):** Sahnedeki en karanlık gölgeler bile asla tam zifiri siyah (`#000000`) değildir; hafif puslu, lacivert veya sıcak kahve tonlarına çekilmiştir (eski analog 35mm film estetiği).
* **Sıkıştırılmış Parlak Noktalar (Soft Highlights):** Güneş yansımaları beyaz patlamalara (clipping) izin vermez; yumuşak şeftali ve krem tonlarında sınırlandırılır.
* **Ton Ayrımı (Split Toning):** Gölgelerde soğuk maviler ve morlar hakimken, aydınlık alanlarda sıcak altın ve hardal tonları kullanılır.

---

## 3. Geometri ve Malzeme Dili: "Az Çoktur" (Less is More)

Oyundaki nesnelerin modellenme ve dokulandırma tarzı 3 ana kurala dayanır:

### 3.1 Şekil Sadeliği (Low-to-Mid Poly Silhouettes)
* Ağaçlar, kayalar ve araçlar keskin mikroskobik çatlaklarla doldurulmaz; ana kütleleri ve siluetleri belirgindir.
* Kayalar büyük, güçlü düzlemlerden (faceted planes) oluşur ancak kenarlarına pah kırılmıştır (beveled edges); bu sayede ışık kenarlarda ince, parlak çizgiler halinde akar.

### 3.2 Dokusuz / Prosedürel Degrade Malzemeler
* PBR malzeme katmanlarında gürültülü fotoğraf dokuları (photo scans) kullanılmaz.
* Bunun yerine **dikey renk degradeleri (vertical gradients)** ve **eğim bazlı renk karışımları (slope-based blending)** uygulanır:
  * Ağaçların alt kısımları koyu yosun yeşili, tepe yaprakları ise sarı-yeşil ışık alır.
  * Dağların dik yamaçları taş grisi, yatay sırtları ise kadifemsi çimen kaplıdır.

---

## 4. UI ve Taktil (Fiziksel) Arayüz Dili: Polaroid Ruhu

*Over the Hill*'in tarzı arayüzünde de devam eder:
* **Sıfır Ekran Kalabalığı:** Ekranda kocaman mini haritalar, dev can barları veya yanıp sönen oklar yoktur. Ekran tamamen manzaraya bırakılmıştır.
* **Polaroid Fotoğraf Sistemi:**
  * Fotoğraf makinesi açıldığında (`ActivatePolaroidModeCamera`) ekranda klasik bir vizör çerçevesi ve hafif vinyet belirir.
  * Deklanşöre basıldığında analog bir mekanik ses duyulur ve ekrandan fiziksel bir Polaroid kartı fırlar.
  * Kartların üzerinde beyaz kenarlıklar, elle yazılmış gibi duran tipografi ve hafif açılı eğimler (`PolaroidMaxRotation`, `PolaroidPositionJitter`) bulunur.
* **Yumuşatılmış Kart Arayüzü (`Nobi.UiRoundedCorners`):** Menüler modern, yuvarlak köşeli, yarı saydam cam hissi veren (`LeTai.TranslucentImage`) pastel kartlar olarak açılır.

---

## 5. Just Sail İçin Çıkarılacak Sanat Dersleri

Just Sail'in görsel kalitesini bir üst seviyeye taşımak için *Over the Hill*'den alınabilecek 4 altın kural:

1. **Kamera FOV Değerini Düşürün:** Kamerayı geniş açıdan (60°-70°) dar açıya (30°-40°) çekip tekneye biraz daha uzaktan bakın. Tekneniz ve adalarınız anında şık birer masaüstü maketine dönüşecektir.
2. **Tilt-Shift Derinlik Bulanıklığı Ekleyin:** URP Volume içine `Depth of Field` bileşeni ekleyip ön ve arka planı yumuşakça bulanıklaştırın.
3. **Dokulardaki Paraziti Temizleyin:** Ahşap ve yelken modellerinizde detaylı fotoğrafik dokular yerine zengin renk degradeleri ve temiz kenar çizgileri kullanın.
4. **Analog Renk Filtresi (LUT) Uygulayın:** Siyahları hafifçe yukarı kaldırıp gölgelere deniz mavisi, güneşe ise sıcak bal rengi tonları verin.
