using MonopolyBackend.Interfaces;
using NUnit.Framework;
using Moq;
using MonopolyBackend.Services;
using Microsoft.Extensions.Logging;
using MonopolyBackend.Common;
using Microsoft.VisualBasic;

namespace MonopolyBackend.Tests.Services
{
    [TestFixture]
    public class GameServiceTests
    {
        private Mock<IBoard> _mockBoard;
        private Mock<ILogger<GameService>> _mockLogger;
        private List<IPlayer> _players;
        private List<IDice> _dices;
        private Mock<IDecks> _mockCommunityChest;
        private Mock<IDecks> _mockChance;
        private Dictionary<ITile, IAsset?> _tileAssets;
        private GameService _gameService;

        [SetUp]
        public void SetUp()
        {
            _mockBoard = new Mock<IBoard>();
            _mockLogger = new Mock<ILogger<GameService>>();
            _dices = new List<IDice>();
            _mockCommunityChest = new Mock<IDecks>();
            _mockChance = new Mock<IDecks>();
            _tileAssets = new Dictionary<ITile, IAsset?>();
            _players = new List<IPlayer>
            {
                CreateMockPlayer("Rahul"),
                CreateMockPlayer("Bagus")
            };

            _gameService = new GameService(
                _mockBoard.Object,
                _players,
                _dices,
                _mockCommunityChest.Object,
                _mockChance.Object,
                _tileAssets,
                _mockLogger.Object
            );
        }

        private IPlayer CreateMockPlayer(string name)
        {
            var mock = new Mock<IPlayer>();
            mock.Setup(p => p.Name).Returns(name);
            mock.SetupProperty(p => p.PlayerState, Enums.PlayerState.Normal);
            mock.Setup(p => p.Assets).Returns(new List<IAsset>());
            return mock.Object;
        }

        private IAsset CreateMockAsset(string name, int value)
        {
            var mock = new Mock<IAsset>();
            mock.Setup(a => a.Name).Returns(name);
            mock.Setup(a => a.Value).Returns(value);
            mock.Setup(a => a.AssetCondition).Returns(Enums.AssetCondition.Normal);
            mock.Setup(a => a.AmountHouse).Returns(0);
            return mock.Object;
        }

        private void SetupBoardPath() // helper method
        {
            var path = new List<ITile>();
            for (int i = 0; i < 40; i++)
            {
                var tile = new Mock<ITile>();
                tile.Setup(t => t.Name).Returns(i == 10 ? "Jail" : $"Tile {i}");
                path.Add(tile.Object);
            }
            _mockBoard.Setup(b => b.Path).Returns(path);
        }

        #region AddMoney Tests

        [Test]
        public void AddMoney_ValidAmount_ShouldIncreasePlayerMoney()
        {
            // Arrange
            var player = _players[0];
            int initialMoney = _gameService.GetPlayerMoney(player).Data; // 1500 dulu
            int addedAmount = 500;

            // Act
            var result = _gameService.AddMoney(player, addedAmount); // 2000

            // Assert
            Assert.That(result.IsSuccess, Is.True); // sukses nih
            Assert.That(_gameService.GetPlayerMoney(player).Data,
                        Is.EqualTo(initialMoney + addedAmount)); // 2000 == 1500 + 500
        }

