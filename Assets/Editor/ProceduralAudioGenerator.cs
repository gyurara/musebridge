using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 악기별로 다른 파형을 가진 8음계(도~도) WAV 파일을 프로시저럴 생성
/// </summary>
public static class ProceduralAudioGenerator
{
    public enum Waveform
    {
        Sine, Square, Sawtooth, Triangle, Pluck, Noise, Bell, Breath,
        // 확장 파형
        Organ,      // 다중 사인 하모닉스 (오르간)
        Brass,      // 스퀘어 + 램프 (트럼펫/튜바)
        StringPad,  // 레이어드 사인 + 느린 어택 (첼로/하프)
        Marimba,    // 사인 + 빠른 디케이 (마림바)
        WoodBlock,  // 짧은 노이즈 버스트 (우드블록)
        Whistle,    // 순수 고음 사인 + 비브라토 (휘슬)
        Sitar,      // 플럭 + 버징 오버톤 (시타르/밴조)
        Kalimba,    // 사인 + 약한 홀수배음 (칼림바)
        SynthPad,   // PWM 스퀘어 + 디튠 (신스패드)
        Accordion,  // 리드 오르간 (아코디언)
        Harmonica,  // 사인 + 리드 진동 (하모니카)
        Pizzicato,  // 빠른 어택 + 빠른 디케이 (피치카토)
    }


    public struct InstrumentVoice
    {
        public string id;
        public Waveform waveform;
        public float duration;
        public float attack;
        public float decay;
        public float octaveShift;
        public float noiseMix;
    }

    public static readonly string[] NoteNames = { "do", "re", "mi", "fa", "sol", "la", "ti", "do2" };

    // C4 ~ C5 주파수
    private static readonly float[] NoteFrequencies =
    {
        261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 440.00f, 493.88f, 523.25f
    };

    private const int SampleRate = 44100;

