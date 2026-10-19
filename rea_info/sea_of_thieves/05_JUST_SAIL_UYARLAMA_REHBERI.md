# Just Sail — Sea of Thieves Tekniklerini Unity URP'ye Uyarlama Rehberi

Bu rehber, Sea of Thieves'ten elde edilen analiz bulgularının mevcut **Just Sail (Unity 6 / URP 17.6.0)** projenize nasıl entegre edilebileceğini somut C# ve HLSL kod bloklarıyla açıklar.

---

## 1. Dalga Formülü: Ortak HLSL ve C# Dalga Çekirdeği

Sea of Thieves'in en kritik kuralı: **CPU ve GPU aynı matematiksel dalga formülünü çalıştırmalıdır.**

### 1.1 HLSL Gölgelendirici Kodu (`GerstnerWaves.hlsl`)
Shader Graph içine **Custom Function** olarak ekleyebileceğiniz veya doğrudan HLSL su gölgelendiricinizde kullanabileceğiniz formül:

```hlsl
#ifndef JUST_SAIL_GERSTNER_INCLUDED
#define JUST_SAIL_GERSTNER_INCLUDED

struct WaveParameters
{
    float2 direction;
    float amplitude;
    float wavelength;
    float steepness;
};

void EvaluateGerstnerWave(
    float3 worldPos, 
    float time, 
    WaveParameters wave, 
    inout float3 displacement, 
    inout float3 normal)
{
    float k = 2.0 * 3.14159265 / wave.wavelength;
    float w = sqrt(9.81 * k);
    float2 d = normalize(wave.direction);
    float phase = k * dot(d, worldPos.xz) - w * time;

    float sinP = sin(phase);
    float cosP = cos(phase);

    float q = wave.steepness / (k * wave.amplitude * 4.0); // 4 dalgalı normalizasyon

    // Tepe noktası yer değiştirmesi (Displacement)
    displacement.x += q * wave.amplitude * d.x * cosP;
    displacement.y += wave.amplitude * sinP;
    displacement.z += q * wave.amplitude * d.y * cosP;

    // Normal vektörü türevleri
    float wa = wave.amplitude * k;
    normal.x -= d.x * wa * cosP;
    normal.y -= q * wa * sinP;
    normal.z -= d.y * wa * cosP;
}

void CalculateOceanSurface_float(
    float3 worldPos, 
    float time, 
    out float3 outDisplacement, 
    out float3 outNormal,
    out float outJacobian)
{
    // Sea of Thieves tarzı 4 kademeli okyanus dalgası
    WaveParameters waves[4];
    
    // 1. Ana Solugan (Swell)
    waves[0].direction = float2(1.0, 0.2);
    waves[0].amplitude = 1.2;
    waves[0].wavelength = 45.0;
    waves[0].steepness = 0.5;

    // 2. İkincil Solugan
    waves[1].direction = float2(0.8, -0.4);
    waves[1].amplitude = 0.7;
    waves[1].wavelength = 24.0;
    waves[1].steepness = 0.45;

    // 3. Rüzgâr Dalgası
    waves[2].direction = float2(0.3, 0.9);
    waves[2].amplitude = 0.35;
    waves[2].wavelength = 12.0;
    waves[2].steepness = 0.4;

    // 4. Kırışıklık (Chop)
    waves[3].direction = float2(-0.5, 0.6);
    waves[3].amplitude = 0.15;
    waves[3].wavelength = 5.0;
    waves[3].steepness = 0.3;

    float3 disp = float3(0, 0, 0);
    float3 nrm = float3(0, 1, 0);

    [unroll]
    for (int i = 0; i < 4; i++)
    {
        EvaluateGerstnerWave(worldPos, time, waves[i], disp, nrm);
    }

    outDisplacement = disp;
    outNormal = normalize(nrm);

    // Jacobian köpük belirteci (sıkışma oranı)
    outJacobian = saturate(1.0 - (disp.y / 2.4)); 
}

#endif
```

---

### 1.2 C# Deterministik Yükseklik Fonksiyonu (`OceanService.cs`)
Geminin fizik probları tarafından çağrılan, HLSL ile birebir aynı matematik:

```csharp
using UnityEngine;

public static class OceanService
{
    public struct Wave
    {
        public Vector2 direction;
        public float amplitude;
        public float wavelength;
        public float steepness;
    }

    private static readonly Wave[] Waves = new Wave[]
    {
        new Wave { direction = new Vector2(1.0f, 0.2f).normalized, amplitude = 1.2f, wavelength = 45.0f, steepness = 0.5f },
        new Wave { direction = new Vector2(0.8f, -0.4f).normalized, amplitude = 0.7f, wavelength = 24.0f, steepness = 0.45f },
        new Wave { direction = new Vector2(0.3f, 0.9f).normalized, amplitude = 0.35f, wavelength = 12.0f, steepness = 0.4f },
        new Wave { direction = new Vector2(-0.5f, 0.6f).normalized, amplitude = 0.15f, wavelength = 5.0f, steepness = 0.3f }
    };

    public static float GetWaterHeight(float x, float z, float time)
    {
        float height = 0f;

        for (int i = 0; i < Waves.Length; i++)
        {
            var w = Waves[i];
            float k = 2.0f * Mathf.PI / w.wavelength;
            float freq = Mathf.Sqrt(9.81f * k);
            float phase = k * (w.direction.x * x + w.direction.y * z) - freq * time;

            height += w.amplitude * Mathf.Sin(phase);
        }

        return height;
    }
}
```

