using RubbishRumble.Services;

namespace RubbishRumble.Views
{
    public partial class TutorialPage : ContentPage
    {
        public class TutorialSlide
        {
            public string Title { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string DescPrefix { get; set; } = string.Empty;
            public string CoinAmount { get; set; } = string.Empty;
            public string DescSuffix { get; set; } = string.Empty;
            public bool HasNumberFormatting => !string.IsNullOrEmpty(CoinAmount);
        }

        private readonly List<TutorialSlide> _slides;
        private bool _carouselInitialized;

        public TutorialPage()
        {
            InitializeComponent();

            _slides =
            [
                new TutorialSlide
                {
                    Title = "Swipe left or right\r\nto move items",
                    Description = "Match trash with\r\nits correct bin!",
                    ImageUrl = "tutorial_slide1.png"
                },
                new TutorialSlide
                {
                    Title = "Blue bin for\r\nRecyclables",
                    Description = "Green bin for\r\nBiodegradable",
                    ImageUrl = "tutorial_slide2.png"
                },
                new TutorialSlide
                {
                    Title = "Red bin for\r\nBiohazard",
                    Description = "Gray bin for\r\nLandfill",
                    ImageUrl = "tutorial_slide3.png"
                },
                new TutorialSlide
                {
                    Title = "You start with 3 lives",
                    Description = "Use power-ups wisely!",
                    ImageUrl = "tutorial_slide4.png"
                },
                new TutorialSlide
                {
                    Title = "Earn coins per item",
                    Description = "Points depend on\r\ntrash rarity",
                    ImageUrl = "tutorial_slide5.png"
                },
                new TutorialSlide
                {
                    Title = "Common Trash =\r\nBasic Points",
                    Description = "Worth 5 coins\r\nFound most often",
                    ImageUrl = "tutorial_common.png"
                },
                new TutorialSlide
                {
                    Title = "Uncommon Trash = Better Points",
                    Description = "Worth 10 coins\r\nAppears sometimes",
                    ImageUrl = "tutorial_uncommon.png"
                },
                new TutorialSlide
                {
                    Title = "Rare Trash = Big Points",
                    Description = "Worth 20 coins\r\nHard to find",
                    ImageUrl = "tutorial_rare.png"
                },
                new TutorialSlide
                {
                    Title = "Epic Trash = Jackpot!",
                    Description = "Worth 50 coins\r\nExtremely rare",
                    ImageUrl = "tutorial_epic.png"
                },
                new TutorialSlide
                {
                    Title = "You're ready!",
                    Description = "Let's keep the world clean together",
                    ImageUrl = "tutorial_slide6.png"
                }
            ];

            TutorialCarousel.PositionChanged += OnCarouselPositionChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (_carouselInitialized)
                return;

            _carouselInitialized = true;
            InitializeCarousel();
        }

        private void InitializeCarousel()
        {
#if WINDOWS
            // Defer binding until after Shell navigation completes on WinUI.
            Dispatcher.Dispatch(() =>
            {
                TutorialCarousel.ItemsSource = _slides;
                TutorialCarousel.IndicatorView = TutorialIndicator;
                UpdateNextButton(_slides.Count - 1);
            });
#else
            TutorialCarousel.ItemsSource = _slides;
            TutorialCarousel.IndicatorView = TutorialIndicator;
            UpdateNextButton(_slides.Count - 1);
#endif
        }

        private void OnCarouselPositionChanged(object? sender, PositionChangedEventArgs e)
        {
            UpdateNextButton(_slides.Count - 1);
        }

        private void UpdateNextButton(int lastPosition)
        {
            bool isLastSlide = TutorialCarousel.Position >= lastPosition;
            NextImageButton.Source = isLastSlide ? "play.png" : "next.png";
        }

        private async void OnNextButtonClicked(object sender, EventArgs e)
        {
            int currentPosition = TutorialCarousel.Position;
            int lastPosition = _slides.Count - 1;
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");

            if (currentPosition < lastPosition)
            {
                TutorialCarousel.Position = currentPosition + 1;
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
