using System.Windows.Input;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class StoreViewModel : BaseViewModel
    {
        private readonly StoreService _storeService;
        private readonly InventoryService _inventoryService;
        private readonly PowerUpService _powerUpService;

        private int _coins;
        private int _ownedFreeze;
        private int _ownedSlow;
        private int _ownedAutoSort;
        private int _ownedSpeed;
        private int _freezePrice;
        private int _slowPrice;
        private int _autoSortPrice;
        private int _speedPrice;

        public int Coins
        {
            get => _coins;
            private set => SetProperty(ref _coins, value);
        }

        public int OwnedFreeze
        {
            get => _ownedFreeze;
            private set
            {
                if (SetProperty(ref _ownedFreeze, value))
                    OnPropertyChanged(nameof(FreezeDisplayText));
            }
        }

        public int OwnedSlow
        {
            get => _ownedSlow;
            private set
            {
                if (SetProperty(ref _ownedSlow, value))
                    OnPropertyChanged(nameof(SlowDisplayText));
            }
        }

        public int OwnedAutoSort
        {
            get => _ownedAutoSort;
            private set
            {
                if (SetProperty(ref _ownedAutoSort, value))
                    OnPropertyChanged(nameof(AutoSortDisplayText));
            }
        }

        public int OwnedSpeed
        {
            get => _ownedSpeed;
            private set
            {
                if (SetProperty(ref _ownedSpeed, value))
                    OnPropertyChanged(nameof(SpeedDisplayText));
            }
        }

        public int FreezePrice
        {
            get => _freezePrice;
            private set
            {
                if (SetProperty(ref _freezePrice, value))
                    OnPropertyChanged(nameof(FreezePriceText));
            }
        }

        public int SlowPrice
        {
            get => _slowPrice;
            private set
            {
                if (SetProperty(ref _slowPrice, value))
                    OnPropertyChanged(nameof(SlowPriceText));
            }
        }

        public int AutoSortPrice
        {
            get => _autoSortPrice;
            private set
            {
                if (SetProperty(ref _autoSortPrice, value))
                    OnPropertyChanged(nameof(AutoSortPriceText));
            }
        }

        public int SpeedPrice
        {
            get => _speedPrice;
            private set
            {
                if (SetProperty(ref _speedPrice, value))
                    OnPropertyChanged(nameof(SpeedPriceText));
            }
        }

        public string FreezeDisplayText => $"Freeze\nOwned: {OwnedFreeze}";
        public string SlowDisplayText => $"Slow\nOwned: {OwnedSlow}";
        public string AutoSortDisplayText => $"Auto Sort\nOwned: {OwnedAutoSort}";
        public string SpeedDisplayText => $"Speed\nOwned: {OwnedSpeed}";
        public string FreezePriceText => $"Coins: {FreezePrice}";
        public string SlowPriceText => $"Coins: {SlowPrice}";
        public string AutoSortPriceText => $"Coins: {AutoSortPrice}";
        public string SpeedPriceText => $"Coins: {SpeedPrice}";

        public ICommand BuyFreezeCommand { get; }
        public ICommand BuySlowCommand { get; }
        public ICommand BuyAutoSortCommand { get; }
        public ICommand BuySpeedCommand { get; }

        public StoreViewModel()
        {
            var databaseService = new DatabaseService();
            _powerUpService = new PowerUpService();
            _inventoryService = new InventoryService(databaseService, _powerUpService);
            _storeService = new StoreService(_powerUpService, _inventoryService, databaseService);

            BuyFreezeCommand = new Command(async () => await BuyPowerUpAsync("Freeze"));
            BuySlowCommand = new Command(async () => await BuyPowerUpAsync("Slow"));
            BuyAutoSortCommand = new Command(async () => await BuyPowerUpAsync("Auto Sort"));
            BuySpeedCommand = new Command(async () => await BuyPowerUpAsync("Speed"));
        }

        public async Task InitializeAsync()
        {
            await _powerUpService.InitializeAsync();
            LoadPrices();
            await RefreshAsync();
        }

        private void LoadPrices()
        {
            foreach (var powerUp in _powerUpService.PowerUps)
            {
                switch (powerUp.Name)
                {
                    case "Freeze":
                        FreezePrice = powerUp.Price;
                        break;
                    case "Slow":
                        SlowPrice = powerUp.Price;
                        break;
                    case "Auto Sort":
                        AutoSortPrice = powerUp.Price;
                        break;
                    case "Speed":
                        SpeedPrice = powerUp.Price;
                        break;
                }
            }
        }

        private async Task RefreshAsync()
        {
            Coins = await _storeService.GetPlayerCoinsAsync();
            OwnedFreeze = await _inventoryService.GetPowerUpCountAsync("Freeze");
            OwnedSlow = await _inventoryService.GetPowerUpCountAsync("Slow");
            OwnedAutoSort = await _inventoryService.GetPowerUpCountAsync("Auto Sort");
            OwnedSpeed = await _inventoryService.GetPowerUpCountAsync("Speed");
        }

        private async Task BuyPowerUpAsync(string powerUpName)
        {
            bool success = await _storeService.BuyPowerUpAsync(powerUpName);
            if (!success)
                return;

            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            await RefreshAsync();
        }
    }
}