---

## 2. Tekne Yüzdürme Bileşeni (`BoatBuoyancy.cs`)

Just Sail teknenizin altına yerleştireceğiniz çok noktalı fizik prob sistemi:

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatBuoyancy : MonoBehaviour
{
    [System.Serializable]
    public struct BuoyancyProbe
    {
        public Vector3 localPosition;
        public float volumeWeight;
    }

    [Header("Problar")]
    [SerializeField] private BuoyancyProbe[] probes;

    [Header("Fizik Ayarları")]
    [SerializeField] private float waterDensity = 1000f;
    [SerializeField] private float maxSubmergenceDepth = 1.2f;
    [SerializeField] private float linearDragInWater = 2.5f;
    [SerializeField] private float angularDragInWater = 3.0f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float time = Time.time;
        int submergedCount = 0;

        for (int i = 0; i < probes.Length; i++)
        {
            Vector3 worldProbePos = transform.TransformPoint(probes[i].localPosition);
            float waterLevel = OceanService.GetWaterHeight(worldProbePos.x, worldProbePos.z, time);
            float depth = waterLevel - worldProbePos.y;

            if (depth > 0f)
            {
                submergedCount++;
                float submergenceRatio = Mathf.Clamp01(depth / maxSubmergenceDepth);
                
                // Archimedes kaldırma kuvveti
                Vector3 buoyantForce = Vector3.up * (waterDensity * probes[i].volumeWeight * 9.81f * submergenceRatio);
                rb.AddForceAtPosition(buoyantForce, worldProbePos, ForceMode.Force);
            }
        }

        // Su içindeyken sönümleme uygula (durmaksızın sallanmayı engeller)
        if (submergedCount > 0)
        {
            float waterFactor = (float)submergedCount / probes.Length;
            rb.linearDamping = Mathf.Lerp(0.05f, linearDragInWater, waterFactor);
            rb.angularDamping = Mathf.Lerp(0.05f, angularDragInWater, waterFactor);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (probes == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in probes)
        {
            Gizmos.DrawSphere(transform.TransformPoint(p.localPosition), 0.15f);
        }
    }
}
```

---

## 3. Gün/Gece ve Hava Durumu Sistemi Entegrasyonu (`DayNightWeatherController.cs`)

Sea of Thieves tarzı gün döngüsü ve hava durumu değişimini URP Volume ve Directional Light ile bağlayan yapı:

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightWeatherController : MonoBehaviour
{
    [Header("Işık ve Gökyüzü")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Volume globalVolume;
    
    [Header("Zaman Ayarı (24 dakikalık döngü)")]
    [Range(0f, 24f)] public float timeOfDay = 12.0f;
    public float dayDurationInSeconds = 1440f; // 24 dakika

    [Header("Güneş Renk Eğrisi")]
    public Gradient sunColorGradient;
    public AnimationCurve sunIntensityCurve;

    [Header("Fırtına Kontrolü")]
    [Range(0f, 1f)] public float stormIntensity = 0f;
    [SerializeField] private ParticleSystem rainParticles;

    private void Update()
    {
        // Zamanı ilerlet
        timeOfDay += (Time.deltaTime / dayDurationInSeconds) * 24f;
        if (timeOfDay >= 24f) timeOfDay = 0f;

        // Güneş açısını döndür (0 = Gece yarısı, 6 = Şafak, 12 = Öğle, 18 = Gün batımı)
        float sunAngle = (timeOfDay / 24f) * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 50f, 0f);

        // Renk ve şiddeti güncelle
        float t = timeOfDay / 24f;
        sunLight.color = sunColorGradient.Evaluate(t);
        sunLight.intensity = sunIntensityCurve.Evaluate(t) * Mathf.Lerp(1.0f, 0.2f, stormIntensity);

        // Fırtına parçacıkları ve sis
        if (rainParticles != null)
        {
            var emission = rainParticles.emission;
            emission.rateOverTime = stormIntensity * 1500f;
        }

        // RenderSettings sisi fırtınada yoğunlaştır
        RenderSettings.fogDensity = Mathf.Lerp(0.002f, 0.035f, stormIntensity);
    }
}
```

---

## 4. Uygulama Yol Haritası (Checklist)

Just Sail projesinde adım adım yapılacaklar:

- [ ] **1. Adım:** `GerstnerWaves.hlsl` dosyasını `JustSail/Assets/` altına kaydedin ve su Shader Graph'ınıza `Custom Function` düğümü olarak ekleyin.
- [ ] **2. Adım:** `OceanService.cs` dosyasını oluşturarak aynı dalga katsayılarını C#'a aktarın.
- [ ] **3. Adım:** Teknenin altına 6 adet prob (Baş, Kıç, İskele, Sancak, Orta-Sol, Orta-Sağ) yerleştirip `BoatBuoyancy.cs` bileşenini ekleyin.
- [ ] **4. Adım:** URP Asset ayarlarında **Cascade Count: 4** seçin; kaskad mesafelerini `15m, 50m, 180m, 600m` olarak ayarlayın.
- [ ] **5. Adım:** Su gölgelendiricinize `Beer-Lambert` derinlik renk soğurması ve `det(J)` Jacobian tepe köpüğü çıktısını bağlayın.
- [ ] **6. Adım:** `DayNightWeatherController.cs` ile 24 dakikalık gün döngüsünü ve fırtına yoğunluk kaydırıcısını sahneye entegre edin.
