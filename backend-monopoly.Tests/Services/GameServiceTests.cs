using MonopolyBackend.Interfaces;
using NUnit.Framework;
using Moq;
using MonopolyBackend.Services;
using Microsoft.Extensions.Logging;
using MonopolyBackend.Common;

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
            return mock.Object;
        }

        private IAsset CreateMockAsset(string name, int value)
        {
            var mock = new Mock<IAsset>();
            mock.Setup(a => a.Name).Returns(name);
            mock.Setup(a => a.Value).Returns(value);
            return mock.Object;
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
            Assert.That(result.Error?.Type, Is.EqualTo(ErrorType.Validation)); // type validation
            Assert.That(result.Error?.Message, Is.EqualTo("Amount must be greater than zero."));
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
            var player = _players[0];
            int expectedInitialMoney = 1500;

            // Act
            var result = _gameService.GetPlayerMoney(player);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(expectedInitialMoney));
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
            var asset = CreateMockAsset("Solo", 1000);

            // Act
            var result = _gameService.GetMortgageValue(asset);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(500));
        }

        [Test]
        public void GetMortgageValue_NullAsset_ShouldReturnValidationError()
        {
            // Arrange
            IAsset nullAsset = null!;

            // Act
            var result = _gameService.GetMortgageValue(nullAsset);

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
            var asset = CreateMockAsset("Solo", 1000);

            // Act
            var result = _gameService.GetUnmortgageCost(asset);

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
            var player = _players[0];

            // Act
            var result = _gameService.GetJailTurns(player);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(0)); // Initial jail turns
        }

        [Test]
        public void GetJailTurns_NullPlayer_ShouldReturnValidationError()
        {
            // Arrange
            IPlayer nullPlayer = null!;

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
            var player = _players[0];

            // Act
            var result = _gameService.HasGetOutOfJailCard(player);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.False); // No cards initially
        }

        [Test]
        public void HasGetOutOfJailCard_NullPlayer_ShouldReturnValidationError()
        {
            // Arrange
            IPlayer nullPlayer = null!;

            // Act
            var result = _gameService.HasGetOutOfJailCard(nullPlayer);

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
            var activePlayers = _gameService.GetActivePlayers();

            // Assert
            Assert.That(activePlayers, Is.Not.Null);
            Assert.That(activePlayers.Count, Is.EqualTo(2));
            Assert.That(activePlayers, Does.Contain(_players[0]));
            Assert.That(activePlayers, Does.Contain(_players[1]));
        }

        [Test]
        public void GetActivePlayers_OneBankruptPlayer_ShouldReturnOnlyActivePlayer()
        {
            // Arrange
            var bankruptPlayer = _players[0];
            bankruptPlayer.PlayerState = Enums.PlayerState.Bankrupt;

            // Act
            var activePlayers = _gameService.GetActivePlayers();

            // Assert
            Assert.That(activePlayers, Is.Not.Null);
            Assert.That(activePlayers.Count, Is.EqualTo(1));
            Assert.That(activePlayers, Does.Not.Contain(bankruptPlayer));
            Assert.That(activePlayers, Does.Contain(_players[1]));
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

        #endregion

    }
}