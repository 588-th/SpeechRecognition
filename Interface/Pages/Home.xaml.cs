using Common;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Logic.Classes;
using NAudio.Wave;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Interface.Pages
{
    public partial class Home : Page
    {
        public SolidColorPaint LegendTextPaint { get; set; } = new SolidColorPaint { Color = new SKColor(255, 255, 255) };
        private Dictionary<int, SKColor> _chartColors = new()
        {
            { 0, new SKColor(0,150,136) },
            { 1, new SKColor(0,188,212) },
            { 2, new SKColor(233,30,99) },
            { 3, new SKColor(255,193,7) },
            { 4, new SKColor(95,125,139) }
        };
        private Dictionary<string, ObservableCollection<ObservablePoint>> _recognizedStatesDictionary;
        private Dictionary<string, ObservableCollection<ObservablePoint>> _observedStatesDictionary;
        private WaveOutEvent _waveOut;

        private enum StateType
        {
            Recognized,
            Observed
        }

        public Home()
        {
            InitializeComponent();
            SetupEventHandlers();
            DataContext = this;
        }

        private void SetupEventHandlers()
        {
            FileExplorerRecognize.FileSelected += (_, __) => UpdateAudioFileInformation();
            FileExplorerRecognize.DoubleClick += async (_, __) => await OutputStates(StateType.Recognized);
            FileExplorerRecognize.DoubleClick += async (_, __) => await OutputStates(StateType.Observed);
            FileExplorerRecognize.DoubleClick += (_, __) => OutputRecognizedWord();
            ButtonRetrain.Click += async (_, __) => await Retrain();
            ButtonRecognize.Click += async (_, __) => await OutputStates(StateType.Recognized);
            ButtonRecognize.Click += async (_, __) => await OutputStates(StateType.Observed);
            ButtonRecognize.Click += (_, __) => OutputRecognizedWord();
            ButtonRecognizeAddAudio.Click += async (_, __) => await AddStates(StateType.Recognized);
            ButtonRecognizeAddAudio.Click += async (_, __) => await AddStates(StateType.Observed);
            ButtonRecognizeRemoveAudio.Click += (_, __) => RemoveStates(StateType.Recognized);
            ButtonRecognizeRemoveAudio.Click += (_, __) => RemoveStates(StateType.Observed);
            ButtonRecognizeShiftLeft.Click += (_, __) => ShiftGraph(StateType.Recognized, "left", 1);
            ButtonRecognizeShiftLeft.Click += (_, __) => ShiftGraph(StateType.Observed, "left", 1);
            ButtonRecognizeShiftRight.Click += (_, __) => ShiftGraph(StateType.Recognized, "right", 1);
            ButtonRecognizeShiftRight.Click += (_, __) => ShiftGraph(StateType.Observed, "right", 1);
            ButtonRecognizedStatesColorUpdate.Click += (_, __) => ChangeColors(StateType.Recognized);
            ButtonRecognizedStatesColorUpdate.Click += (_, __) => ChangeColors(StateType.Observed);
            ButtonPlayStopAudio.Click += (_, __) => PlayStopAudio();
            ButtonClearRecognizedWords.Click += (_, __) => ClearRecognizedWords();
        }

        private void UpdateAudioFileInformation()
        {
            ProjectSettings.AudioCurrent = FileExplorerRecognize.CurrentFile;
            AudioFile audioFile = new(ProjectSettings.AudioCurrent);
            TextBlockAudioFileInformation.Text = $"Name: {audioFile.Name}\nChannels: {audioFile.Channels}\nSampleRate: {audioFile.SampleRate}";
        }

        private void OutputRecognizedWord()
        {
            while (true)
            {
                Thread.Sleep(10);
                if (!Model.BufferRecognitionWords.ContainsKey(ProjectSettings.AudioCurrent))
                {
                    continue;
                }

                AudioFile audioFile = new(ProjectSettings.AudioCurrent);
                ListViewRecognizedWord.Items.Add(Model.BufferRecognitionWords[ProjectSettings.AudioCurrent]);
                ListViewRecognizedWordFileName.Items.Add(audioFile.Name);
                break;
            }
        }

        private async Task Retrain()
        {
            ButtonRetrain.IsEnabled = false;

            Stopwatch elapsedTime = new();
            System.Timers.Timer timer = new(1000);
            timer.Elapsed += (_, __) =>
            {
                elapsedTime.Start();
                Dispatcher.Invoke(() => LabelRetrainTime.Content = $"RetrainTime: {elapsedTime.Elapsed:hh\\:mm\\:ss}");
            };
            timer.Start();

            await Task.Run(Model.Train);
            await OutputStates(StateType.Recognized);
            await OutputStates(StateType.Observed);

            timer.Stop();
            timer.Dispose();

            LabelNumOvertraining.Content = $"NumOvertrain: {int.Parse(LabelNumOvertraining.Content.ToString().Split(' ')[1]) + 1}";

            ButtonRetrain.IsEnabled = true;
        }

        private async Task OutputStates(StateType stateType)
        {
            AudioFile audioFile = new(ProjectSettings.AudioCurrent);

            Func<int[]> recognizeOrObserve = stateType switch
            {
                StateType.Recognized => Model.Recognize,
                StateType.Observed => Model.Observe,
                _ => throw new ArgumentException(nameof(stateType))
            };

            int[] states = await Task.Run(recognizeOrObserve);

            ObservableCollection<ObservablePoint> points = new(states.Select((state, index) => new ObservablePoint(index, state)));

            Dictionary<string, ObservableCollection<ObservablePoint>> statesDictionary = new()
            {
                [audioFile.Name] = points
            };

            OutputChart(stateType, statesDictionary);
        }

        private async Task AddStates(StateType stateType)
        {
            AudioFile audioFile = new(ProjectSettings.AudioCurrent);
            Dictionary<string, ObservableCollection<ObservablePoint>> statesDictionary = stateType switch
            {
                StateType.Recognized => _recognizedStatesDictionary,
                StateType.Observed => _observedStatesDictionary,
                _ => throw new ArgumentException(nameof(stateType))
            };

            if (statesDictionary.ContainsKey(audioFile.Name))
            {
                return;
            }

            int[] states = stateType switch
            {
                StateType.Recognized => await Task.Run(Model.Recognize),
                StateType.Observed => await Task.Run(Model.Observe),
                _ => throw new ArgumentException(nameof(stateType))
            };

            ObservableCollection<ObservablePoint> points = new();
            for (int i = 0; i < states.Length - 1; i++)
            {
                points.Add(new ObservablePoint(i, states[i]));
            }

            statesDictionary.Add(audioFile.Name, points);
            OutputChart(stateType, statesDictionary);
        }

        private void RemoveStates(StateType stateType)
        {
            Dictionary<string, ObservableCollection<ObservablePoint>> statesDictionary = stateType switch
            {
                StateType.Recognized => _recognizedStatesDictionary,
                StateType.Observed => _observedStatesDictionary,
                _ => throw new ArgumentException(nameof(stateType))
            };

            AudioFile audioFile = new(ProjectSettings.AudioCurrent);

            if (statesDictionary.Remove(audioFile.Name))
            {
                OutputChart(stateType, statesDictionary);
            }
        }

        private void OutputChart(StateType stateType, Dictionary<string, ObservableCollection<ObservablePoint>> statesDictionary)
        {
            if (statesDictionary is null)
            {
                return;
            }

            var chartToUse = stateType switch
            {
                StateType.Recognized => ChartRecognizedStates,
                StateType.Observed => ChartObservedStates,
                _ => throw new ArgumentException("Invalid StateType")
            };

            if (_chartColors.Count < statesDictionary.Count)
            {
                GenerateRandomColors(_chartColors.Count, statesDictionary.Count - _chartColors.Count);
            }

            var statesSeries = CreateSeriesFromDictionary(statesDictionary);

            if (stateType == StateType.Recognized)
            {
                _recognizedStatesDictionary = statesDictionary;
            }
            else
            {
                _observedStatesDictionary = statesDictionary;
            }

            chartToUse.Series = statesSeries;
        }

        private ISeries[] CreateSeriesFromDictionary(Dictionary<string, ObservableCollection<ObservablePoint>> statesDictionary)
        {
            var statesSeries = new ISeries[statesDictionary.Count];
            int i = 0;
            foreach (var item in statesDictionary)
            {
                if (_chartColors.Count <= i)
                    GenerateRandomColors(i, 1);

                statesSeries[i] = new LineSeries<ObservablePoint>
                {
                    Name = item.Key,
                    Values = item.Value,
                    Stroke = new SolidColorPaint { Color = _chartColors[i], StrokeThickness = 4 },
                    GeometryStroke = new SolidColorPaint { Color = _chartColors[i], StrokeThickness = 3 },
                    GeometrySize = 0,
                    Fill = null
                };
                i++;
            }

            return statesSeries;
        }

        private void ShiftGraph(StateType stateType, string direction, int shiftAmount)
        {
            Dictionary<string, ObservableCollection<ObservablePoint>> statesDictionary = stateType switch
            {
                StateType.Recognized => _recognizedStatesDictionary,
                StateType.Observed => _observedStatesDictionary,
                _ => throw new ArgumentException(nameof(stateType))
            };

            if (statesDictionary == null || statesDictionary.Count == 0)
            {
                return;
            }

            AudioFile audio = new(ProjectSettings.AudioCurrent);
            string graphToShift = audio.Name;

            if (statesDictionary.TryGetValue(graphToShift, out var graphPoints))
            {
                for (int i = 0; i < graphPoints.Count; i++)
                {
                    if (direction.Equals("left", StringComparison.OrdinalIgnoreCase))
                    {
                        graphPoints[i].X -= shiftAmount;
                    }
                    else if (direction.Equals("right", StringComparison.OrdinalIgnoreCase))
                    {
                        graphPoints[i].X += shiftAmount;
                    }
                }

                OutputChart(stateType, statesDictionary);
            }
        }

        private void ChangeColors(StateType stateType)
        {
            Dictionary<string, ObservableCollection<ObservablePoint>> outputStates = stateType switch
            {
                StateType.Recognized => new Dictionary<string, ObservableCollection<ObservablePoint>>(_recognizedStatesDictionary),
                StateType.Observed => new Dictionary<string, ObservableCollection<ObservablePoint>>(_observedStatesDictionary),
                _ => throw new ArgumentException(nameof(stateType))
            };

            List<int[]> statesList = stateType switch
            {
                StateType.Recognized => new List<int[]>(Model.BufferRecognitionStates.Values),
                StateType.Observed => new List<int[]>(Model.BufferObservedStates.Values),
                _ => throw new ArgumentException(nameof(stateType))
            };

            _chartColors.Clear();
            GenerateRandomColors(0, statesList.Count - 1);

            OutputChart(StateType.Recognized, outputStates);
            OutputChart(StateType.Observed, outputStates);
        }

        private void GenerateRandomColors(int startIndex, int count)
        {
            Random random = new();
            for (int i = startIndex; i < count + startIndex; i++)
            {
                byte[] rgbValues = new byte[3];
                random.NextBytes(rgbValues);
                _chartColors[i] = new SKColor(rgbValues[0], rgbValues[1], rgbValues[2]);
            }
        }

        private void ClearRecognizedWords()
        {
            ListViewRecognizedWord.Items.Clear();
            ListViewRecognizedWordFileName.Items.Clear();
        }

        private void PlayStopAudio()
        {
            if (_waveOut == null)
            {
                _waveOut = new WaveOutEvent();
                _waveOut.Init(new AudioFileReader(ProjectSettings.AudioCurrent));
                _waveOut.Play();
                ButtonPlayStopAudio.ControlContent = "Stop";

                _waveOut.PlaybackStopped += (_, __) =>
                {
                    _waveOut?.Stop();
                    _waveOut?.Dispose();
                    _waveOut = null;
                    ButtonPlayStopAudio.ControlContent = "Play";
                };
            }
            else if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Stop();
            }
        }
    }
}