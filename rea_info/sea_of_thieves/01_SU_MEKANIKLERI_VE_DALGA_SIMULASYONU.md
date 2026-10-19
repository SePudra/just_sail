# Sea of Thieves — Su Mekanikleri ve Dalga Simülasyonu

Sea of Thieves'in okyanusu, oyun dünyasındaki en gelişmiş ve stilize edilmiş gerçek zamanlı su simülasyonlarından biridir. Bu doküman, suyun matematiksel modelini, CPU-GPU senkronizasyonunu, köpük üretimini ve kıyı etkileşimini açıklar.

---

## 1. Dalga Matematiği: Hibrit Gerstner ve Spektral Yaklaşım

Sea of Thieves, fiziksel olarak doğru okyanus swells (büyük okyanus dalgaları) oluşturmak için **Trochoidal (Gerstner) Dalga Teorisi**'ni kullanır. Basit sinüs dalgaları tepe ve çukurlarda simetrikken, Gerstner dalgaları suyu sadece dikeyde kaldırmakla kalmaz, tepe noktalarına doğru yatay olarak da çeker. Bu da gerçek denizlerdeki sivri tepeleri ve geniş, düz çukurları üretir.

### 1.1 Gerstner Dalga Formülasyonu
Tek bir Gerstner dalgası için yer değiştirme vektörü $\vec{P}(x, y, t)$:

$$
\vec{P}(x, y, t) = \begin{pmatrix}
x + \sum_{i=1}^{N} \left( Q_i A_i D_{i,x} \cos(\theta_i) \right) \\
y + \sum_{i=1}^{N} \left( Q_i A_i D_{i,y} \cos(\theta_i) \right) \\
\sum_{i=1}^{N} \left( A_i \sin(\theta_i) \right)
\end{pmatrix}
$$

Burada:
* $A_i$: Dalga genliği (Wave Amplitude).
* $\vec{D}_i = (D_{i,x}, D_{i,y})$: Normalize edilmiş dalga yayılma yönü (rüzgâr yönüyle ilişkili).
* $Q_i$: Dalga dikliği (Steepness). $Q_i = \frac{1}{\omega_i A_i \times N}$ sınırlandırmasıyla döngü oluşması (dalganın kendi içine katlanıp poligonların çakışması) engellenir.
* $k_i$: Dalga sayısı (Wave Number), $k_i = \frac{2\pi}{\lambda_i}$ ($\lambda_i$ = Dalga boyu).
* $\omega_i$: Açısal frekans (Dispersion relation: derin suda $\omega_i = \sqrt{g \cdot k_i}$, $g = 9.81 \, m/s^2$).
* $\theta_i$: Dalga fazı, $\theta_i = \vec{D}_i \cdot (x, y) \cdot k_i + \omega_i t + \phi_i$.

### 1.2 Dalga Kademelendirmesi (Multi-Octave Waves)
Sea of Thieves'te su tek bir formülle değil, frekans bandına göre ayrılmış katmanlarla simüle edilir:
1. **Okyanus Soluganı (Macro Swell - 2 ila 4 dalga):** Çok uzun dalga boylu ($\lambda = 60 - 150m$), gemilerin ana eğimini ve yalpalamasını belirleyen devasa dalgalar.
2. **Rüzgâr Dalgaları (Meso Waves - 4 ila 8 dalga):** Yerel rüzgâr yönüne paralel, orta boylu dalgalar ($\lambda = 15 - 40m$).
3. **Mikro Dalgalar ve Kırışıklıklar (Micro Chop - Normal Haritası):** Doğrudan geometri olarak çizilmeyen, piksel gölgelendiricide (Pixel Shader) kayan çift normal haritası (scrolling tangent normals) ile simüle edilen yüzey detayları.

---

## 2. Ağ Senkronizasyonu ve Determinizm

Sea of Thieves'te çok oyunculu ortamda dalgaların birebir uyuşması için şu yapı kullanılır:

```
[Server World Time] (float saniye)
       │
       ├──> CPU Yüzdürme Hesabı: GetWaterHeight(WorldPos, ServerTime)
       │    └──> Gemi ve Oyuncu Fiziği
       │
       └──> GPU Vertex Shader: GerstnerDisplacement(VertexWorldPos, ServerTime)
            └──> Ekrandaki Su Poligonları
```

* **Hiçbir dalga verisi ağ paketleriyle gönderilmez.** İstemci ve sunucu sadece aynı `ServerTime` değişkenine ve dalga parametrelerine (dalga tablosu) sahiptir.
* CPU'da C++ kodu ile GPU'da HLSL kodu tam olarak aynı trigonometrik denklemi çalıştırır. Sonuç olarak geminin karinasının suya battığı an ile ekranda suyun sıçradığı an 0 milisaniye gecikmeyle tam örtüşür.

