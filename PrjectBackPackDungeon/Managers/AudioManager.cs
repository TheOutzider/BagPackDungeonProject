using System;
using Microsoft.Xna.Framework.Audio;

namespace PrjectBackPackDungeon;

public static class AudioManager
{
    private static Random _random = new Random();
    private const int SampleRate = 44100;

    /// <summary>
    /// Génère et joue un son d'impact "Crunchy" 16-bit (mélange de bruit blanc et onde carrée)
    /// </summary>
    public static void PlayRetroImpact()
    {
        float duration = 0.4f;
        int sampleCount = (int)(SampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = (float)Math.Pow(1.0f - t, 3); // Decay rapide

            // 1. Bruit blanc (le "Crunch")
            float noise = (float)(_random.NextDouble() * 2.0 - 1.0);

            // 2. Onde carrée descendante (le "Boom")
            float freq = 100f * (1.0f - t * 0.5f); 
            float square = Math.Sin(i * 2 * Math.PI * freq / SampleRate) > 0 ? 1.0f : -1.0f;

            // Mélange
            float mixed = (noise * 0.6f + square * 0.4f) * envelope;
            samples[i] = (short)(mixed * short.MaxValue * 0.5f);
        }

        PlayRawSamples(samples);
    }

    /// <summary>
    /// Petit "Pop" 8-bit pour l'apparition d'un dé
    /// </summary>
    public static void PlayRetroPop()
    {
        float duration = 0.1f;
        int sampleCount = (int)(SampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = 1.0f - t;
            
            // Onde carrée montante (Bip!)
            float freq = 400f + (t * 400f);
            float square = Math.Sin(i * 2 * Math.PI * freq / SampleRate) > 0 ? 1.0f : -1.0f;

            samples[i] = (short)(square * envelope * short.MaxValue * 0.2f);
        }

        PlayRawSamples(samples);
    }

    private static void PlayRawSamples(short[] samples)
    {
        byte[] buffer = new byte[samples.Length * 2];
        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);

        // On crée une instance jetable pour ce son
        var instance = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
        instance.SubmitBuffer(buffer);
        instance.Play();
        
        // Note: Dans un vrai projet, on recyclerait les instances, 
        // mais pour des sons courts et rares, le GC gère ça.
    }
}