        [Test]
        public void AddMoney_NegativeAmount_ShouldReturnValidationError()
        {
            // Arrange
            var player = _players[0]; // 1500
            int negativeAmount = -100;

            // Act
            var result = _gameService.AddMoney(player, negativeAmount);  // error nih pasti

            // Assert
            Assert.That(result.IsSuccess, Is.False); // is success false
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation)); //
            Assert.That(result.Error?.Message, Is.EqualTo("Amount must be greater than zero."));
        }

        [Test]
        public void AddMoney_PlayerNotFound_ShouldReturnValidationError()
        {
            // Arrange
            var nonExistentPlayer = CreateMockPlayer("Kevin"); // pemain lai
            int amount = 100;

            // Act
            var result = _gameService.AddMoney(nonExistentPlayer, amount); // error

            // Assert
            Assert.That(result.IsSuccess, Is.False); // gagal
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.NotFound));
            Assert.That(result.Error?.Message, Is.EqualTo("Player not found in game."));
        }

        #endregion

        #region SubtractMoney Tests

        [Test]
        public void SubtractMoney_ValidAmount_ShouldDecreasePlayerMoney()
        {
            // Arrange
            var player = _players[0];
            int initialMoney = _gameService.GetPlayerMoney(player).Data; // 1500
            int subtractAmount = 500;

            // Act
            var result = _gameService.SubtractMoney(player, subtractAmount); // true // 1000

            // Assert
            Assert.That(result.IsSuccess, Is.True); // sukses
            Assert.That(_gameService.GetPlayerMoney(player).Data,
                        Is.EqualTo(initialMoney - subtractAmount)); // 1000 == 1500 - 500
        }

        [Test]
        public void SubtractMoney_InsufficientFunds_ShouldReturnValidationError()
        {
            // Arrange
            var player = _players[0];
            int initialMoney = _gameService.GetPlayerMoney(player).Data; // 1500
            int excessiveAmount = initialMoney + 1000;  // 2500

            // Act
            var result = _gameService.SubtractMoney(player, excessiveAmount); // error

            // Assert
            Assert.That(result.IsSuccess, Is.False); // error
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Insufficient funds."));
        }

        #endregion

        #region GetPlayerMoney Tests

        [Test]
        public void GetPlayerMoney_ValidPlayer_ShouldReturnCorrectAmount()
        {
            // Arrange
            var player = _players[0]; // ambil player pertama (1500)
            int expectedInitialMoney = 1500;

            // Act
            var result = _gameService.GetPlayerMoney(player); // ambil uangnya

            // Assert
            Assert.That(result.IsSuccess, Is.True); // sukses
            Assert.That(result.Data, Is.EqualTo(expectedInitialMoney)); // 1500 == 1500
        }

        [Test]
        public void GetPlayerMoney_NullPlayer_ShouldReturnValidationError()
        {
            // Arrange
            IPlayer nullPlayer = null!;

            // Act
            var result = _gameService.GetPlayerMoney(nullPlayer);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Player cannot be null."));
        }

        #endregion

        #region GetMortgageValue Tests

        [Test]
        public void GetMortgageValue_ValidAsset_ShouldReturnHalfValue()
        {
            // Arrange
            var asset = CreateMockAsset("Solo", 1000); // nilai aset 1000

            // Act
            var result = _gameService.GetMortgageValue(asset); // harusnya 500, dipotong 50%

            // Assert
            Assert.That(result.IsSuccess, Is.True); // sukses
            Assert.That(result.Data, Is.EqualTo(500)); // 500 == 1000 / 2
        }

        [Test]
        public void GetMortgageValue_NullAsset_ShouldReturnValidationError()
        {
            // Arrange
            IAsset nullAsset = null!;

            // Act
            var result = _gameService.GetMortgageValue(nullAsset); // error

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Asset cannot be null."));
        }

        #endregion

        #region GetUnmortgageCost Tests

        [Test]
        public void GetUnmortgageCost_ValidAsset_ShouldReturnMortgageValuePlusTenPercent()
        {
            // Arrange
            var asset = CreateMockAsset("Solo", 1000); // nilai aset 1000

            // Act
            var result = _gameService.GetUnmortgageCost(asset);   // (1000/2) * 1.1 = 550

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(550)); // (1000/2) * 1.1 = 550
        }

        [Test]
        public void GetUnmortgageCost_NullAsset_ShouldReturnValidationError()
        {
            // Arrange
            IAsset nullAsset = null!;

            // Act
            var result = _gameService.GetUnmortgageCost(nullAsset);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Asset cannot be null."));
        }

        #endregion

        #region GetJailTurns Tests

        [Test]
        public void GetJailTurns_ValidPlayer_ShouldReturnInitialZero()
        {
            // Arrange
            var player = _players[0]; // player rahul

            // Act
            var result = _gameService.GetJailTurns(player); // awalnya pasti 0

            // Assert
            Assert.That(result.IsSuccess, Is.True); // sukses
            Assert.That(result.Data, Is.EqualTo(0)); // awalnya 0
        }

        [Test]
        public void GetJailTurns_NullPlayer_ShouldReturnValidationError()
        {
            // Arrange
            IPlayer nullPlayer = null!; // null

            // Act
            var result = _gameService.GetJailTurns(nullPlayer);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Player cannot be null."));
        }

        #endregion

        #region HasGetOutOfJailCard Tests

        [Test]
        public void HasGetOutOfJailCard_PlayerWithoutCard_ShouldReturnFalse()
        {
            // Arrange
            var player = _players[0]; // player rahul

            // Act
            var result = _gameService.HasGetOutOfJailCard(player); // awalnya pasti false

            // Assert
            Assert.That(result.IsSuccess, Is.True); // sukses
            Assert.That(result.Data, Is.False); // tidak ada kartu keluar dari penjara
        }

        [Test]
        public void HasGetOutOfJailCard_NullPlayer_ShouldReturnValidationError()
        {
            // Arrange
            IPlayer nullPlayer = null!;

            // Act
            var result = _gameService.HasGetOutOfJailCard(nullPlayer); // false

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Player cannot be null."));
        }

        #endregion

        #region GetActivePlayers Tests

        [Test]
        public void GetActivePlayers_AllPlayersActive_ShouldReturnAllPlayers()
        {
            // Act
            var activePlayers = _gameService.GetActivePlayers(); // ambil semua pemain aktif

            // Assert
            Assert.That(activePlayers, Is.Not.Null); // hasil ga boleh null
            Assert.That(activePlayers.Count, Is.EqualTo(2)); // ada 2 pemain aktif
            Assert.That(activePlayers, Does.Contain(_players[0])); // ada pemain pertama rahul
            Assert.That(activePlayers, Does.Contain(_players[1])); // ada pemain kedua bagus
        }

        [Test]
        public void GetActivePlayers_OneBankruptPlayer_ShouldReturnOnlyActivePlayer()
        {
            // Arrange
            var bankruptPlayer = _players[0];
            bankruptPlayer.PlayerState = Enums.PlayerState.Bankrupt;

            // Act
            var activePlayers = _gameService.GetActivePlayers(); // ambil semua pemain aktif

            // Assert
            Assert.That(activePlayers, Is.Not.Null); // Daftar hasil tidak boleh null
            Assert.That(activePlayers.Count, Is.EqualTo(1)); // Hanya ada 1 pemain aktif
            Assert.That(activePlayers, Does.Not.Contain(bankruptPlayer)); // Pemain bangkrut tidak ada di daftar
            Assert.That(activePlayers, Does.Contain(_players[1])); // Pemain kedua tetap ada di daftar

        }

        #endregion


        #region NextTurn_Tests
        [Test]
        public void NextTurn_ShouldAdvance_ToNextPlayer()
        {
            // act
            _gameService.NextTurn();

            // assert
            // CurrentTurn awalnya 0 (pemain pertama), setelah NextTurn() bertambah 1
            Assert.That(_gameService.CurrentTurn, Is.EqualTo(1));

            // Index 1 di daftar _players menunjuk pemain kedua (Bagus)
            Assert.That(_gameService.CurrentPlayer, Is.EqualTo(_players[1]));
        }

        [Test]
        public void NextTurn_PlayerSkipsBankruptPlayer_ShouldAdvanceToNextActivePlayer()
        {
            // arrange
            // Set pemain pertama menjadi bangkrut
            var bankruptPlayer = _players[0];
            bankruptPlayer.PlayerState = Enums.PlayerState.Bankrupt;

            // act
            _gameService.NextTurn();

            // assert
            // CurrentTurn harus melompat ke 1
            Assert.That(_gameService.CurrentTurn, Is.EqualTo(1));

            // CurrentPlayer harus menunjuk pemain kedua (Bagus)
            Assert.That(_gameService.CurrentPlayer, Is.EqualTo(_players[1]));
        }

        #endregion

        #region CalculateTotaAssets_Tests

        [Test]
        public void CalculatePlayerTotalAssetsValue_UnmortgagedAssets_ShouldReturnCorrectTotal()
        {
            // Arrange
            var player = _players[0]; // ambil pemain pertama

            var asset1 = CreateMockAsset("Yogyakarta", 350); // buat aset pertama
            var asset2 = CreateMockAsset("Surakarta", 400); // buat aset kedua

            player.Assets.Add(asset1);
            player.Assets.Add(asset2);

            // Act
            var result = _gameService.CalculatePlayerTotalAssetsValue(player);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(750)); // 350 + 400
        }

        [Test]
        public void CalculatePlayerTotalAssetsValue_NullAsset_ShouldReturnValidationError()
        {
            // Arrange
            IPlayer nullPlayer = null!;

            // Act
            var result = _gameService.CalculatePlayerTotalAssetsValue(nullPlayer);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Player cannot be null."));
        }
        #endregion

        #region CalculateTotalPlayerWealth_Tests
        [Test]
        public void CalculatePlayerTotalWealth_FailGetMoney_ShouldReturnInvalid()
        {
            // arrage
            // IPlayer player =_players[0];
            IPlayer player = null!;

            // act
            var result = _gameService.CalculatePlayerTotalWealth(player);

            // assert
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Failed to get player money."));

        }

        [Test]
        public void CalculatePlayerTotalWealth_GetTotalWealth_ShouldReturnTotalMoneyAssets()
        {
            // arrage
            IPlayer player = _players[0]; // 1500
            var asset1 = CreateMockAsset("Semarang", 200);
            var asset2 = CreateMockAsset("Purwokerto", 300);

            player.Assets.Add(asset1); // add asets ke player
            player.Assets.Add(asset2);

            var resultMoney = _gameService.GetPlayerMoney(player);
            var resultPlayerAssets = _gameService.CalculatePlayerTotalAssetsValue(player);
            var resultTotal = resultMoney.Data + resultPlayerAssets.Data;
            // act
            var resultWealth = _gameService.CalculatePlayerTotalWealth(player);

            // assert
            Assert.That(resultPlayerAssets.IsSuccess, Is.True);
            Assert.That(resultWealth.Data,
                        Is.EqualTo(resultTotal));

        }

        #endregion

        #region SendToJail_Tests

        [Test]
        public void SendToJail_GoToJail_ShouldPlacePlayerInJail()
        {
            SetupBoardPath();
            var player = _players[0];

            _gameService.SendToJail();

            Assert.That(player.PlayerState, Is.EqualTo(Enums.PlayerState.InJail));
        }

        #endregion

        #region PayJailFee_Tests

        [Test]
        public void PayJailFee_PlayerNotInJail_ShouldReturnValidationError()
        {
            var player = _players[0];

            var result = _gameService.PayJailFee();

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Player is not in jail."));
        }

        [Test]
        public void PayJailFee_InvalidPay_ShouldReturnFalse()
        {
            var player = _players[0]; // permain pertama
            var playerState = Enums.PlayerState.InJail;
            player.PlayerState = playerState;
            var playerMoney = _gameService.SubtractMoney(player, 1499);

            var result = _gameService.PayJailFee();
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation));
            Assert.That(result.Error?.Message, Is.EqualTo("Insufficient funds to pay jail fee."));
        }

        #endregion

    }
}