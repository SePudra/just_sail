# Just Sail — REA Tersine Mühendislik Bilgi Bankası

Bu dizin, **REA (Reverse Engineer Anything)** araçları ve ikili dosya incelemeleriyle analiz edilen referans oyunların sistem mimarilerini, görsel sanat felsefelerini ve tekniklerini içerir.

---

## 1. [Sea of Thieves İncelemesi](00_GENEL_BAKIS_VE_MIMARI.md)
* **Motor:** Özel Unreal Engine 4 (Athena)
* **Odak:** Büyük okyanus simülasyonu, deterministik dalga senkronizasyonu, prob tabanlı gemi yüzdürme fiziği, sintine/su alma mekaniği, hacimsel fırtınalar ve gün döngüsü.
* **Detaylı Raporlar:**
  * [00_GENEL_BAKIS_VE_MIMARI.md](00_GENEL_BAKIS_VE_MIMARI.md)
  * [01_SU_MEKANIKLERI_VE_DALGA_SIMULASYONU.md](01_SU_MEKANIKLERI_VE_DALGA_SIMULASYONU.md)
  * [02_GEMI_FIZIGI_VE_YUZDURME_BUOYANCY.md](02_GEMI_FIZIGI_VE_YUZDURME_BUOYANCY.md)
  * [03_GOLGELENDIRME_VE_AYDINLATMA.md](03_GOLGELENDIRME_VE_AYDINLATMA.md)
  * [04_HAVA_DURUMU_VE_ORTAM_SISTEMI.md](04_HAVA_DURUMU_VE_ORTAM_SISTEMI.md)
  * [05_JUST_SAIL_UYARLAMA_REHBERI.md](05_JUST_SAIL_UYARLAMA_REHBERI.md) *(Hazır HLSL ve C# kodları)*
  * [SOT_DERIN_ANALIZ_VE_EFEKTLER.md](SOT_DERIN_ANALIZ_VE_EFEKTLER.md) *(Derin Su Fiziği, Spektral Renk Geçişleri, SSS ve Gemi Sıçrama Efektleri)*

---

## 2. [Over the Hill İncelemesi](over_the_hill/00_OVER_THE_HILL_GENEL_BAKIS_VE_MIMARI.md)
* **Geliştirici:** Funselektor Labs Inc. (*art of rally*, *Absolute Drift*)
* **Motor:** Unity 6 / URP 17 / IL2CPP (DirectX 12)
* **Odak:** İkonik diorama & tilt-shift sanat yönetimi, analog renk teorisi, COZY 3 atmosferi, Buto hacimsel sisi, kutup ışıkları (Aurora Borealis), arazi fiziği, Obi Rope vinç mekaniği ve Polaroid fotoğrafçılık sistemi.
* **Detaylı Raporlar:**
  * [00_OVER_THE_HILL_GENEL_BAKIS_VE_MIMARI.md](over_the_hill/00_OVER_THE_HILL_GENEL_BAKIS_VE_MIMARI.md)
  * [01_TARZ_TEMA_VE_GORSEL_DIL.md](over_the_hill/01_TARZ_TEMA_VE_GORSEL_DIL.md) *(En Önemli Bölüm: Sanat Tarzı, Renkler ve Diorama)*
  * [02_RENDER_GOLGELER_VE_POST_PROCESSING.md](over_the_hill/02_RENDER_GOLGELER_VE_POST_PROCESSING.md) *(URP, HBAO, Blob Shadows, DoF)*
  * [03_ATMOSFER_HAVA_DURUMU_VE_ISIKLANDIRMA.md](over_the_hill/03_ATMOSFER_HAVA_DURUMU_VE_ISIKLANDIRMA.md) *(COZY Weather, Buto Fog, Aurora, Kamp Ateşi)*
  * [04_ARAZI_VEGETASYON_VE_DOGAL_CEVRE.md](over_the_hill/04_ARAZI_VEGETASYON_VE_DOGAL_CEVRE.md) *(MicroVerse, MicroSplat, TVE rüzgârı, Impostors)*
  * [05_SU_VE_NEHIR_SISTEMI.md](over_the_hill/05_SU_VE_NEHIR_SISTEMI.md) *(Stylized Water 3, nehir geçişleri, su direnci)*
  * [06_ARAC_FIZIGI_VINC_VE_HALAT_MEKANIGI.md](over_the_hill/06_ARAC_FIZIGI_VINC_VE_HALAT_MEKANIGI.md) *(Süspansiyon fiziği ve dinamik Obi Rope vinç kablosu)*
  * [07_YABAN_HAYATI_VE_POLAROID_FOTOGRAF_SISTEMI.md](over_the_hill/07_YABAN_HAYATI_VE_POLAROID_FOTOGRAF_SISTEMI.md) *(Hayvan yapay zekâsı, vizör ve Polaroid albümü)*
  * [08_JUST_SAIL_ICIN_ILHAM_VE_UYARLAMA_STRATEJISI.md](over_the_hill/08_JUST_SAIL_ICIN_ILHAM_VE_UYARLAMA_STRATEJISI.md) *(Just Sail için diorama kamera ve deniz yaban hayatı planı)*
