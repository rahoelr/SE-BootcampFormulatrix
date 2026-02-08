/**
 * Monopoly board: 11x11 CSS Grid
 * 
 * Positions 0-39 mapped to grid coordinates
 */

export interface GridPosition {
  row: number; // 0-10
  col: number; // 0-10
}

export function getTileGridPosition(position: number): GridPosition {
  // Bottom row: positions 0-10 (row 10, col 10→0)
  if (position >= 0 && position <= 10) {
    return { row: 10, col: 10 - position };
  }
  
  // Left column: positions 11-19 (col 0, row 9→1)
  if (position >= 11 && position <= 19) {
    return { row: 20 - position, col: 0 };
  }
  
  // Top row: positions 20-30 (row 0, col 0→10)
  if (position >= 20 && position <= 30) {
    return { row: 0, col: position - 20 };
  }
  
  // Right column: positions 31-39 (col 10, row 1→9)
  if (position >= 31 && position <= 39) {
    return { row: position - 30, col: 10 };
  }
  
  // Fallback
  return { row: 0, col: 0 };
}

export function isCornerTile(position: number): boolean {
  return position === 0 || position === 10 || position === 20 || position === 30;
}
