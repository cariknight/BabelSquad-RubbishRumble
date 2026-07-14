using Microsoft.Maui.Layouts;
using RubbishRumble.Models;
using RubbishRumble.ViewModels;
#if ANDROID
using Android.Views;
using Microsoft.Maui.Platform;
#endif
using RubbishRumble.Services;

namespace RubbishRumble.Views;

public partial class GamePage : ContentPage
{
    private const double TrashSize = 56;
    private const double TouchPadding = 16;
    private const double HitSize = TrashSize + TouchPadding * 2;
    private const double SpawnPadding = 10;

    private readonly GameViewModel _viewModel;
    private readonly List<FallingTrash> _activeTrash = new();
    private readonly Random _random = new();

    private IDispatcherTimer? _gameLoopTimer;
    private Grid? _touchLayer;
    private FallingTrash? _draggingTrash;
    private double _grabOffsetX;
    private double _grabOffsetY;
    private Point? _lastTouchPoint;
    private double _spawnAccumulator;
    private double _arenaWidth;
    private double _arenaHeight;
    private bool _isPageActive;

    public GamePage()
    {
        InitializeComponent();
        _viewModel = new GameViewModel();
        BindingContext = _viewModel;
        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
        Loaded += (_, _) =>
        {
            UpdateLayoutMetrics();
            SetupArenaTouchLayer();
        };

        if (SpawningArena != null)
            SpawningArena.SizeChanged += (_, _) => UpdateLayoutMetrics();
    }

    private void SetupArenaTouchLayer()
    {
        if (SpawningArena == null || _touchLayer != null)
            return;

        _touchLayer = new Grid
        {
            BackgroundColor = Colors.Transparent,
            InputTransparent = false,
            ZIndex = 1000
        };

        AbsoluteLayout.SetLayoutFlags(_touchLayer, AbsoluteLayoutFlags.All);
        AbsoluteLayout.SetLayoutBounds(_touchLayer, new Rect(0, 0, 1, 1));

#if !ANDROID
        var pointer = new PointerGestureRecognizer();
        pointer.PointerPressed += OnArenaPointerPressed;
        pointer.PointerMoved += OnArenaPointerMoved;
        pointer.PointerReleased += OnArenaPointerReleased;
        _touchLayer.GestureRecognizers.Add(pointer);

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnArenaPanUpdated;
        _touchLayer.GestureRecognizers.Add(pan);
#endif

        _touchLayer.HandlerChanged += OnTouchLayerHandlerChanged;
        SpawningArena.Children.Add(_touchLayer);
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
        _isPageActive = true;
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
        SetupArenaTouchLayer();
        StartGameLoop();
    }

    private void OnDisappearing(object? sender, EventArgs e)
    {
        _isPageActive = false;
        _draggingTrash = null;
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
            InputTransparent = true,
            IsEnabled = false,
            WidthRequest = TrashSize,
            HeightRequest = TrashSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var container = new Grid
        {
            WidthRequest = HitSize,
            HeightRequest = HitSize,
            BackgroundColor = Colors.Transparent,
            InputTransparent = true
        };
        container.Children.Add(image);

        var fallingTrash = new FallingTrash(trash, container, image)
        {
            X = startX,
            Y = 0
        };

        SpawningArena.Children.Add(container);
        if (_touchLayer != null)
            _touchLayer.ZIndex = 1000;

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

            if (_viewModel.TryAutoSortTrash(fallingTrash.Trash, fallingTrash.Y + TrashSize, _arenaHeight))
            {
                RemoveTrash(fallingTrash);
                continue;
            }

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
        if (SpawningArena == null || fallingTrash.View == null)
            return;

        if (fallingTrash.View.Parent != SpawningArena)
            return;

        double layoutX = Math.Max(0, fallingTrash.X - TouchPadding);
        double layoutY = Math.Max(0, fallingTrash.Y - TouchPadding);

        AbsoluteLayout.SetLayoutBounds(
            fallingTrash.View,
            new Rect(layoutX, layoutY, HitSize, HitSize));
    }

    private void OnArenaPointerPressed(object? sender, PointerEventArgs e)
    {
        Point? point = e.GetPosition(SpawningArena);
        if (point == null)
            return;

        _lastTouchPoint = point;
        StartDragAt(point.Value);
    }

