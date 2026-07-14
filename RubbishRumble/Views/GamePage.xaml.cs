using RubbishRumble.Models;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class GamePage : ContentPage
{
    private const double TrashSize = 56;
    private const double SpawnPadding = 10;

    private readonly GameViewModel _viewModel;
    private readonly List<FallingTrash> _activeTrash = new();
    private readonly Random _random = new();

    private IDispatcherTimer? _gameLoopTimer;
    private double _spawnAccumulator;
    private double _arenaWidth;
    private double _arenaHeight;

    public GamePage()
    {
        InitializeComponent();
        _viewModel = new GameViewModel();
        BindingContext = _viewModel;
        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
        Loaded += (_, _) => UpdateLayoutMetrics();

        if (SpawningArena != null)
            SpawningArena.SizeChanged += (_, _) => UpdateLayoutMetrics();
    }

    private void UpdateLayoutMetrics()
    {
        if (SpawningArena == null)
            return;

        _arenaWidth = SpawningArena.Width;
        _arenaHeight = SpawningArena.Height;
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        StopGameLoop();
        ClearActiveTrash();

        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Game init failed: {ex}");
        }

        UpdateLayoutMetrics();
        StartGameLoop();
    }

    private void OnDisappearing(object? sender, EventArgs e)
    {
        StopGameLoop();
        ClearActiveTrash();
    }

    private void StartGameLoop()
    {
        if (Dispatcher == null)
            return;

        _spawnAccumulator = 0;
        _gameLoopTimer = Dispatcher.CreateTimer();
        if (_gameLoopTimer == null)
            return;

        _gameLoopTimer.Interval = TimeSpan.FromMilliseconds(16);
        _gameLoopTimer.Tick += OnGameLoopTick;
        _gameLoopTimer.Start();
    }

    private void StopGameLoop()
    {
        if (_gameLoopTimer == null)
            return;

        _gameLoopTimer.Stop();
        _gameLoopTimer.Tick -= OnGameLoopTick;
        _gameLoopTimer = null;
    }

    private void OnGameLoopTick(object? sender, EventArgs e)
    {
        if (_viewModel.IsGameOver)
        {
            StopGameLoop();
            return;
        }

        if (_arenaWidth <= 0 || _arenaHeight <= 0)
            return;

        double deltaSeconds = _gameLoopTimer?.Interval.TotalSeconds ?? 0.016;

        _spawnAccumulator += deltaSeconds;
        double spawnInterval = _viewModel.GetSpawnIntervalSeconds();

        while (_spawnAccumulator >= spawnInterval)
        {
            _spawnAccumulator -= spawnInterval;
            SpawnTrashItem();
        }

        UpdateFallingTrash(deltaSeconds);
    }

    private void SpawnTrashItem()
    {
        if (SpawningArena == null || _arenaWidth <= 0 || _arenaHeight <= 0)
            return;

        TrashItem? trash = _viewModel.GetRandomTrash();
        if (trash == null)
            return;

        string imageSource = trash.GetMauiImageSource();
        if (string.IsNullOrWhiteSpace(imageSource))
            return;

        double maxX = Math.Max(SpawnPadding, _arenaWidth - TrashSize - SpawnPadding);
        double startX = _random.NextDouble() * (maxX - SpawnPadding) + SpawnPadding;

        var image = new Image
        {
            Source = imageSource,
            Aspect = Aspect.AspectFit,
            InputTransparent = false,
            WidthRequest = TrashSize,
            HeightRequest = TrashSize
        };

        var fallingTrash = new FallingTrash(trash, image)
        {
            X = startX,
            Y = 0
        };

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += (_, args) => OnTrashPanned(fallingTrash, args);
        image.GestureRecognizers.Add(panGesture);

        SpawningArena.Children.Add(image);
        _activeTrash.Add(fallingTrash);
        SetBounds(fallingTrash);
    }

    private void UpdateFallingTrash(double deltaSeconds)
    {
        if (_viewModel.IsTrashFrozen())
            return;

        double fallSpeed = _viewModel.GetFallSpeedPixelsPerSecond();
        double fallDistance = fallSpeed * deltaSeconds;

        foreach (FallingTrash fallingTrash in _activeTrash.ToList())
        {
            if (fallingTrash.IsDragging)
                continue;

            fallingTrash.Y += fallDistance;

            if (fallingTrash.Y >= _arenaHeight - TrashSize)
            {
                RemoveTrash(fallingTrash);
                _viewModel.OnTrashMissed();
                continue;
            }

            SetBounds(fallingTrash);
        }
    }

    private void SetBounds(FallingTrash fallingTrash)
    {
        if (fallingTrash.Image?.Parent == null)
            return;

        AbsoluteLayout.SetLayoutBounds(
            fallingTrash.Image,
            new Rect(fallingTrash.X, fallingTrash.Y, TrashSize, TrashSize));
    }

    private void OnTrashPanned(FallingTrash fallingTrash, PanUpdatedEventArgs args)
    {
        if (!_activeTrash.Contains(fallingTrash) || fallingTrash.Image == null)
            return;

        try
        {
            switch (args.StatusType)
            {
                case GestureStatus.Started:
                    fallingTrash.IsDragging = true;
                    fallingTrash.Image.TranslationX = 0;
                    fallingTrash.Image.TranslationY = 0;
                    break;

                case GestureStatus.Running:
                    if (!fallingTrash.IsDragging)
                        fallingTrash.IsDragging = true;

                    double maxOffsetX = Math.Max(0, _arenaWidth - TrashSize) - fallingTrash.X;
                    double maxOffsetY = Math.Max(0, _arenaHeight - TrashSize) - fallingTrash.Y;

                    fallingTrash.Image.TranslationX = Math.Clamp(args.TotalX, -fallingTrash.X, maxOffsetX);
                    fallingTrash.Image.TranslationY = Math.Clamp(args.TotalY, -fallingTrash.Y, maxOffsetY);
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    CommitDrag(fallingTrash);
                    fallingTrash.IsDragging = false;

                    if (!TrySortTrash(fallingTrash))
                        SetBounds(fallingTrash);

                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Drag error: {ex}");
            if (_activeTrash.Contains(fallingTrash))
            {
                fallingTrash.IsDragging = false;
                fallingTrash.Image.TranslationX = 0;
                fallingTrash.Image.TranslationY = 0;
                SetBounds(fallingTrash);
            }
        }
    }

    private void CommitDrag(FallingTrash fallingTrash)
    {
        if (fallingTrash.Image == null)
            return;

        fallingTrash.X = Math.Clamp(
            fallingTrash.X + fallingTrash.Image.TranslationX,
            0,
            Math.Max(0, _arenaWidth - TrashSize));
        fallingTrash.Y = Math.Clamp(
            fallingTrash.Y + fallingTrash.Image.TranslationY,
            0,
            Math.Max(0, _arenaHeight - TrashSize));

        fallingTrash.Image.TranslationX = 0;
        fallingTrash.Image.TranslationY = 0;
        SetBounds(fallingTrash);
    }

    private bool TrySortTrash(FallingTrash fallingTrash)
    {
        if (!_activeTrash.Contains(fallingTrash))
            return true;

        string? droppedCategory = GetBinCategoryAt(fallingTrash);
        if (droppedCategory == null)
            return false;

        string itemCategory = fallingTrash.Trash.Category ?? string.Empty;

        if (droppedCategory == itemCategory)
        {
            RemoveTrash(fallingTrash);
            _viewModel.OnTrashSorted(fallingTrash.Trash);
            return true;
        }

        RemoveTrash(fallingTrash);
        _viewModel.OnTrashMissed();
        return true;
    }

    private string? GetBinCategoryAt(FallingTrash fallingTrash)
    {
        if (_arenaWidth <= 0 || _arenaHeight <= 0)
            return null;

        if (fallingTrash.Y + TrashSize < _arenaHeight * 0.65)
            return null;

        double trashCenterX = fallingTrash.X + (TrashSize / 2);
        double ratio = trashCenterX / _arenaWidth;

        if (ratio < 0.25)
            return "Recyclables";

        if (ratio < 0.5)
            return "Biodegradable";

        if (ratio < 0.75)
            return "Biohazard";

        return "Landfill";
    }

    private void RemoveTrash(FallingTrash fallingTrash)
    {
        fallingTrash.IsDragging = false;

        if (fallingTrash.Image?.Parent is Layout parent)
            parent.Children.Remove(fallingTrash.Image);

        _activeTrash.Remove(fallingTrash);
    }

    private void ClearActiveTrash()
    {
        foreach (FallingTrash fallingTrash in _activeTrash.ToList())
            RemoveTrash(fallingTrash);
    }

    private sealed class FallingTrash
    {
        public FallingTrash(TrashItem trash, Image image)
        {
            Trash = trash;
            Image = image;
        }

        public TrashItem Trash { get; }
        public Image Image { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsDragging { get; set; }
    }
}