---

## 3. Okyanus Ağı (LOD & Mesh Topology)

Ekranın tamamını kaplayan okyanusun performanslı çizilmesi için **Camera-Centric Projected Grid (veya Radyal Kademeli Halka Ağı)** kullanılır:
* Kameranın tam altında poligon yoğunluğu en yüksektir (santimetre düzeyinde tepe noktaları).
* Kameradan ufka doğru gidildikçe poligon boyutları geometrik olarak büyür ($2^n$ katlama).
* **LOD Morflaması (Popping Engelleme):** Kamera hareket ettikçe uzak halkalardaki tepe noktaları bir sonraki LOD kademesine yumuşakça (lerp) geçer; böylece dalgalarda titreme veya aniden beliren poligonlar (pop-in) oluşmaz.

---

## 4. Tepe Köpüğü (Crest Foam) ve Jacobian Matrisi

Sea of Thieves'teki beyaz köpüklü dalga tepeleri el yapımı bir doku haritası değildir; dalganın fiziksel olarak sıkıştığı yerlerde **matematiksel olarak türetilir**.

### 4.1 Jacobian Belirteci (Jacobian Determinant)
Gerstner dalgası yüzeyi yatayda sıkıştırdığında alan daralır. Bu durum tepe noktasının yerel Jacobian matrisi $J$ ile ölçülür:

$$
J = \begin{pmatrix} 
\frac{\partial x'}{\partial x} & \frac{\partial x'}{\partial y} \\ 
\frac{\partial y'}{\partial x} & \frac{\partial y'}{\partial y} 
\end{pmatrix}
$$

* Eğer $\det(J) \approx 1$ ise su yüzeyi düzgündür.
* Eğer $\det(J) < 0.3$ ise dalga tepeye doğru aşırı sıkışmıştır ve dalga kırılmak üzeredir.
* Gölgelendiricide `FoamIntensity = saturate((Threshold - det(J)) * Multiplier)` formülüyle dinamik beyaz köpük dokusu tepeye bindirilir.

---

## 5. Kıyı Şeridi Etkileşimi (Shoreline Attenuation)

Okyanus dalgaları adalara yaklaştığında üç kritik fiziksel davranış sergiler:
1. **Genlik Sönümleme (Amplitude Attenuation):** Açık denizdeki 6 metrelik dalgalar kıyıya çarptığında adanın içine girmemeli veya plajda dev dalgalar oluşturmamalıdır. Sea of Thieves, adaların **Global Distance Field (Mesafe Alanı)** verisini kullanır. Kıyıya olan mesafe azaldıkça $A_i$ çarpanı $0$'a doğru sönümlenir.
2. **Dalga Yönünün Kıyıya Kırılması (Refraction):** Dalga yön vektörü $\vec{D}_i$, adanın kıyı normal vektörüne doğru yönlendirilir (dalgalar her zaman sahile paralel vurur).
3. **Sığlık Köpüğü ve Kırılma:** Kıyı derinliği azaldıkça zemin derinlik farkı (`SceneDepth - WaterDepth`) hesaplanır; sığ kıyılarda köpük haritası adanın etrafında yumuşak dalgacıklar oluşturur.

---

## 6. Su Gölgelendiricisi (Water Shading & Optics)

Sea of Thieves'in kendine has stilize su görseli 4 ana optik katmandan oluşur:
1. **Derinlik Soğurması (Absorption / Beer-Lambert Yasası):**
   $$C_{final} = C_{shallow} \cdot e^{-\sigma \cdot depth} + C_{deep} \cdot (1 - e^{-\sigma \cdot depth})$$
   Sığ sular turkuaz-açık yeşil, derin okyanus çukurları ise koyu lacivert görünür.
2. **Yüzey Altı Saçılımı (Subsurface Scattering - SSS):**
   Güneş dalganın arkasındayken, dalga tepelerinden geçen ışık içten aydınlanan zümrüt yeşili bir parlama oluşturur. Bu, $\vec{L}$ (ışık) ve $\vec{V}$ (görüş) arasındaki açı ile dalga kalınlığının çarpımıyla hesaplanır.
3. **Fresnel Yansıması:**
   Schlick yaklaşımı ile suya dik bakıldığında dip görünür, yatay bakıldığında gökyüzü ve bulutlar ayna gibi yansır.
4. **Islaklık ve Su Damlacıkları:**
   Oyuncunun kamerası suya girip çıktığında post-process katmanında distorsiyonlu damla kaydırma (droplet refraction) uygulanır.
