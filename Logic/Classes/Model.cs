using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Logic.Classes
{
    public static class Model
    {
        private static readonly Dictionary<string, HiddenMarkovModel> hmmList = new();
        private static bool IsTrained;

        private static readonly Dictionary<string, int[]> bufferRecognitionStates = new();
        private static readonly Dictionary<string, int[]> bufferObservedStates = new();
        private static readonly Dictionary<string, string> bufferRecognitionWords = new();

        public static Dictionary<string, int[]> BufferRecognitionStates => bufferRecognitionStates;
        public static Dictionary<string, int[]> BufferObservedStates => bufferObservedStates;
        public static Dictionary<string, string> BufferRecognitionWords => bufferRecognitionWords;

        public static void Train()
        {
            hmmList.Clear();
            bufferRecognitionStates.Clear();
            bufferObservedStates.Clear();
            bufferRecognitionWords.Clear();

            DirectoryInfo audioDir = new(ProjectSettings.AudioTrainingFolderPath);

            List<double[]> mfccList = new();

            string[] audioFiles = Directory.GetFiles(audioDir.FullName, "*", SearchOption.AllDirectories).OrderBy(file => file).ToArray();
            foreach (var file in audioFiles)
            {
                AudioFile audioFile = new(file);
                List<double[]> mfcc = MelFrequencyCepstralCoefficients.CalculateMFCC(audioFile, ProjectSettings.FrameDurationMs, ProjectSettings.FrameOverlapDurationMs, ProjectSettings.NumFilters, ProjectSettings.NumCepstralCoefficients);
                mfccList.AddRange(mfcc);
            }

            Console.WriteLine("MFCC: Complite");

            KMeans.Train(mfccList);
            Console.WriteLine("KMeans: Complite");

            foreach (var subDirectory in audioDir.GetDirectories())
            {
                HiddenMarkovModel hmm = new(ProjectSettings.NumStates, ProjectSettings.NumSymbols);

                string[] subDirAudioFiles = Directory.GetFiles(subDirectory.FullName, "*").OrderBy(file => file).ToArray();
                foreach (var file in subDirAudioFiles)
                {
                    AudioFile audioFile = new(file);
                    List<double[]> mfcc = MelFrequencyCepstralCoefficients.CalculateMFCC(audioFile, ProjectSettings.FrameDurationMs, ProjectSettings.FrameOverlapDurationMs, ProjectSettings.NumFilters, ProjectSettings.NumCepstralCoefficients);
                    hmm.BaumWelchAlgorithm(mfcc);

                    Console.WriteLine("HMM: " + audioFile.Name);
                }

                hmmList.Add(subDirectory.Name, hmm);
            }

            IsTrained = true;
        }

        public static int[] Recognize()
        {
            if (!IsTrained)
            {
                Train();
            }

            if (BufferRecognitionStates.ContainsKey(ProjectSettings.AudioCurrent))
            {
                return BufferRecognitionStates[ProjectSettings.AudioCurrent];
            }

            AudioFile audioFile = new(ProjectSettings.AudioCurrent);
            List<double[]> mfcc = MelFrequencyCepstralCoefficients.CalculateMFCC(audioFile, ProjectSettings.FrameDurationMs, ProjectSettings.FrameOverlapDurationMs, ProjectSettings.NumFilters, ProjectSettings.NumCepstralCoefficients);

            string recognizedWord = RecognizeWord(mfcc);
            int[] recognitionStates = hmmList[recognizedWord].ViterbiAlgorithm(mfcc);
            BufferRecognitionStates.Add(ProjectSettings.AudioCurrent, recognitionStates);
            BufferRecognitionWords.Add(ProjectSettings.AudioCurrent, recognizedWord);

            return recognitionStates;
        }

        public static int[] Observe()
        {
            if (!IsTrained)
            {
                Train();
            }

            if (BufferObservedStates.ContainsKey(ProjectSettings.AudioCurrent))
            {
                return BufferObservedStates[ProjectSettings.AudioCurrent];
            }

            AudioFile audioFile = new(ProjectSettings.AudioCurrent);
            List<double[]> mfcc = MelFrequencyCepstralCoefficients.CalculateMFCC(audioFile, ProjectSettings.FrameDurationMs, ProjectSettings.FrameOverlapDurationMs, ProjectSettings.NumFilters, ProjectSettings.NumCepstralCoefficients);

            int[] observedStates = mfcc.Select(KMeans.PredictCluster).ToArray();
            BufferObservedStates.Add(ProjectSettings.AudioCurrent, observedStates);

            return observedStates;
        }

        private static string RecognizeWord(List<double[]> mfcc)
        {
            double highestScore = double.MinValue;
            string recognizedWord = "";

            foreach (var hmmModel in hmmList)
            {
                double score = hmmModel.Value.GetRecognitionScore(mfcc);
                if (score > highestScore)
                {
                    highestScore = score;
                    recognizedWord = hmmModel.Key;
                }
            }

            return recognizedWord;
        }
    }
}