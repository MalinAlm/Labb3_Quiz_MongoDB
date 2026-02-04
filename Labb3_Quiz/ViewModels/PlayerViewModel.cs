// File: ViewModels/PlayerViewModel.cs

using Labb3_Quiz.Command;
using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Services;
using Labb3_Quiz_MongoDB.Data.Mongo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Labb3_Quiz.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        // Timer for per-question countdown (UI)
        private readonly DispatcherTimer _questionCountdownTimer;
        private int _remainingSecondsForCurrentQuestion;

        // Total run timing (VG: total speltid)
        private readonly Stopwatch _runStopwatch = new();

        // Shuffling
        private static readonly Random _random = new();
        private List<QuestionViewModel> _shuffledQuestions = new();

        // Track progress
        private int _currentQuestionIndexInRun;   // 0-based index in _shuffledQuestions
        private int _currentQuestionIndexInPack;  // 0-based index in ActivePack.Questions (stable for stats)

        // VG: Who is playing
        private string _playerName = string.Empty;
        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = (value ?? string.Empty).Trim();
                RaisePropertyChanged();
            }
        }

        // VG: Track what the player chose per question (stable by question index in pack)
        private readonly List<RunAnswerEntry> _runAnswers = new();

        // Services (VG)
        // NOTE: Kept self-contained so this file compiles without requiring more plumbing.
        private readonly MongoQuizRunService _quizRunService;

        // Cancellation for the "3 second feedback pause"
        private CancellationTokenSource? _feedbackDelayCancellationTokenSource;

        // ===== Bindable state =====

        private QuestionViewModel? _activeQuestion;
        public QuestionViewModel? ActiveQuestion
        {
            get => _activeQuestion;
            private set
            {
                _activeQuestion = value;
                RaisePropertyChanged();
            }
        }

        private bool _canAnswer = true;
        public bool CanAnswer
        {
            get => _canAnswer;
            private set
            {
                _canAnswer = value;
                RaisePropertyChanged();
                AnswerCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _correctAnswer;
        public string? CorrectAnswer
        {
            get => _correctAnswer;
            private set
            {
                _correctAnswer = value;
                RaisePropertyChanged();
            }
        }

        private string? _clickedAnswer;
        public string? ClickedAnswer
        {
            get => _clickedAnswer;
            private set
            {
                _clickedAnswer = value;
                RaisePropertyChanged();
            }
        }

        private List<string> _answerOptions = new() { "", "", "", "" };
        public List<string> AnswerOptions
        {
            get => _answerOptions;
            private set
            {
                _answerOptions = value;
                RaisePropertyChanged();
            }
        }

        private bool _quizFinished;
        public bool QuizFinished
        {
            get => _quizFinished;
            private set
            {
                _quizFinished = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ResultText));
            }
        }

        private int _score;
        public int Score
        {
            get => _score;
            private set
            {
                _score = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ResultText));
            }
        }

        private string _timerText = string.Empty;
        public string TimerText
        {
            get => _timerText;
            private set
            {
                _timerText = value;
                RaisePropertyChanged();
            }
        }

        private string _questionProgressText = string.Empty;
        public string QuestionProgressText
        {
            get => _questionProgressText;
            private set
            {
                _questionProgressText = value;
                RaisePropertyChanged();
            }
        }

        private string _feedbackText = string.Empty;
        public string FeedbackText
        {
            get => _feedbackText;
            private set
            {
                _feedbackText = value;
                RaisePropertyChanged();
            }
        }

        private string _feedbackHeaderText = string.Empty;
        public string FeedbackHeaderText
        {
            get => _feedbackHeaderText;
            private set { _feedbackHeaderText = value; RaisePropertyChanged(); }
        }

        private string _feedbackStatsText = string.Empty;
        public string FeedbackStatsText
        {
            get => _feedbackStatsText;
            private set { _feedbackStatsText = value; RaisePropertyChanged(); }
        }


        private Brush _feedbackColor = Brushes.Black;
        public Brush FeedbackColor
        {
            get => _feedbackColor;
            private set
            {
                _feedbackColor = value;
                RaisePropertyChanged();
            }
        }

        // VG: show Top5 after run finished (kept inside ResultText => no UI changes required)
        private string _top5Text = string.Empty;
        public string Top5Text
        {
            get => _top5Text;
            private set
            {
                _top5Text = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ResultText));
            }
        }

        public string ResultText
        {
            get
            {
                var totalQuestions = ActivePack?.Questions.Count ?? 0;
                var baseLine = $"You got {Score} out of {totalQuestions} Correct!";
                if (string.IsNullOrWhiteSpace(Top5Text))
                    return baseLine;

                return baseLine + "\n\nTop 5:\n" + Top5Text;
            }
        }

        public DelegateCommand AnswerCommand { get; }
        public DelegateCommand RestartCommand { get; }

        public QuestionPackViewModel? ActivePack => _mainWindowViewModel?.ActivePack;

        public PlayerViewModel(MainWindowViewModel? mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;

            _remainingSecondsForCurrentQuestion = 30;
            _questionCountdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.0) };
            _questionCountdownTimer.Tick += QuestionCountdownTimer_Tick;

            AnswerCommand = new DelegateCommand(SelectAnswer, _ => CanAnswer);
            RestartCommand = new DelegateCommand(_ => RestartQuiz());

            // Build QuizRun service (self-contained; uses same DB name/connection)
            var settings = new MongoSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "HenrikMalin"
            };

            var mongoDbContext = new MongoDbContext(settings);
            var runRepository = new MongoQuizRunRepository(mongoDbContext);
            _quizRunService = new MongoQuizRunService(runRepository);
        }

        // ===== Public lifecycle API (called by MainWindowViewModel) =====

        public void StartQuiz()
        {
            if (ActivePack == null || !ActivePack.Questions.Any())
                return;

            QuizFinished = false;
            Score = 0;
            Top5Text = string.Empty;

            FeedbackHeaderText = string.Empty;
            FeedbackStatsText = string.Empty;
            FeedbackColor = Brushes.Black;

            ClickedAnswer = null;
            CorrectAnswer = null;

            _runAnswers.Clear();
            _currentQuestionIndexInRun = 0;
            _currentQuestionIndexInPack = 0;

            _runStopwatch.Reset();
            _runStopwatch.Start();

            _shuffledQuestions = ShuffleQuestions(ActivePack.Questions);
            LoadNextQuestion();
        }

        // Called when user exits quiz view without finishing (mid-quiz quit)
        public void CancelRun()
        {
            _runStopwatch.Stop();
            _runAnswers.Clear();

            CancelPendingFeedbackDelay();
            StopQuestionTimerAndClearUi();
        }

        public void RestartQuiz()
        {
            // Keeps same PlayerName; main flow prompts before Play anyway.
            CancelRun();
            StartQuiz();
        }

        // ===== Internal helpers =====

        private void QuestionCountdownTimer_Tick(object? sender, EventArgs e)
        {
            if (_remainingSecondsForCurrentQuestion > 0)
            {
                _remainingSecondsForCurrentQuestion--;
                TimerText = _remainingSecondsForCurrentQuestion.ToString();
                return;
            }

            _questionCountdownTimer.Stop();
            _ = HandleTimedOutQuestionAsync();
        }

        private static List<QuestionViewModel> ShuffleQuestions(IEnumerable<QuestionViewModel> questions)
        {
            return questions
                .OrderBy(_ => _random.Next())
                .ToList();
        }

        private async Task HandleTimedOutQuestionAsync()
        {
            if (ActiveQuestion == null)
            {
                LoadNextQuestion();
                return;
            }

            // Record a "no answer" as empty string
            RecordAnswerForStats(selectedAnswerText: string.Empty);

            FeedbackHeaderText = "Time's up!";
            FeedbackStatsText = string.Empty;
            FeedbackColor = Brushes.OrangeRed;


            await PauseForFeedbackAsync();

            FeedbackHeaderText = string.Empty;
            FeedbackStatsText = string.Empty;
            FeedbackColor = Brushes.Black;


            LoadNextQuestion();
        }

        private async void SelectAnswer(object? selected)
        {
            if (!CanAnswer)
                return;

            if (ActiveQuestion == null || selected is not string selectedAnswerText)
                return;

            CanAnswer = false;
            ClickedAnswer = selectedAnswerText;
            CorrectAnswer = ActiveQuestion.CorrectAnswer;

            _questionCountdownTimer.Stop();

            var isCorrect = string.Equals(selectedAnswerText, ActiveQuestion.CorrectAnswer, StringComparison.Ordinal);

            FeedbackHeaderText = BuildFeedbackHeader(isCorrect, ActiveQuestion.CorrectAnswer);
            FeedbackColor = isCorrect ? Brushes.LightGreen : Brushes.Red;

            var packId = ActivePack?.Model?.Id;
            if (!string.IsNullOrWhiteSpace(packId))
            {
                var optionCountsByAnswerText = await _quizRunService.GetAnswerCountsAsync(
                    packId: packId,
                    questionIndexInPack: _currentQuestionIndexInPack);

                FeedbackStatsText = BuildFeedbackStats(optionCountsByAnswerText);
            }
            else
            {
                FeedbackStatsText = string.Empty;
            }


            //FeedbackColor = isCorrect ? Brushes.LightGreen : Brushes.Red;

            await PauseForFeedbackAsync();

            ClickedAnswer = null;
            CorrectAnswer = null;

            FeedbackHeaderText = string.Empty;
            FeedbackStatsText = string.Empty;
            FeedbackColor = Brushes.Black;


            LoadNextQuestion();
            CanAnswer = true;
        }

        private void RecordAnswerForStats(string selectedAnswerText)
        {
            var stableIndex = _currentQuestionIndexInPack;
            _runAnswers.Add(new RunAnswerEntry(stableIndex, selectedAnswerText ?? string.Empty));
        }

        private async Task PauseForFeedbackAsync()
        {
            CancelPendingFeedbackDelay();

            _feedbackDelayCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _feedbackDelayCancellationTokenSource.Token;

            try
            {
                await Task.Delay(3000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ok: user exited mid-feedback
            }
        }

        private void CancelPendingFeedbackDelay()
        {
            if (_feedbackDelayCancellationTokenSource == null)
                return;

            _feedbackDelayCancellationTokenSource.Cancel();
            _feedbackDelayCancellationTokenSource.Dispose();
            _feedbackDelayCancellationTokenSource = null;
        }

        private void LoadNextQuestion()
        {
            if (ActivePack == null || !_shuffledQuestions.Any())
                return;

            if (_currentQuestionIndexInRun >= _shuffledQuestions.Count)
            {
                _ = FinishQuizAsync();
                return;
            }

            ActiveQuestion = _shuffledQuestions[_currentQuestionIndexInRun];

            // stable pack index (used for stats aggregation + run storage)
            _currentQuestionIndexInPack = FindQuestionIndexInPack(ActivePack, ActiveQuestion);

            _currentQuestionIndexInRun++;

            var allAnswers = new List<string>
            {
                ActiveQuestion.CorrectAnswer,
                ActiveQuestion.IncorrectAnswer1,
                ActiveQuestion.IncorrectAnswer2,
                ActiveQuestion.IncorrectAnswer3
            };

            AnswerOptions = allAnswers
                .OrderBy(_ => _random.Next())
                .ToList();

            _remainingSecondsForCurrentQuestion = ActivePack.TimeLimitInSeconds > 0
                ? ActivePack.TimeLimitInSeconds
                : 30;

            TimerText = _remainingSecondsForCurrentQuestion.ToString();
            _questionCountdownTimer.Start();

            QuestionProgressText = $"Question {_currentQuestionIndexInRun} of {ActivePack.Questions.Count}";
        }

        private static int FindQuestionIndexInPack(QuestionPackViewModel packViewModel, QuestionViewModel questionViewModel)
        {
            var index = packViewModel.Questions.IndexOf(questionViewModel);
            return index < 0 ? 0 : index;
        }

        private async Task FinishQuizAsync()
        {
            _questionCountdownTimer.Stop();
            _runStopwatch.Stop();

            TimerText = "Quiz Complete!";
            ActiveQuestion = null;
            QuizFinished = true;

            // VG #1: Save completed run (ONLY if pack has an Id)
            var packId = ActivePack?.Model?.Id;
            if (!string.IsNullOrWhiteSpace(packId))
            {
                var totalSeconds = (int)Math.Round(_runStopwatch.Elapsed.TotalSeconds, MidpointRounding.AwayFromZero);

                await _quizRunService.SaveCompletedRunAsync(
                    packId: packId,
                    playerName: PlayerName,
                    totalTimeSeconds: totalSeconds,
                    correctCount: Score,
                    answers: _runAnswers
                        .Select(a => new QuizRunAnswerDto(a.QuestionIndexInPack, a.ChosenAnswerText))
                        .ToList());

                // VG #1: Refresh Top5 on result screen
                var top5Entries = await _quizRunService.GetTop5Async(packId);
                Top5Text = FormatTop5(top5Entries);

                // Best-effort: refresh "ActivePackHasRuns" on MainWindowViewModel.
                // (This method is private there, so we use safe reflection to avoid changing that file again.)
                await RefreshMainWindowRunStateBestEffortAsync();
            }
        }

        private async Task RefreshMainWindowRunStateBestEffortAsync()
        {
            if (_mainWindowViewModel == null)
                return;

            try
            {
                var methodInfo = _mainWindowViewModel.GetType().GetMethod(
                    "RefreshActivePackHasRunsAsync",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (methodInfo == null)
                    return;

                var result = methodInfo.Invoke(_mainWindowViewModel, Array.Empty<object>());
                if (result is Task task)
                    await task;
            }
            catch
            {
                // Ignore: this is best-effort only, app still works if we can't refresh instantly.
            }
        }

        private void StopQuestionTimerAndClearUi()
        {
            _questionCountdownTimer.Stop();

            ActiveQuestion = null;
            AnswerOptions = new List<string> { "", "", "", "" };

            ClickedAnswer = null;
            CorrectAnswer = null;

            FeedbackHeaderText = string.Empty;
            FeedbackStatsText = string.Empty;
            FeedbackColor = Brushes.Black;


            _currentQuestionIndexInRun = 0;
            _currentQuestionIndexInPack = 0;

            _remainingSecondsForCurrentQuestion = 0;
            TimerText = string.Empty;
            QuestionProgressText = string.Empty;

            CanAnswer = true;
            QuizFinished = false;
        }

        private static string BuildFeedbackHeader(bool isCorrect, string correctAnswerText)
        {
            return isCorrect
                ? "Correct answer!"
                : $"Incorrect answer! Correct was: {correctAnswerText}";
        }

        private static string BuildFeedbackStats(Dictionary<string, int> optionCountsByAnswerText)
        {
            var lines = optionCountsByAnswerText
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}: {x.Value}")
                .ToList();

            if (lines.Count == 0)
                return "(No previous players yet)";

            return "Players picked:\n" + string.Join("\n", lines);
        }


        private static string FormatTop5(List<Top5EntryDto> top5Entries)
        {
            if (top5Entries.Count == 0)
                return "(No runs yet)";

            var lines = new List<string>();
            for (int i = 0; i < top5Entries.Count; i++)
            {
                var entry = top5Entries[i];
                lines.Add($"{i + 1}. {entry.PlayerName} — {entry.CorrectCount} correct — {entry.TotalTimeSeconds}s");
            }

            return string.Join("\n", lines);
        }

        private sealed record RunAnswerEntry(int QuestionIndexInPack, string ChosenAnswerText);
    }
}
