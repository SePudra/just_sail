using UnityEngine;

namespace JustSail
{
    public sealed class SeaAudio : MonoBehaviour
    {
        public Rigidbody boat;
        public Transform dock;
        public bool menuOpen;

        private const int SampleRate = 22050;
        private const float ClipSeconds = 12f;
        private AudioSource waterSource;
        private AudioSource windSource;
        private AudioSource dockSource;
        private AudioClip waterClip;
        private AudioClip windClip;
        private AudioClip creakClip;
        private AudioSource rainSource;
        private AudioClip rainClip;
        private VoyageAtmosphere atmosphere;

        private void Awake()
        {
            waterClip = MakeNoiseClip("Procedural Water", 1847, 0.075f, 0.18f, 0.55f);
            windClip = MakeNoiseClip("Procedural Wind", 9271, 0.035f, 0.12f, 0.9f);
            creakClip = MakeCreakClip();
            rainClip = MakeNoiseClip("Procedural Rain", 9412, .28f, .05f, 6f);
            atmosphere = FindFirstObjectByType<VoyageAtmosphere>();

            waterSource = gameObject.AddComponent<AudioSource>();
            Configure(waterSource, waterClip, 0.17f);
            windSource = gameObject.AddComponent<AudioSource>();
            Configure(windSource, windClip, 0.10f);
            rainSource=gameObject.AddComponent<AudioSource>();Configure(rainSource,rainClip,0);
        }

        private void Update()
        {
            float speed = boat != null ? new Vector2(boat.linearVelocity.x,boat.linearVelocity.z).magnitude : 0f;
            float storm=atmosphere?atmosphere.Storm:0;
            float speed01 = Mathf.Clamp01(speed / 12f);
            float target = menuOpen ? 0.42f : 1f;
            float blend = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 2.5f);

            if (dockSource == null && dock != null)
            {
                var sourceObject = new GameObject("Dock creaks");
                sourceObject.transform.SetParent(dock, false);
                sourceObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                dockSource = sourceObject.AddComponent<AudioSource>();
                Configure(dockSource, creakClip, 0.045f);
                dockSource.spatialBlend = 1f;
                dockSource.minDistance = 3f;
                dockSource.maxDistance = 40f;
            }

            waterSource.volume = Mathf.Lerp(waterSource.volume, (0.14f + speed01 * 0.09f) * target, blend);
            waterSource.pitch = Mathf.Lerp(waterSource.pitch, 0.94f + speed01 * 0.12f, blend);
            windSource.volume = Mathf.Lerp(windSource.volume, (0.085f+storm*.12f) * target, blend);
            rainSource.volume=Mathf.Lerp(rainSource.volume,storm*.14f*target,blend);
            if (dockSource != null)
                dockSource.volume = Mathf.Lerp(dockSource.volume, 0.045f * target, blend);
        }

        private static void Configure(AudioSource source, AudioClip clip, float volume)
        {
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.volume = volume;
            source.Play();
        }

        private static AudioClip MakeNoiseClip(string name, int seed, float filter, float modulation, float tone)
        {
            int count = Mathf.RoundToInt(SampleRate * ClipSeconds);
            var samples = new float[count];
            var random = new System.Random(seed);
            float filtered = 0f;
            float slow = 0f;
            float filterStep = Mathf.Clamp01(filter);
            float slowStep = 0.0008f;

            for (int i = 0; i < count; i++)
            {
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += (noise - filtered) * filterStep;
                slow += (filtered - slow) * slowStep;
                float wave = 0.5f + 0.5f * Mathf.Sin(i * (2f * Mathf.PI * tone / SampleRate));
                samples[i] = (filtered - slow) * (0.62f + modulation * wave);
            }

            // A short smooth fade makes the loop endpoints meet at silence.
            int fade = SampleRate / 50;
            for (int i = 0; i < fade; i++)
            {
                float t = (float)i / (fade - 1);
                float smooth = t * t * (3f - 2f * t);
                samples[i] *= smooth;
                samples[count - fade + i] *= 1f - smooth;
            }

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip MakeCreakClip()
        {
            const int seconds = 36;
            int count = SampleRate * seconds;
            var samples = new float[count];
            var random = new System.Random(3811);
            int start = SampleRate * 3;
            while (start < count - SampleRate)
            {
                int length = SampleRate / 3 + random.Next(SampleRate / 8);
                float frequency = 105f + (float)random.NextDouble() * 55f;
                float phase = (float)random.NextDouble() * 6.2831853f;
                float gain = 0.7f + (float)random.NextDouble() * 0.3f;
                int end = Mathf.Min(start + length, count);
                for (int i = start; i < end; i++)
                {
                    float t = (float)(i - start) / length;
                    float envelope = Mathf.Sin(t * Mathf.PI) * (1f - t) * gain;
                    float tone = Mathf.Sin(phase + 2f * Mathf.PI * frequency * t * length / SampleRate);
                    float overtone = Mathf.Sin(phase * 0.7f + 2f * Mathf.PI * frequency * 1.48f * t * length / SampleRate);
                    float tick = (float)(random.NextDouble() * 2.0 - 1.0);
                    samples[i] += envelope * (tone * 0.16f + overtone * 0.04f + tick * 0.018f);
                }
                start += SampleRate * (4 + random.Next(6));
            }

            var clip = AudioClip.Create("Procedural Dock Creaks", count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (dockSource != null) Destroy(dockSource.gameObject);
            if (waterClip != null) Destroy(waterClip);
            if (windClip != null) Destroy(windClip);
            if (creakClip != null) Destroy(creakClip);
            if (rainClip != null) Destroy(rainClip);
        }
    }
}