    private void OnArenaPointerMoved(object? sender, PointerEventArgs e)
    {
        Point? point = e.GetPosition(SpawningArena);
        if (point == null)
            return;

        _lastTouchPoint = point;
        ContinueDragAt(point.Value);
    }

    private void OnArenaPointerReleased(object? sender, PointerEventArgs e)
    {
        Point? point = e.GetPosition(SpawningArena);
        _lastTouchPoint = point;
        EndDragAt(point);
    }

    private void OnArenaPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (e == null)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                if (_draggingTrash == null && _lastTouchPoint.HasValue)
                    StartDragAt(_lastTouchPoint.Value);
                break;

            case GestureStatus.Running:
                if (_draggingTrash == null)
                {
                    if (_lastTouchPoint.HasValue)
                        StartDragAt(_lastTouchPoint.Value);
                    else
                        return;
                }

                ApplyDragTranslation(_draggingTrash!, e.TotalX, e.TotalY);
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_draggingTrash == null)
                    return;

                ApplyDragTranslation(_draggingTrash, e.TotalX, e.TotalY);
                EndDragAt(_lastTouchPoint);
                break;
        }
    }

    private void OnTouchLayerHandlerChanged(object? sender, EventArgs e)
    {
#if ANDROID
        if (_touchLayer?.Handler?.PlatformView is Android.Views.View nativeView)
        {
            nativeView.Touch -= OnAndroidTouch;
            nativeView.Touch += OnAndroidTouch;
        }
#endif
    }

#if ANDROID
    private void OnAndroidTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        if (e.Event == null || SpawningArena == null)
            return;

        double density = DeviceDisplay.MainDisplayInfo.Density;
        if (density <= 0)
            density = 1;

        // BOTH axes must be converted from raw pixels to DIPs ? this was the bug.
        var point = new Point(e.Event.GetX() / density, e.Event.GetY() / density);
        _lastTouchPoint = point;

        switch (e.Event.ActionMasked)
        {
            case MotionEventActions.Down:
                StartDragAt(point);
                break;

            case MotionEventActions.Move:
                ContinueDragAt(point);
                break;

            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                EndDragAt(point);
                break;
        }

        e.Handled = _draggingTrash != null;
    }
