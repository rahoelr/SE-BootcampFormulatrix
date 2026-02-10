import { useState, useEffect, useRef } from 'react';
import monopolyTheme from '../../assets/monopoly_main_theme.mp3';

interface AudioControllerProps {
  track?: string;
}

export function AudioController({ track = monopolyTheme }: AudioControllerProps) {
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const [isPlaying, setIsPlaying] = useState(true);
  const [volume, setVolume] = useState(0.5);
  const [showVolumePanel, setShowVolumePanel] = useState(false);

  useEffect(() => {
    // Create audio element
    audioRef.current = new Audio(track);
    audioRef.current.loop = true;
    audioRef.current.volume = volume;

    // Auto-play on mount with promise handling
    const playPromise = audioRef.current.play();
    
    if (playPromise !== undefined) {
      playPromise
        .then(() => {
          // Autoplay started successfully
          console.log('Audio autoplay started successfully');
          setIsPlaying(true);
        })
        .catch(() => {
          // Autoplay was prevented - try to enable on first user interaction
          console.log('Auto-play blocked by browser, will try on user interaction');
          setIsPlaying(false);
          
          // Setup one-time click listener to start audio
          const startAudioOnInteraction = () => {
            if (audioRef.current) {
              audioRef.current.play()
                .then(() => {
                  setIsPlaying(true);
                  console.log('Audio started after user interaction');
                })
                .catch(console.error);
            }
            // Remove listeners after first interaction
            document.removeEventListener('click', startAudioOnInteraction);
            document.removeEventListener('keydown', startAudioOnInteraction);
          };
          
          document.addEventListener('click', startAudioOnInteraction, { once: true });
          document.addEventListener('keydown', startAudioOnInteraction, { once: true });
        });
    }

    return () => {
      if (audioRef.current) {
        audioRef.current.pause();
        audioRef.current = null;
      }
    };
  }, []);

  // Switch track when track prop changes
  useEffect(() => {
    if (audioRef.current && audioRef.current.src !== track) {
      const wasPlaying = !audioRef.current.paused;
      const currentVolume = audioRef.current.volume;
      
      // Update track
      audioRef.current.src = track;
      audioRef.current.volume = currentVolume;
      
      // Resume playing if it was playing before
      if (wasPlaying) {
        audioRef.current.play().catch(console.error);
      }
    }
  }, [track]);

  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.volume = volume;
    }
  }, [volume]);

  const togglePlay = () => {
    if (audioRef.current) {
      if (isPlaying) {
        audioRef.current.pause();
      } else {
        audioRef.current.play().catch(console.error);
      }
      setIsPlaying(!isPlaying);
    }
  };

  const handleVolumeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newVolume = parseFloat(e.target.value);
    setVolume(newVolume);
  };

  const getVolumeIcon = () => {
    if (volume === 0 || !isPlaying) return '🔇';
    if (volume < 0.3) return '🔈';
    if (volume < 0.7) return '🔉';
    return '🔊';
  };

  return (
    <div className="fixed bottom-4 right-4 z-50">
      {/* Volume Panel */}
      {showVolumePanel && (
        <div className="absolute bottom-16 right-0 bg-white border-4 border-black shadow-brutal p-4 mb-2 w-64">
          <div className="flex items-center justify-between mb-3">
            <span className="font-display font-bold text-black uppercase text-sm">
              🎵 Music Settings
            </span>
            <button
              onClick={() => setShowVolumePanel(false)}
              className="text-black font-bold hover:text-gray-600"
            >
              ✕
            </button>
          </div>

          {/* Play/Pause Button */}
          <button
            onClick={togglePlay}
            className={`w-full py-2 px-4 mb-3 font-display font-bold uppercase text-sm border-3 border-black shadow-brutal transition-all duration-100 ${
              isPlaying 
                ? 'bg-red-400 hover:bg-red-500' 
                : 'bg-green-400 hover:bg-green-500'
            } hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px]`}
          >
            {isPlaying ? '⏸️ Pause Music' : '▶️ Play Music'}
          </button>

          {/* Volume Slider */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <span className="font-body text-sm font-semibold text-black">Volume</span>
              <span className="font-body text-sm font-bold text-black">{Math.round(volume * 100)}%</span>
            </div>
            <div className="relative">
              <input
                type="range"
                min="0"
                max="1"
                step="0.01"
                value={volume}
                onChange={handleVolumeChange}
                className="w-full h-3 appearance-none cursor-pointer bg-gray-200 border-2 border-black"
                style={{
                  background: `linear-gradient(to right, #22c55e 0%, #22c55e ${volume * 100}%, #e5e7eb ${volume * 100}%, #e5e7eb 100%)`
                }}
              />
            </div>
            <div className="flex justify-between text-xs font-body text-gray-600">
              <span>🔇</span>
              <span>🔊</span>
            </div>
          </div>

          {/* Quick Volume Buttons */}
          <div className="flex gap-2 mt-3">
            {[0, 0.25, 0.5, 0.75, 1].map((v) => (
              <button
                key={v}
                onClick={() => setVolume(v)}
                className={`flex-1 py-1 text-xs font-display font-bold border-2 border-black transition-all ${
                  Math.abs(volume - v) < 0.05 
                    ? 'bg-brutal-yellow' 
                    : 'bg-white hover:bg-gray-100'
                }`}
              >
                {v * 100}%
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Floating Button */}
      <button
        onClick={() => setShowVolumePanel(!showVolumePanel)}
        className={`w-14 h-14 rounded-full border-4 border-black shadow-brutal flex items-center justify-center text-2xl transition-all duration-200 hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] ${
          showVolumePanel ? 'bg-brutal-yellow' : 'bg-white'
        }`}
        title="Music Settings"
      >
        {getVolumeIcon()}
      </button>

      {/* Playing indicator */}
      {isPlaying && (
        <div className="absolute -top-1 -right-1 w-4 h-4 bg-green-500 border-2 border-black rounded-full animate-pulse" />
      )}
    </div>
  );
}
