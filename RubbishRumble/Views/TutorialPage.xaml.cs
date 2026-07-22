using System;
using Microsoft.Maui.Controls;
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

        private List<TutorialSlide> _slides;
        public TutorialPage()
        {
            InitializeComponent();

            _slides = new List<TutorialSlide>
                {
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
                        DescPrefix = "Worth ",
                        CoinAmount = "5",
                        DescSuffix = " coins\r\nFound most often",
                        ImageUrl = "tutorial_common.png"
                    },
                    new TutorialSlide
                    {
                        Title = "Uncommon Trash = Better Points",
                        DescPrefix = "Worth ",
                        CoinAmount = "10",
                        DescSuffix = " coins\r\nAppears sometimes",
                        ImageUrl = "tutorial_uncommon.png"
                    },
                    new TutorialSlide
                    {
                        Title = "Rare Trash = Big Points",
                        DescPrefix = "Worth ",
                        CoinAmount = "20",
                        DescSuffix = " coins\r\nHard to find",
                        ImageUrl = "tutorial_rare.png"
                    },
                    new TutorialSlide
                    {
                        Title = "Epic Trash = Jackpot!",
                        DescPrefix = "Worth ",
                        CoinAmount = "50",
                        DescSuffix = " coins\r\nExtremely rare",
                        ImageUrl = "tutorial_epic.png"
                    },
                    new TutorialSlide
                    {
                        Title = "You're ready!",
                        Description = "Let's keep the world clean together",
                        ImageUrl = "tutorial_slide6.png"
                    }
                };
            TutorialCarousel.ItemsSource = _slides;
            TutorialCarousel.IndicatorView = TutorialIndicator;
        }

        private async void OnNextButtonClicked(object sender, EventArgs e)
        {
            int currentPosition = TutorialCarousel.Position;
            int lastPosition = _slides.Count - 1;
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");

            if (currentPosition < lastPosition)
            {
                int nextPosition = currentPosition + 1;
                TutorialCarousel.CurrentItem = _slides[nextPosition];
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}