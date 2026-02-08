interface NewGameModalProps {
  onConfirm: () => void;
  onCancel: () => void;
}

export function NewGameModal({ onConfirm, onCancel }: NewGameModalProps) {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white border-4 border-black shadow-brutal-lg p-8 max-w-md w-full">
        <div className="text-center mb-6">
          <div className="text-6xl mb-4">🔄</div>
          <h2 className="text-3xl font-display font-black text-black uppercase tracking-tight mb-3">
            Start New Game?
          </h2>
          <p className="font-body text-black text-lg">
            All current progress will be lost.
          </p>
          <p className="font-body text-black mt-2">
            Are you sure you want to continue?
          </p>
        </div>

        <div className="flex gap-3">
          <button
            onClick={onCancel}
            className="flex-1 bg-white text-black px-6 py-4 font-display font-bold uppercase tracking-wide text-lg border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className="flex-1 bg-brutal-yellow text-black px-6 py-4 font-display font-black uppercase tracking-wide text-lg border-4 border-black shadow-brutal hover:shadow-brutal-sm hover:translate-x-[2px] hover:translate-y-[2px] active:shadow-none active:translate-x-[4px] active:translate-y-[4px] transition-all duration-100"
          >
            New Game
          </button>
        </div>

        <p className="text-xs font-body text-gray-600 mt-4 text-center">
          ⚠️ This action cannot be undone
        </p>
      </div>
    </div>
  );
}