    public static AudioClip[] EnsureNoteClips(string instrumentId, InstrumentVoice voice, string outputFolder)
    {
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            var parent = Path.GetDirectoryName(outputFolder).Replace('\\', '/');
            var leaf = Path.GetFileName(outputFolder);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        var clips = new AudioClip[NoteNames.Length];
        for (int i = 0; i < NoteNames.Length; i++)
        {
            string path = $"{outputFolder}/{instrumentId}_{NoteNames[i]}.wav";
            if (!File.Exists(path))
            {
                float freq = NoteFrequencies[i] * Mathf.Pow(2f, voice.octaveShift);
                var samples = Synthesize(freq, voice);
                WavFileWriter.Write(path, samples, SampleRate);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return clips;
    }

    private static float[] Synthesize(float freq, InstrumentVoice voice)
    {
        int sampleCount = (int)(SampleRate * voice.duration);
        var data = new float[sampleCount];
        var rng = new System.Random();

        float phase = 0f;
        float phaseInc = 2f * Mathf.PI * freq / SampleRate;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float env = Envelope(t, voice);
            float sample = Oscillate(phase, voice.waveform, freq, t);

            if (voice.noiseMix > 0f)
            {
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                sample = Mathf.Lerp(sample, noise, voice.noiseMix);
            }

            data[i] = sample * env * 0.8f;
            phase += phaseInc;
            if (phase > 2f * Mathf.PI) phase -= 2f * Mathf.PI;
        }
        return data;
    }

    private static float Envelope(float t, InstrumentVoice voice)
    {
        if (t < voice.attack)
            return t / Mathf.Max(0.001f, voice.attack);

        float release = voice.duration - voice.attack;
        float decayT = (t - voice.attack) / Mathf.Max(0.001f, release);
        return Mathf.Pow(1f - Mathf.Clamp01(decayT), voice.decay);
    }

    private static float Oscillate(float phase, Waveform wave, float freq, float t)
    {
        switch (wave)
        {
            case Waveform.Sine:
                return Mathf.Sin(phase);
            case Waveform.Square:
                return Mathf.Sin(phase) >= 0 ? 0.6f : -0.6f;
            case Waveform.Sawtooth:
                return 2f * (phase / (2f * Mathf.PI)) - 1f;
            case Waveform.Triangle:
                return 2f * Mathf.Abs(2f * (phase / (2f * Mathf.PI)) - 1f) - 1f;
            case Waveform.Pluck:
                // 기타 느낌: 사인 + 2배음 감쇠
                return Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 2f) * 0.3f;
            case Waveform.Noise:
                return UnityEngine.Random.Range(-1f, 1f);
            case Waveform.Bell:
                // 벨 느낌: 사인 + 고배음
                return Mathf.Sin(phase) * 0.6f + Mathf.Sin(phase * 3.5f) * 0.3f + Mathf.Sin(phase * 5.2f) * 0.1f;
            case Waveform.Breath:
                // 플루트 느낌: 사인 + 약한 비브라토
                float vib = Mathf.Sin(2f * Mathf.PI * 5f * t) * 0.02f;
                return Mathf.Sin(phase * (1f + vib));

            case Waveform.Organ:
                // 오르간: 기본음 + 2배음 + 3배음 + 4배음
                return Mathf.Sin(phase) * 0.4f + Mathf.Sin(phase * 2f) * 0.25f
                     + Mathf.Sin(phase * 3f) * 0.2f + Mathf.Sin(phase * 4f) * 0.15f;

            case Waveform.Brass:
                // 트럼펫/튜바: 스퀘어 + 톱니파 블렌드
                float sq = Mathf.Sin(phase) >= 0 ? 0.5f : -0.5f;
                float saw = 2f * (phase / (2f * Mathf.PI)) - 1f;
                return sq * 0.6f + saw * 0.4f;

            case Waveform.StringPad:
                // 첼로/하프: 레이어드 사인파 + 미세 디튠
                return Mathf.Sin(phase) * 0.5f + Mathf.Sin(phase * 1.003f) * 0.3f
                     + Mathf.Sin(phase * 0.997f) * 0.2f;

            case Waveform.Marimba:
                // 마림바: 사인 + 약한 2배음
                return Mathf.Sin(phase) * 0.8f + Mathf.Sin(phase * 4f) * 0.2f;

            case Waveform.WoodBlock:
                // 우드블록: 노이즈 + 고주파 사인
                return UnityEngine.Random.Range(-1f, 1f) * 0.5f + Mathf.Sin(phase * 6f) * 0.5f;

            case Waveform.Whistle:
                // 휘슬: 순수 사인 + 강한 비브라토
                float wVib = Mathf.Sin(2f * Mathf.PI * 6f * t) * 0.04f;
                return Mathf.Sin(phase * (1f + wVib));

            case Waveform.Sitar:
                // 시타르/밴조: 플럭 + 버징 오버톤
                return Mathf.Sin(phase) * 0.5f + Mathf.Sin(phase * 2f) * 0.25f
                     + Mathf.Sin(phase * 7.1f) * 0.15f + Mathf.Sin(phase * 11.3f) * 0.1f;

            case Waveform.Kalimba:
                // 칼림바: 사인 + 홀수배음
                return Mathf.Sin(phase) * 0.6f + Mathf.Sin(phase * 3f) * 0.25f
                     + Mathf.Sin(phase * 5f) * 0.15f;

            case Waveform.SynthPad:
                // 신스패드: PWM 느낌 스퀘어 + 디튠
                float pw = 0.3f + Mathf.Sin(2f * Mathf.PI * 0.5f * t) * 0.2f;
                float norm = (phase % (2f * Mathf.PI)) / (2f * Mathf.PI);
                return (norm < pw ? 0.6f : -0.6f) * 0.5f + Mathf.Sin(phase * 1.01f) * 0.3f;

            case Waveform.Accordion:
                // 아코디언: 리드 오르간 느낌
                return Mathf.Sin(phase) * 0.35f + Mathf.Sin(phase * 2f) * 0.3f
                     + Mathf.Sin(phase * 3f) * 0.2f + (Mathf.Sin(phase) >= 0 ? 0.15f : -0.15f);

            case Waveform.Harmonica:
                // 하모니카: 사인 + 리드 진동 노이즈
                float reed = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.1f;
                return Mathf.Sin(phase) * 0.7f + reed + UnityEngine.Random.Range(-0.05f, 0.05f);

            case Waveform.Pizzicato:
                // 피치카토: 사인 + 2배음, 빠른 감쇠는 envelope에서 처리
                return Mathf.Sin(phase) * 0.6f + Mathf.Sin(phase * 2f) * 0.3f
                     + Mathf.Sin(phase * 3f) * 0.1f;
        }
        return 0f;
    }
}

/// <summary>
/// 16-bit PCM WAV 파일 writer
/// </summary>
public static class WavFileWriter
{
    public static void Write(string path, float[] samples, int sampleRate)
    {
        const int channels = 1;
        const int bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int dataSize = samples.Length * 2;

        using (var fs = new FileStream(path, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);                            // PCM chunk size
            bw.Write((short)1);                      // PCM format
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write((short)(channels * bitsPerSample / 8));
            bw.Write((short)bitsPerSample);

            // data chunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(dataSize);
            for (int i = 0; i < samples.Length; i++)
            {
                short s = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
                bw.Write(s);
            }
        }
    }
}
