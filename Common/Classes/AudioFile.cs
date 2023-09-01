using NAudio.Wave;
using System;
using System.Linq;

namespace Common
{
    public class AudioFile
    {
        public AudioFile(string audioFilePath)
        {
            ReadAudio(audioFilePath);
        }

        public string Path { get; private set; }
        public string Name { get; private set; }
        public int Channels { get; private set; }
        public int SampleRate { get; private set; }
        public double[] Content { get; private set; }

        private void ReadAudio(string audioFilePath)
        {
            using var reader = new AudioFileReader(audioFilePath);
            var audioData = new float[reader.Length / sizeof(float)];
            reader.Read(audioData, 0, audioData.Length);

            Path = audioFilePath;
            Name = System.IO.Path.GetFileName(audioFilePath);
            Channels = reader.WaveFormat.Channels;
            SampleRate = reader.WaveFormat.SampleRate;
            Content = audioData.Select(sample => (double)sample).ToArray();

            Normalize();
        }

        public void Normalize()
        {
            int blockSize = SampleRate;
            int numBlocks = (int)Math.Ceiling(Content.Length / (double)blockSize);

            double targetLoudnessLUFS = -23.0;

            for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
            {
                int startIndex = blockIndex * blockSize;
                int endIndex = Math.Min(startIndex + blockSize, Content.Length);

                double[] blockData = new double[endIndex - startIndex];
                Array.Copy(Content, startIndex, blockData, 0, blockData.Length);

                double rms = CalculateRMS(blockData);

                double loudnessLUFS = 20 * Math.Log10(rms);

                double correctionFactor = Math.Pow(10.0, (targetLoudnessLUFS - loudnessLUFS) / 20.0);

                for (int i = startIndex; i < endIndex; i++)
                {
                    Content[i] *= correctionFactor;
                }
            }
        }

        private double CalculateRMS(double[] data)
        {
            double sum = 0.0;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i] * data[i];
            }
            double meanSquare = sum / data.Length;
            return Math.Sqrt(meanSquare);
        }
    }
}