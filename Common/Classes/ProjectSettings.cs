using Common.Classes;

namespace Common
{
    public static class ProjectSettings
    {
        #region Fields

        // Audio
        private static string _audioCurrent = "D:\\Desktop\\VisualStudio\\SpeechRecognition\\Common\\Training\\Audio\\Ноль\\Саня_Ноль-01.wav";
        private static string _audioDefault = "D:\\Desktop\\VisualStudio\\SpeechRecognition\\Common\\Training\\Audio\\Ноль\\Саня_Ноль-01.wav";
        private static string _audioTrainingFolderPath = "D:\\Desktop\\VisualStudio\\SpeechRecognition\\Common\\Training\\Audio\\";
        private static string _audioRecognizeFolderPath = "D:\\Desktop\\VisualStudio\\SpeechRecognition\\Common\\Recognize\\Audio\\";

        // Mel Frequency Cepstral Coefficients
        private static int _numFilters = 26;
        private static int _numCepstralCoefficients = 12;
        private static int _frameDurationMs = 20;
        private static int _frameOverlapDurationMs = 10;

        // Hidden Markov Model
        private static int _numStates = 20;
        private static int _numSymbols = 20;

        // Gaussian Mixture Model
        private static int _numClusters = 20;

        #endregion

        #region Accessors

        // Audio
        public static string AudioCurrent
        {
            get { return _audioCurrent; }
            set { if (ValidationData.FileExist(value)) _audioCurrent = value; }
        }

        public static string AudioDefault
        {
            get { return _audioDefault; }
            set { if (ValidationData.FileExist(value)) _audioDefault = value; }
        }

        public static string AudioTrainingFolderPath
        {
            get { return _audioTrainingFolderPath; }
            set { if (ValidationData.FileExist(value)) _audioTrainingFolderPath = value; }
        }

        public static string AudioRecognizeFolderPath
        {
            get { return _audioRecognizeFolderPath; }
            set { if (ValidationData.FileExist(value)) _audioRecognizeFolderPath = value; }
        }

        // Mel Frequency Cepstral Coefficients
        public static int NumFilters
        {
            get { return _numFilters; }
            set { if (ValidationData.AboveZero(value)) _numFilters = value; }
        }
        public static int NumCepstralCoefficients
        {
            get { return _numCepstralCoefficients; }
            set { if (ValidationData.AboveZero(value)) _numCepstralCoefficients = value; }
        }
        public static int FrameDurationMs
        {
            get { return _frameDurationMs; }
            set { if (ValidationData.AboveZero(value)) _frameDurationMs = value; }
        }
        public static int FrameOverlapDurationMs
        {
            get { return _frameOverlapDurationMs; }
            set { if (ValidationData.AboveZero(value)) _frameOverlapDurationMs = value; }
        }

        // Hidden Markov Model
        public static int NumStates
        {
            get { return _numStates; }
            set { if (ValidationData.AboveZero(value)) _numStates = value; }
        }

        public static int NumSymbols
        {
            get { return _numSymbols; }
            set { if (ValidationData.AboveZero(value)) _numSymbols = value; }
        }

        // Gaussian Mixture Model
        public static int NumClusters
        {
            get { return _numClusters; }
            set { if (ValidationData.AboveZero(value)) _numClusters = value; }
        }

        #endregion
    }
}