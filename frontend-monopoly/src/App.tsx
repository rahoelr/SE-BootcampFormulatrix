import { useState, useEffect } from 'react';
import { Toaster } from 'react-hot-toast';
import toast from 'react-hot-toast';
import { useGameState } from './hooks/useGameState';
import { gameApi } from './services/api';
import { GameSetup } from './components/Game/GameSetup';
import { GameOver } from './components/Game/GameOver';
import { ForceGameOverModal } from './components/Game/ForceGameOverModal';
import { NewGameModal } from './components/Game/NewGameModal';
import { AudioController } from './components/Audio/AudioController';
import monopolyBg from './assets/monopoly_bg.webp';
import monopolyMainTheme from './assets/monopoly_main_theme.mp3';
import victorySound from './assets/victory.mp3';
// import { PropertyActionModal } from './components/Actions/PropertyActionModal'; // Not needed - now in ActionPanel
import { Board } from './components/Board/Board';
import { PlayerCard } from './components/Player/PlayerCard';
import { ActionPanel } from './components/Actions/ActionPanel';
// import { PropertyActions } from './components/Actions/PropertyActions'; // Hidden - now in ActionPanel
// import { MortgagePanel } from './components/Actions/MortgagePanel'; // Hidden for now
// import { TradeModal } from './components/Actions/TradeModal'; // Hidden for now
import type { AvailableAction, RollDiceResponse } from './types';

type AppState = 'loading' | 'no-game' | 'playing' | 'game-over';

