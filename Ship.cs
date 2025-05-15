namespace ShipBattle
{
    public enum ShipDirection
    {
        Horizontal,
        Vertical
    }

    public class Ship
    {
        public int Length { get; private set; }
        public Position StartPosition { get; private set; }
        public ShipDirection Direction { get; private set; }
        public bool IsSunk => _hitPositions.Count == Length;

        private readonly HashSet<Position> _positions;
        private readonly HashSet<Position> _hitPositions;

        public Ship(int length, Position startPosition, ShipDirection direction)
        {
            Length = length;
            StartPosition = startPosition;
            Direction = direction;
            _positions = new HashSet<Position>();
            _hitPositions = new HashSet<Position>();

            CalculatePositions();
        }

        private void CalculatePositions()
        {
            _positions.Clear();

            for (int i = 0; i < Length; i++)
            {
                Position position;

                if (Direction == ShipDirection.Horizontal)
                {
                    position = new Position(StartPosition.Row, StartPosition.Column + i);
                }
                else
                {
                    position = new Position(StartPosition.Row + i, StartPosition.Column);
                }

                _positions.Add(position);
            }
        }

        public bool ContainsPosition(Position position)
        {
            return _positions.Contains(position);
        }

        public void Hit(Position position)
        {
            if (ContainsPosition(position))
            {
                _hitPositions.Add(position);
            }
        }

        public IEnumerable<Position> GetPositions()
        {
            return _positions;
        }

        public override string ToString()
        {
            return $"Laivas [{Length}] nuo {StartPosition} iki {GetEndPosition()} ({Direction})";
        }

        private Position GetEndPosition()
        {
            if (Direction == ShipDirection.Horizontal)
            {
                return new Position(StartPosition.Row, StartPosition.Column + Length - 1);
            }
            else
            {
                return new Position(StartPosition.Row + Length - 1, StartPosition.Column);
            }
        }
    }
}
