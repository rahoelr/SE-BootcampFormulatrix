import { useState, useEffect } from 'react';
import { Toaster } from 'react-hot-toast';
import toast from 'react-hot-toast';
import { useGameState } from './hooks/useGameState';
import { gameApi } from './services/api';
import { GameSetup } from './components/Game/GameSetup';
import { GameOver } from './components/Game/GameOver';
import { Board } from './components/Board/Board';
import { PlayerCard } from './components/Player/PlayerCard';
import { ActionPanel } from './components/Actions/ActionPanel';
import { PropertyActions } from './components/Actions/PropertyActions';
import { MortgagePanel } from './components/Actions/MortgagePanel';
import { TradeModal } from './components/Actions/TradeModal';
import type { AvailableAction, RollDiceResponse } from './types';

type AppState = 'loading' | 'no-game' | 'playing' | 'game-over';

function App() {
  const { gameState, board, loading, refreshState, executeAction } = useGameState();
  const [appState, setAppState] = useState<AppState>('loading');
  const [lastRoll, setLastRoll] = useState<RollDiceResponse | null>(null);
  const [showTradeModal, setShowTradeModal] = useState(false);

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

  const handleResetGame = async () => {
    if (!window.confirm('Are you sure you want to start a new game? Current progress will be lost.')) {
      return;
    }
    
    try {
      await gameApi.resetGame();
      setLastRoll(null);
      setAppState('no-game');
      toast.success('Game reset! Enter new player names.');
    } catch (err) {
      toast.error('Failed to reset game');
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

        case 'trade': {
          setShowTradeModal(true);
          break;
        }

        case 'build-house':
        case 'sell-house':
        case 'mortgage-property':
        case 'unmortgage-property': {
          // These will be handled by their respective panels
          break;
        }

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

  // Loading state
  if (appState === 'loading') {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-2xl font-display font-black text-black uppercase tracking-wide">Loading Monopoly...</div>
      </div>
    );
  }

  // No game state
  if (appState === 'no-game') {
    return (
      <>
        <Toaster position="top-right" />
        <GameSetup onGameCreated={handleGameCreated} />
      </>
    );
  }

  // Game over state
  if (appState === 'game-over' && gameState) {
    return (
      <>
        <Toaster position="top-right" />
        <GameOver gameState={gameState} onNewGame={handleNewGame} />
      </>
    );
  }

  // Playing state
  if (appState === 'playing' && gameState && board) {
    const currentPlayer = gameState.players.find(p => p.name === gameState.currentPlayerName);

    return (
      <div className="min-h-screen bg-white p-4">
        <Toaster position="top-right" />

        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <div className="flex justify-between items-center mb-4">
            <div className="flex-1">
              <h1 className="text-5xl font-display font-black text-black uppercase tracking-tight">🎲 Monopoly</h1>
              <p className="font-body text-black font-semibold">Turn {gameState.currentTurn + 1}</p>
            </div>
            <button
              onClick={handleResetGame}
              className="bg-black text-white px-4 py-2 font-display font-bold uppercase tracking-wide border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
              title="Start a new game"
            >
              🔄 New Game
            </button>
          </div>

          {/* Main Layout */}
          <div className="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-4">
            {/* Left: Board & Actions */}
            <div className="space-y-4">
              <Board board={board} gameState={gameState} lastRoll={lastRoll} />
              
              <ActionPanel
                gameState={gameState}
                onAction={handleAction}
                loading={loading}
              />

              {/* Advanced Actions */}
              {currentPlayer && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {gameState.availableActions.includes('build-house' as AvailableAction) && (
                    <PropertyActions
                      properties={currentPlayer.properties}
                      onBuild={handleBuildHouse}
                      onSell={handleSellHouse}
                      loading={loading}
                    />
                  )}

                  {gameState.availableActions.includes('mortgage-property' as AvailableAction) && (
                    <MortgagePanel
                      properties={currentPlayer.properties}
                      onMortgage={handleMortgage}
                      onUnmortgage={handleUnmortgage}
                      loading={loading}
                    />
                  )}
                </div>
              )}
            </div>

            {/* Right: Player Cards */}
            <div className="space-y-4">
              {gameState.players.map((player, index) => (
                <PlayerCard
                  key={player.name}
                  player={player}
                  playerIndex={index}
                  isCurrentTurn={player.name === gameState.currentPlayerName}
                />
              ))}
            </div>
          </div>
        </div>

        {/* Trade Modal */}
        {showTradeModal && currentPlayer && (
          <TradeModal
            currentPlayer={currentPlayer}
            allPlayers={gameState.players}
            onTrade={handleTrade}
            onClose={() => setShowTradeModal(false)}
            loading={loading}
          />
        )}
      </div>
    );
  }

  return null;
}

export default App
