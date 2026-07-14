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
        SpawningArena.SizeChanged += OnArenaSizeChanged;
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        StopGameLoop();
        ClearActiveTrash();
        await _viewModel.InitializeAsync();
        StartGameLoop();
    }

    private void OnDisappearing(object? sender, EventArgs e)
    {
        StopGameLoop();
        ClearActiveTrash();
    }

    private void OnArenaSizeChanged(object? sender, EventArgs e)
    {
        _arenaWidth = SpawningArena.Width;
        _arenaHeight = SpawningArena.Height;
    }

    private void StartGameLoop()
    {
        _spawnAccumulator = 0;
        _gameLoopTimer = Dispatcher.CreateTimer();
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
            InputTransparent = false
        };

        var fallingTrash = new FallingTrash(trash, image)
        {
            X = startX,
            Y = 0
        };

        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += (sender, args) => OnTrashPanned(fallingTrash, args);
        image.GestureRecognizers.Add(panGesture);

        AbsoluteLayout.SetLayoutBounds(image, new Rect(fallingTrash.X, fallingTrash.Y, TrashSize, TrashSize));

        SpawningArena.Children.Add(image);
        _activeTrash.Add(fallingTrash);
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

            AbsoluteLayout.SetLayoutBounds(
                fallingTrash.Image,
                new Rect(fallingTrash.X, fallingTrash.Y, TrashSize, TrashSize));
        }
    }

    private void OnTrashPanned(FallingTrash fallingTrash, PanUpdatedEventArgs args)
    {
        switch (args.StatusType)
        {
            case GestureStatus.Started:
                fallingTrash.IsDragging = true;
                break;

            case GestureStatus.Running:
                fallingTrash.X = Math.Clamp(
                    fallingTrash.X + args.TotalX - fallingTrash.LastPanX,
                    0,
                    Math.Max(0, _arenaWidth - TrashSize));
                fallingTrash.Y = Math.Clamp(
                    fallingTrash.Y + args.TotalY - fallingTrash.LastPanY,
                    0,
                    Math.Max(0, _arenaHeight - TrashSize));
                fallingTrash.LastPanX = args.TotalX;
                fallingTrash.LastPanY = args.TotalY;

                AbsoluteLayout.SetLayoutBounds(
                    fallingTrash.Image,
                    new Rect(fallingTrash.X, fallingTrash.Y, TrashSize, TrashSize));
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                fallingTrash.IsDragging = false;
                fallingTrash.LastPanX = 0;
                fallingTrash.LastPanY = 0;
                TrySortTrash(fallingTrash);
                break;
        }
    }

    private void TrySortTrash(FallingTrash fallingTrash)
    {
        string? droppedCategory = GetBinCategoryAt(fallingTrash);

        if (droppedCategory == null)
            return;

        if (droppedCategory == fallingTrash.Trash.Category)
        {
            RemoveTrash(fallingTrash);
            _viewModel.OnTrashSorted(fallingTrash.Trash);
            return;
        }

        RemoveTrash(fallingTrash);
        _viewModel.OnTrashMissed();
    }

    private string? GetBinCategoryAt(FallingTrash fallingTrash)
    {
        double trashCenterX = fallingTrash.X + (TrashSize / 2);
        double trashBottom = fallingTrash.Y + TrashSize;

        if (trashBottom < _arenaHeight * 0.65)
            return null;

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
        SpawningArena.Children.Remove(fallingTrash.Image);
        _activeTrash.Remove(fallingTrash);
    }

    private void ClearActiveTrash()
    {
        foreach (FallingTrash fallingTrash in _activeTrash)
            SpawningArena.Children.Remove(fallingTrash.Image);

        _activeTrash.Clear();
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
        public double LastPanX { get; set; }
        public double LastPanY { get; set; }
    }
}
