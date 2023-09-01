using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common
{
    public static class AudioData
    {
        public static string AudioCurrent
        {
            get { return _audioCurrent; }
            set { if (value != null) _audioCurrent = value; }
        }

        private static string _audioCurrent =
            "Winston_Two.mp3";

        public static readonly string AudioDefault =
            "Winston_Two.mp3";

        public static readonly string AudioTrainingFolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VisualStudio", "SpeechRecognition", "Training");

        public static readonly string AudioRecognizeFolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VisualStudio", "SpeechRecognition", "Recognize");

        public static IEnumerable<string> GetTrainingFilesDirectory(bool onlyFileName)
        {
            return onlyFileName ?
                Directory.GetFiles(AudioTrainingFolderPath, "*", SearchOption.AllDirectories).OrderBy(file => file).Select(Path.GetFileName) :
                Directory.GetFiles(AudioTrainingFolderPath, "*", SearchOption.AllDirectories).OrderBy(file => file);
        }

        public static IEnumerable<string> GetRecognizeFilesDirectory(bool onlyFileName)
        {
            return onlyFileName ?
                Directory.GetFiles(AudioRecognizeFolderPath, "*", SearchOption.AllDirectories).OrderBy(file => file).Select(Path.GetFileName) :
                Directory.GetFiles(AudioRecognizeFolderPath, "*", SearchOption.AllDirectories).OrderBy(file => file);
        }

        public static string GetTrainingFilesDirector(string fileName)
        {
            return Directory.GetFiles(AudioTrainingFolderPath, fileName, SearchOption.AllDirectories)[0];
        }

        public static string GetRecognizeFilesDirector(string fileName)
        {
            return Directory.GetFiles(AudioRecognizeFolderPath, fileName)[0];
        }
    }
}
