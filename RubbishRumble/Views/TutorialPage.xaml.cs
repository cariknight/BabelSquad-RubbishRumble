using System;
using Microsoft.Maui.Controls;

namespace RubbishRumble.Views
{
    public partial class TutorialPage : ContentPage
    {
        public class TutorialSlide
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; } 
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
                        ImageUrl = "Tutorial_Images/tutorial_slide1.png"
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
                        Title = "Earn coins per item\r\nRare trash gives bonus",
                        Description = "Unlock Trash Index\r\nby finding items",
                        ImageUrl = "tutorial_slide5.gif"
                    },
                    new TutorialSlide
                    {
                        Title = "You’re ready!",
                        Description = "Let’s keep the world clean together",
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

            if (currentPosition < lastPosition)
            {
                int nextPosition = currentPosition + 1;
                TutorialCarousel.CurrentItem = _slides[nextPosition];
            }
            else
            {
                // Going to main menu page
                await Shell.Current.GoToAsync("..");
            }
        }

        private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
        {
            // Intentionally left empty
        }
    }
}