function App() {
  const { gameState, board, loading, refreshState, executeAction } = useGameState();
  const [appState, setAppState] = useState<AppState>('loading');
  const [lastRoll, setLastRoll] = useState<RollDiceResponse | null>(null);
  // const [showTradeModal, setShowTradeModal] = useState(false); // Hidden for now
  const [showForceGameOverModal, setShowForceGameOverModal] = useState(false);
  const [showNewGameModal, setShowNewGameModal] = useState(false);
  // const [showPropertyActionModal, setShowPropertyActionModal] = useState<'build' | 'sell' | null>(null); // Not needed - now in ActionPanel

  // Check for active game on mount
  useEffect(() => {
    const checkGameStatus = async () => {
      try {
        const statusRes = await gameApi.getStatus();
        if (statusRes.data.hasActiveGame) {
          await refreshState();
          setAppState('playing');
        } else {
          setAppState('no-game');
        }
      } catch (err) {
        setAppState('no-game');
      }
    };

    checkGameStatus();
  }, [refreshState]);

  // Check if game is over
  useEffect(() => {
    if (gameState?.isGameOver) {
      setAppState('game-over');
    } else if (gameState?.isGameStarted) {
      setAppState('playing');
    }
  }, [gameState]);

  const handleGameCreated = async () => {
    await refreshState();
    setAppState('playing');
  };

  const handleNewGame = async () => {
    try {
      await gameApi.resetGame();
      setLastRoll(null);
      setAppState('no-game');
    } catch (err) {
      // Error handled by interceptor
    }
  };

  const handleResetGame = () => {
    setShowNewGameModal(true);
  };

  const handleConfirmNewGame = async () => {
    try {
      await gameApi.resetGame();
      setLastRoll(null);
      setAppState('no-game');
      setShowNewGameModal(false);
      toast.success('Game reset! Enter new player names.');
    } catch (err) {
      toast.error('Failed to reset game');
    }
  };

  const handleTestGameOver = () => {
    setShowForceGameOverModal(true);
  };

  const handleForceGameOver = async () => {
    try {
      const response = await gameApi.forceEndGame();
      setShowForceGameOverModal(false);
      
      // Force refresh game state to get the final rankings
      await refreshState();
      
      toast.success(`Game Over! Winner: ${response.data.gameResult.winnerName}`);
    } catch (err: any) {
      // If endpoint doesn't exist or error
      const errorMsg = err.response?.data?.error || 'Failed to end game';
      toast.error(errorMsg);
      setShowForceGameOverModal(false);
    }
  };

  const handleAction = async (action: AvailableAction) => {
    if (!gameState) return;

    const currentPlayer = gameState.currentPlayerName;

    try {
      switch (action) {
        case 'roll-dice': {
          const result = await executeAction(() => gameApi.rollDice(currentPlayer));
          setLastRoll(result);
          toast.success(`Rolled ${result.total}! ${result.isDouble ? 'DOUBLES!' : ''}`);
          break;
        }

        case 'buy-property': {
          await executeAction(() => gameApi.buyProperty(currentPlayer));
          toast.success('Property purchased!');
          break;
        }

        case 'end-turn': {
          await executeAction(() => gameApi.endTurn(currentPlayer));
          setLastRoll(null);
          toast.success('Turn ended');
          break;
        }

        case 'pay-jail-fee': {
          await executeAction(() => gameApi.payJailFee(currentPlayer));
          toast.success('Paid jail fee!');
          break;
        }

        case 'use-jail-card': {
          await executeAction(() => gameApi.useJailCard(currentPlayer));
          toast.success('Used Get Out of Jail card!');
          break;
        }

        case 'try-roll-doubles': {
          const result = await executeAction(() => gameApi.tryRollDoubles(currentPlayer));
          setLastRoll(result);
          if (result.isDouble) {
            toast.success('Rolled doubles! You are free!');
          } else {
            toast.error('Not doubles. Still in jail.');
          }
          break;
        }

        // Trade feature hidden for now
        // case 'trade': {
        //   setShowTradeModal(true);
        //   break;
        // }

        case 'build-house':
        case 'sell-house': {
          // These will be handled by ActionPanel's build/sell section
          break;
        }

        // Mortgage features hidden for now
        // case 'mortgage-property':
        // case 'unmortgage-property': {
        //   // These will be handled by their respective panels
        //   break;
        // }

        default:
          toast.error('Action not yet implemented');
      }
    } catch (err) {
      // Error already handled by interceptor
    }
  };

  const handleBuildHouse = async (propertyName: string) => {
    if (!gameState) return;
    try {
      await executeAction(() => 
        gameApi.buildHouse({ playerName: gameState.currentPlayerName, propertyName })
      );
      toast.success(`Built house on ${propertyName}!`);
    } catch (err) {
      // Error handled by interceptor
    }
  };

  const handleSellHouse = async (propertyName: string) => {
    if (!gameState) return;
    try {
      await executeAction(() => 
        gameApi.sellHouse({ playerName: gameState.currentPlayerName, propertyName })
      );
      toast.success(`Sold house on ${propertyName}!`);
    } catch (err) {
      // Error handled by interceptor
    }
  };

  // Mortgage/Unmortgage handlers - Hidden for now
  /*
  const handleMortgage = async (propertyName: string) => {
    if (!gameState) return;
    try {
      await executeAction(() => 
        gameApi.mortgage({ playerName: gameState.currentPlayerName, propertyName })
      );
      toast.success(`Mortgaged ${propertyName}!`);
    } catch (err) {
      // Error handled by interceptor
    }
  };

  const handleUnmortgage = async (propertyName: string) => {
    if (!gameState) return;
    try {
      await executeAction(() => 
        gameApi.unmortgage({ playerName: gameState.currentPlayerName, propertyName })
      );
      toast.success(`Unmortgaged ${propertyName}!`);
    } catch (err) {
      // Error handled by interceptor
    }
  };
  */

  // Trade feature hidden for now
  /*
  const handleTrade = async (tradeData: any) => {
    if (!gameState) return;
    try {
      await executeAction(() => 
        gameApi.trade({
          playerName: gameState.currentPlayerName,
          ...tradeData,
        })
      );
      toast.success('Trade completed!');
      setShowTradeModal(false);
    } catch (err) {
      // Error handled by interceptor
    }
  };
  */

  return (
    <>
      {/* Toaster - always available */}
      <Toaster position="top-right" />

      {/* Loading state */}
      {appState === 'loading' && (
        <div 
          className="min-h-screen flex items-center justify-center bg-cover bg-center bg-no-repeat"
          style={{ backgroundImage: `url(${monopolyBg})` }}
        >
          <div className="text-2xl font-display font-black text-black uppercase tracking-wide bg-white/90 backdrop-blur-sm border-4 border-black shadow-brutal-lg px-8 py-4">Loading Monopoly...</div>
        </div>
      )}

      {/* No game state */}
      {appState === 'no-game' && (
        <GameSetup onGameCreated={handleGameCreated} />
      )}

      {/* Game over state */}
      {appState === 'game-over' && gameState && (
        <GameOver gameState={gameState} onNewGame={handleNewGame} />
      )}

      {/* Playing state */}
      {appState === 'playing' && gameState && board && (
        <div 
          className="min-h-screen p-2 bg-cover bg-center bg-no-repeat relative flex items-center justify-center"
          style={{ backgroundImage: `url(${monopolyBg})` }}
        >
          {/* Main Layout - Board Left, Sidebar Right - Centered */}
          <div className="flex items-start justify-center gap-6">
            {/* Board - Left Side */}
            <div className="flex-shrink-0">
              <Board board={board} gameState={gameState} lastRoll={lastRoll} />
            </div>
            
            {/* Sidebar - Right Side */}
            <div className="flex flex-col gap-2 w-[480px] flex-shrink-0">
              {/* Control Buttons - Top of Sidebar */}
              <div className="flex gap-2 mb-2">
                <button
                  onClick={handleTestGameOver}
                  className="flex-1 bg-red-500 text-white px-3 py-2 font-display font-bold uppercase tracking-wide text-sm border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
                  title="Test Game Over scenario"
                >
                  💀 End Game
                </button>
                <button
                  onClick={handleResetGame}
                  className="flex-1 bg-black text-white px-4 py-2 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
                  title="Start a new game"
                >
                  🔄 New Game
                </button>
              </div>

              {/* Player Cards */}
              <div className="space-y-2">
                {gameState.players.map((player, index) => (
                  <PlayerCard
                    key={player.name}
                    player={player}
                    playerIndex={index}
                    isCurrentTurn={player.name === gameState.currentPlayerName}
                  />
                ))}
              </div>
              
              {/* Action Panel */}
              <ActionPanel
                gameState={gameState}
                onAction={handleAction}
                onBuildHouse={handleBuildHouse}
                onSellHouse={handleSellHouse}
                loading={loading}
              />
            </div>
          </div>

          {/* Force Game Over Modal */}
          {showForceGameOverModal && gameState && (
            <ForceGameOverModal
              gameState={gameState}
              onConfirm={handleForceGameOver}
              onClose={() => setShowForceGameOverModal(false)}
            />
          )}

          {/* New Game Modal */}
          {showNewGameModal && (
            <NewGameModal
              onConfirm={handleConfirmNewGame}
              onCancel={() => setShowNewGameModal(false)}
            />
          )}
        </div>
      )}

      {/* Single Audio Controller - switches track based on game state */}
      <AudioController track={appState === 'game-over' ? victorySound : monopolyMainTheme} />
    </>
  );
}

export default App
