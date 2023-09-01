using SpeechRecognition.AudioTraining.Numbers.Winstone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace SpeechRecognition
{
    public partial class MainWindow : Window
    {
        private readonly Dictionary<int, Brush> numberColors = new();

        private HiddenMarkovModel hmm;
        private int[] recognizedStates;

        public MainWindow()
        {
            InitializeComponent();
            Initializing();
        }

        private void Initializing()
        {
            ButtonRecognize.Visibility = Visibility.Hidden;

            PopulateComboBox();
            TextBoxFileName.Text = AudioData.AudioDefault;
            ComboBoxFileName.SelectedItem = AudioData.AudioDefault;
            ComboBoxFileName.SelectionChanged += ComboBoxFileName_SelectionChanged;
        }

        private void PopulateComboBox()
        {
            IEnumerable<string> mp3Files = AudioData.GetTrainingFilesDirectory(true);
            foreach (string mp3File in mp3Files)
            {
                ComboBoxFileName.Items.Add(mp3File);
            }
        }

        private void OutputRecognizedStates()
        {
            TextBlockRecognizedStates.Text = "";

            foreach (var item in recognizedStates)
            {
                var newTextBlock = new TextBlock { Text = $"{item} " };

                if (numberColors.TryGetValue(item, out var color))
                {
                    newTextBlock.Foreground = color;
                }
                else
                {
                    color = GenerateRandomColor();
                    numberColors[item] = color;
                    newTextBlock.Foreground = color;
                }

                TextBlockRecognizedStates.Inlines.Add(newTextBlock);
            }
        }

        private Brush GenerateRandomColor()
        {
            Random random = new();
            byte r = (byte)random.Next(100, 250);
            byte g = (byte)random.Next(100, 250);
            byte b = (byte)random.Next(100, 250);

            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        private void ButtonRecognize_Click(object sender, RoutedEventArgs e)
        {
            string input = TextBoxFileName.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Empty file name");
            }
            else
            {
                AudioFile audioFile = new(AudioData.GetTrainingFilesDirector(AudioData.AudioCurrent));
                var mfcc = MfccCalculator.CalculateMFCC(audioFile);
                recognizedStates = hmm.Recognize(mfcc);
                OutputRecognizedStates();
            }
        }

        private async void ButtonRetrain_Click(object sender, RoutedEventArgs e)
        {
            ButtonRecognize.Visibility = Visibility.Hidden;
            numberColors.Clear();

            int numDifferentNumbers = int.Parse(TextBoxNumDifferentNumbers.Text);
            string fileName = TextBoxFileName.Text.Trim();

            var cancellationTokenSource = new CancellationTokenSource();

            TimeSpan elapsedTime = new();
            Timer timer = new();
            timer.Interval = 1000;
            timer.Elapsed += (sender, e) =>
            {
                elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
                Dispatcher.InvokeAsync(() => LabelRetrainTime.Content = elapsedTime.ToString(@"hh\:mm\:ss"));
            };
            timer.Start();

            try
            {
                await RetrainAsync(numDifferentNumbers, fileName, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Retrain loop was cancelled");
            }
            finally
            {
                ButtonRecognize.Visibility = Visibility.Visible;
                timer.Stop();
                timer.Dispose();
            }
        }

        private async Task RetrainAsync(int numDifferentNumbers, string fileName, CancellationToken cancellationToken)
        {
            LabelNumOvertraining.Content = "NumOvertrain: 0";

            int numStates = int.Parse(TextBoxNumStates.Text);
            int numObservations = int.Parse(TextBoxNumObservations.Text);
            int numIterations = int.Parse(TextBoxNumIterations.Text);
            hmm = new HiddenMarkovModel(numStates, numObservations);
            IEnumerable<string> mp3Files = AudioData.GetTrainingFilesDirectory(false);

            int uniqueNumbers = 0;
            int numOvertraining = 0;

            while (numDifferentNumbers > uniqueNumbers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Run(() =>
                {
                    foreach (string mp3File in mp3Files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        AudioFile audioFile = new(mp3File);
                        var mfcc = MfccCalculator.CalculateMFCC(audioFile);
                        hmm.Train(mfcc);
                    }
                }, cancellationToken);

                if (!string.IsNullOrEmpty(fileName))
                {
                    var recognizerTask = Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        AudioFile audioFile = new(AudioData.GetTrainingFilesDirector(AudioData.AudioCurrent));
                        var mfcc = MfccCalculator.CalculateMFCC(audioFile);
                        recognizedStates = hmm.Recognize(mfcc);
                    }, cancellationToken);

                    await recognizerTask;

                    OutputRecognizedStates();
                    LabelNumOvertraining.Content = $"NumOvertrain: {int.Parse(LabelNumOvertraining.Content.ToString().Split(' ')[1]) + 1}";
                    uniqueNumbers = recognizedStates.Distinct().Count();
                }

                numOvertraining++;
            }

            int numClamped = uniqueNumbers < 0 ? 0 : numDifferentNumbers;
            TextBoxNumDifferentNumbers.Text = numClamped.ToString();
        }

        private void ComboBoxFileName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedFile = ComboBoxFileName.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedFile))
            {
                AudioData.AudioCurrent = selectedFile;
                TextBoxFileName.Text = selectedFile;
                AudioFile audioFile = new(AudioData.GetTrainingFilesDirector(AudioData.AudioCurrent));
                var mfcc = MfccCalculator.CalculateMFCC(audioFile);
                recognizedStates = hmm.Recognize(mfcc);
                OutputRecognizedStates();
            }
        }
    }
}