#endif

    private void StartDragAt(Point point)
    {
        if (!_isPageActive || _draggingTrash != null)
            return;

        FallingTrash? trash = HitTestTrash(point);
        if (trash?.View == null)
            return;

        _draggingTrash = trash;
        trash.IsDragging = true;
        trash.DragOriginX = trash.X;
        trash.DragOriginY = trash.Y;
        _grabOffsetX = point.X - trash.X;
        _grabOffsetY = point.Y - trash.Y;
        trash.View.TranslationX = 0;
        trash.View.TranslationY = 0;
        BringToFront(trash);
    }

    private void ContinueDragAt(Point point)
    {
        if (_draggingTrash?.View == null)
            return;

        SetDragTarget(_draggingTrash, point.X - _grabOffsetX, point.Y - _grabOffsetY);
    }

    private void ApplyDragTranslation(FallingTrash fallingTrash, double totalX, double totalY)
    {
        if (fallingTrash.View == null)
            return;

        double maxX = Math.Max(0, _arenaWidth - TrashSize);
        double maxY = Math.Max(0, _arenaHeight - TrashSize);

        double targetX = Math.Clamp(fallingTrash.DragOriginX + totalX, 0, maxX);
        double targetY = Math.Clamp(fallingTrash.DragOriginY + totalY, 0, maxY);

        fallingTrash.View.TranslationX = targetX - fallingTrash.DragOriginX;
        fallingTrash.View.TranslationY = targetY - fallingTrash.DragOriginY;
    }

    private void SetDragTarget(FallingTrash fallingTrash, double targetX, double targetY)
    {
        if (fallingTrash.View == null)
            return;

        double maxX = Math.Max(0, _arenaWidth - TrashSize);
        double maxY = Math.Max(0, _arenaHeight - TrashSize);

        targetX = Math.Clamp(targetX, 0, maxX);
        targetY = Math.Clamp(targetY, 0, maxY);

        fallingTrash.View.TranslationX = targetX - fallingTrash.DragOriginX;
        fallingTrash.View.TranslationY = targetY - fallingTrash.DragOriginY;
    }

    private void EndDragAt(Point? point)
    {
        if (_draggingTrash == null)
            return;

        try
        {
            if (point.HasValue)
                ContinueDragAt(point.Value);

            CommitDrag(_draggingTrash);

            FallingTrash trash = _draggingTrash;
            _draggingTrash = null;
            trash.IsDragging = false;

            if (_activeTrash.Contains(trash) && !TrySortTrash(trash))
                SetBounds(trash);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Drag error: {ex}");
            if (_draggingTrash != null)
            {
                _draggingTrash.IsDragging = false;
                if (_draggingTrash.View != null)
                {
                    _draggingTrash.View.TranslationX = 0;
                    _draggingTrash.View.TranslationY = 0;
                    _draggingTrash.View.ZIndex = 0;
                }

                SetBounds(_draggingTrash);
                _draggingTrash = null;
            }
        }
    }

    private void CommitDrag(FallingTrash fallingTrash)
    {
        if (fallingTrash.View == null || SpawningArena == null || fallingTrash.View.Parent != SpawningArena)
            return;

        double maxX = Math.Max(0, _arenaWidth - TrashSize);
        double maxY = Math.Max(0, _arenaHeight - TrashSize);

        fallingTrash.X = Math.Clamp(fallingTrash.DragOriginX + fallingTrash.View.TranslationX, 0, maxX);
        fallingTrash.Y = Math.Clamp(fallingTrash.DragOriginY + fallingTrash.View.TranslationY, 0, maxY);

        fallingTrash.View.TranslationX = 0;
        fallingTrash.View.TranslationY = 0;
        fallingTrash.View.ZIndex = 0; // release elevated z-order so it stops winning future hit-tests
        SetBounds(fallingTrash);
    }

    private FallingTrash? HitTestTrash(Point point)
    {
        FallingTrash? best = null;
        int bestZIndex = int.MinValue;

        foreach (FallingTrash trash in _activeTrash)
        {
            if (!ContainsPoint(trash, point))
                continue;

            int zIndex = trash.View?.ZIndex ?? 0;
            if (best == null || zIndex >= bestZIndex)
            {
                best = trash;
                bestZIndex = zIndex;
            }
        }

        return best;
    }

    private bool ContainsPoint(FallingTrash fallingTrash, Point point)
    {
        double left = fallingTrash.X - TouchPadding;
        double top = fallingTrash.Y - TouchPadding;

        return point.X >= left
            && point.X <= left + HitSize
            && point.Y >= top
            && point.Y <= top + HitSize;
    }

    private void BringToFront(FallingTrash fallingTrash)
    {
        if (SpawningArena == null || fallingTrash.View == null)
            return;

        foreach (IView child in SpawningArena.Children)
        {
            if (child is VisualElement element && child != _touchLayer)
                element.ZIndex = 0;
        }

        fallingTrash.View.ZIndex = 1;
        if (_touchLayer != null)
            _touchLayer.ZIndex = 1000;
    }

    private bool TrySortTrash(FallingTrash fallingTrash)
    {
        if (!_activeTrash.Contains(fallingTrash))
            return true;

        bool sorted = _viewModel.TryManualSortTrash(
            fallingTrash.Trash,
            fallingTrash.X + (TrashSize / 2),
            fallingTrash.Y + TrashSize,
            _arenaWidth,
            _arenaHeight);

        if (!sorted)
            return false;

        RemoveTrash(fallingTrash);
        return true;
    }

    private void RemoveTrash(FallingTrash fallingTrash)
    {
        if (_draggingTrash == fallingTrash)
            _draggingTrash = null;

        fallingTrash.IsDragging = false;

        if (fallingTrash.View?.Parent is Layout parent)
            parent.Children.Remove(fallingTrash.View);

        _activeTrash.Remove(fallingTrash);
    }

    private void ClearActiveTrash()
    {
        _draggingTrash = null;

        foreach (FallingTrash fallingTrash in _activeTrash.ToList())
            RemoveTrash(fallingTrash);
    }

    private sealed class FallingTrash
    {
        public FallingTrash(TrashItem trash, VisualElement view, Image image)
        {
            Trash = trash;
            View = view;
            Image = image;
        }

        public TrashItem Trash { get; }
        public VisualElement View { get; }
        public Image Image { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DragOriginX { get; set; }
        public double DragOriginY { get; set; }
        public bool IsDragging { get; set; }
    }

    private async void OnPauseButtonClicked(object sender, EventArgs e)
    {
        await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
    }

    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}