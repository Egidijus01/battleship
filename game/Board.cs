namespace ShipBattle
{
    public enum CellState
    {
        Unknown,
        Empty,
        Ship,
        Hit,
        Miss,
        RadarShip
    }

    public enum ShotResult
    {
        Miss,
        Hit,
        Sunk
    }

    public class Board
    {
        private readonly int _size;
        private readonly CellState[,] _cells;
        private readonly Dictionary<Position, Ship> _shipPositions;

        public Board(int size, bool isOpponent = false)
        {
            _size = size;
            _cells = new CellState[size, size];
            _shipPositions = new Dictionary<Position, Ship>();

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    _cells[row, col] = isOpponent ? CellState.Unknown : CellState.Empty;
                }
            }
        }

        public bool IsWithinBounds(Position position) =>
            position.Row >= 0 && position.Row < _size && position.Column >= 0 && position.Column < _size;

        public bool IsWithinBounds(Ship ship) =>
            ship.GetPositions().All(position => IsWithinBounds(position));

        public bool IsValidPlacement(Ship ship)
        {
            if (!IsWithinBounds(ship)) return false;

            foreach (Position position in ship.GetPositions())
            {
                if (_cells[position.Row, position.Column] == CellState.Ship) return false;

                for (int row = position.Row - 1; row <= position.Row + 1; row++)
                {
                    for (int col = position.Column - 1; col <= position.Column + 1; col++)
                    {
                        var surroundingPos = new Position(row, col);
                        if (IsWithinBounds(surroundingPos) && _cells[surroundingPos.Row, surroundingPos.Column] == CellState.Ship
                            && !ship.ContainsPosition(surroundingPos)) return false;
                    }
                }
            }
            return true;
        }

        public void PlaceShip(Ship ship)
        {
            foreach (var position in ship.GetPositions())
            {
                _cells[position.Row, position.Column] = CellState.Ship;
                _shipPositions[position] = ship;
            }
        }

        public ShotResult TakeShot(Position position)
        {
            if (!IsWithinBounds(position)) throw new ArgumentOutOfRangeException(nameof(position));

            var currentState = _cells[position.Row, position.Column];
            if (currentState == CellState.Hit || currentState == CellState.Miss)
                throw new InvalidOperationException("Already shot here!");

            if (currentState == CellState.Ship)
            {
                _cells[position.Row, position.Column] = CellState.Hit;
                var hitShip = _shipPositions[position];
                hitShip.Hit(position);
                return hitShip.IsSunk ? ShotResult.Sunk : ShotResult.Hit;
            }

            _cells[position.Row, position.Column] = CellState.Miss;
            return ShotResult.Miss;
        }

        public void SetCellState(Position position, CellState state)
        {
            if (IsWithinBounds(position)) _cells[position.Row, position.Column] = state;
        }

        public CellState GetCellState(Position position) =>
            IsWithinBounds(position) ? _cells[position.Row, position.Column] : throw new ArgumentOutOfRangeException(nameof(position));

        public Ship GetShipAt(Position position) => _shipPositions.TryGetValue(position, out var ship) ? ship : null;

        public List<Position> GetSurroundingPositions(Ship ship)
        {
            var surroundingPositions = new List<Position>();

            foreach (var shipPosition in ship.GetPositions())
            {
                for (int row = shipPosition.Row - 1; row <= shipPosition.Row + 1; row++)
                {
                    for (int col = shipPosition.Column - 1; col <= shipPosition.Column + 1; col++)
                    {
                        var pos = new Position(row, col);
                        if (IsWithinBounds(pos) && !ship.ContainsPosition(pos) && !surroundingPositions.Contains(pos))
                            surroundingPositions.Add(pos);
                    }
                }
            }
            return surroundingPositions;
        }

        public void Print(bool showShips)
        {
            Console.Write("    ");
            for (int col = 0; col < _size; col++) Console.Write($"{(char)('A' + col)}   ");
            Console.WriteLine();

            for (int row = 0; row < _size; row++)
            {
                Console.Write($"{row + 1,2}  ");
                for (int col = 0; col < _size; col++)
                {
                    var state = _cells[row, col];
                    var symbol = state switch
                    {
                        CellState.Empty => " ",
                        CellState.Ship => showShips ? "#" : " ",
                        CellState.Hit => "X",
                        CellState.Miss => "O",
                        CellState.Unknown => " ",
                        CellState.RadarShip => "?",
                        _ => " "
                    };
                    Console.Write($"[{symbol}] ");
                }
                Console.WriteLine();
            }
        }
    }
}
