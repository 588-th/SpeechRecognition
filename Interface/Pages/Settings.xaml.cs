using Common;
using System.Windows.Controls;

namespace Interface.Pages
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            LoadSettings();

            ButtonApply.Click += (_, __) => ButtonApply_Click();
        }

        private void LoadSettings()
        {
            TextBoxAudioCurrent.Text = ProjectSettings.AudioCurrent;
            TextBoxAudioDefault.Text = ProjectSettings.AudioDefault;
            TextBoxAudioTrainingFolderPath.Text = ProjectSettings.AudioTrainingFolderPath;
            TextBoxAudioRecognizeFolderPath.Text = ProjectSettings.AudioRecognizeFolderPath;

            TextBoxNumFilters.Text = ProjectSettings.NumFilters.ToString();
            TextBoxNumCepstralCoefficients.Text = ProjectSettings.NumCepstralCoefficients.ToString();
            TextBoxFrameDurationMs.Text = ProjectSettings.FrameDurationMs.ToString();
            TextBoxFrameOverlapDurationMs.Text = ProjectSettings.FrameOverlapDurationMs.ToString();

            TextBoxNumStates.Text = ProjectSettings.NumStates.ToString();
            TextBoxNumSymbols.Text = ProjectSettings.NumSymbols.ToString();

            TextBoxNumClusters.Text = ProjectSettings.NumClusters.ToString();
        }

        private void ButtonApply_Click()
        {
            ProjectSettings.AudioCurrent = TextBoxAudioCurrent.Text.Trim();
            ProjectSettings.AudioDefault = TextBoxAudioDefault.Text.Trim();
            ProjectSettings.AudioTrainingFolderPath = TextBoxAudioTrainingFolderPath.Text.Trim();
            ProjectSettings.AudioRecognizeFolderPath = TextBoxAudioRecognizeFolderPath.Text.Trim();

            ProjectSettings.NumFilters = int.Parse(TextBoxNumFilters.Text.Trim());
            ProjectSettings.NumCepstralCoefficients = int.Parse(TextBoxNumCepstralCoefficients.Text.Trim());
            ProjectSettings.FrameDurationMs = int.Parse(TextBoxFrameDurationMs.Text.Trim());
            ProjectSettings.FrameOverlapDurationMs = int.Parse(TextBoxFrameOverlapDurationMs.Text.Trim());

            ProjectSettings.NumStates = int.Parse(TextBoxNumStates.Text.Trim());
            ProjectSettings.NumSymbols = int.Parse(TextBoxNumSymbols.Text.Trim());

            ProjectSettings.NumClusters = int.Parse(TextBoxNumClusters.Text.Trim());
        }
    }
}