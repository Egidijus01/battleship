namespace ShipBattle
{
    public interface IPlayer
    {
        string Name { get; }
        Board OwnBoard { get; }

        int RadarBombs { get; }
        int ExplosiveBombs { get; }

        bool TryPlaceShip(Ship ship);
        ShotResult Shoot(Position position, Board opponentBoard);
        void UseRadar(Position position, Board opponentBoard);
        bool UseExplosiveBomb(Position position, Board opponentBoard);

        void PrintOwnBoard();
        void PrintOpponentBoard();

        bool AreAllShipsSunk();
    }


    public class Player : IPlayer
    {
        public string Name { get; private set; }
        public Board OwnBoard { get; private set; }
        public Board OpponentBoard { get; private set; }
        public int RadarBombs { get; private set; }
        public int ExplosiveBombs { get; private set; }
        private List<Ship> _ships;

        public Player(string name, int boardSize, Dictionary<int, int> ships, int radarBombs, int explosiveBombs)
        {
            Name = name;
            OwnBoard = new Board(boardSize);
            OpponentBoard = new Board(boardSize, true);
            RadarBombs = radarBombs;
            ExplosiveBombs = explosiveBombs;
            _ships = new List<Ship>();
        }

        public bool TryPlaceShip(Ship ship)
        {
            if (!OwnBoard.IsWithinBounds(ship) || !OwnBoard.IsValidPlacement(ship))
                return false;

            OwnBoard.PlaceShip(ship);
            _ships.Add(ship);
            return true;
        }

        public void PrintOwnBoard() => OwnBoard.Print(true);
        public void PrintOpponentBoard() => OpponentBoard.Print(false);

        public ShotResult Shoot(Position position, Board targetBoard)
        {
            if (OpponentBoard.GetCellState(position) != CellState.Unknown && OpponentBoard.GetCellState(position) != CellState.RadarShip)
                throw new InvalidOperationException("Already shot at this position!");

            ShotResult result = targetBoard.TakeShot(position);
            UpdateOpponentBoard(position, result, targetBoard);
            return result;
        }

        public bool AreAllShipsSunk() => _ships.All(ship => ship.IsSunk);

        public void UseRadar(Position position, Board targetBoard)
        {
            if (RadarBombs <= 0)
                throw new InvalidOperationException("No radar bombs left!");

            int radarRadius = new Random().Next(1, 4);
            for (int row = position.Row - radarRadius; row <= position.Row + radarRadius; row++)
            {
                for (int col = position.Column - radarRadius; col <= position.Column + radarRadius; col++)
                {
                    Position radarPos = new Position(row, col);
                    if (targetBoard.IsWithinBounds(radarPos) && OpponentBoard.GetCellState(radarPos) == CellState.Unknown)
                    {
                        CellState targetState = targetBoard.GetCellState(radarPos);
                        OpponentBoard.SetCellState(radarPos, targetState == CellState.Ship ? CellState.RadarShip : CellState.Miss);
                    }
                }
            }
            RadarBombs--;
        }

        public bool UseExplosiveBomb(Position position, Board targetBoard)
        {
            if (ExplosiveBombs <= 0)
                throw new InvalidOperationException("No explosive bombs left!");

            bool hitAnyShip = false;
            for (int row = position.Row - 1; row <= position.Row + 1; row++)
            {
                for (int col = position.Column - 1; col <= position.Column + 1; col++)
                {
                    Position bombPos = new Position(row, col);
                    if (targetBoard.IsWithinBounds(bombPos) && OpponentBoard.GetCellState(bombPos) == CellState.Unknown)
                    {
                        ShotResult result = targetBoard.TakeShot(bombPos);
                        if (result == ShotResult.Hit || result == ShotResult.Sunk)
                        {
                            hitAnyShip = true;
                            OpponentBoard.SetCellState(bombPos, CellState.Hit);
                            MarkSunkShip(bombPos, targetBoard);
                        }
                        else
                        {
                            OpponentBoard.SetCellState(bombPos, CellState.Miss);
                        }
                    }
                }
            }
            ExplosiveBombs--;
            return hitAnyShip;
        }

        private void UpdateOpponentBoard(Position position, ShotResult result, Board targetBoard)
        {
            if (result == ShotResult.Miss)
                OpponentBoard.SetCellState(position, CellState.Miss);
            else if (result == ShotResult.Hit)
                OpponentBoard.SetCellState(position, CellState.Hit);
            else if (result == ShotResult.Sunk)
            {
                OpponentBoard.SetCellState(position, CellState.Hit);
                Ship sunkShip = targetBoard.GetShipAt(position);
                if (sunkShip != null)
                {
                    foreach (Position shipPos in sunkShip.GetPositions())
                        OpponentBoard.SetCellState(shipPos, CellState.Hit);
                    foreach (Position surroundingPos in targetBoard.GetSurroundingPositions(sunkShip))
                        if (OpponentBoard.IsWithinBounds(surroundingPos) &&
                            (OpponentBoard.GetCellState(surroundingPos) == CellState.Unknown ||
                             OpponentBoard.GetCellState(surroundingPos) == CellState.RadarShip))
                            OpponentBoard.SetCellState(surroundingPos, CellState.Miss);
                }
            }
        }

        private void MarkSunkShip(Position bombPos, Board targetBoard)
        {
            Ship sunkShip = targetBoard.GetShipAt(bombPos);
            if (sunkShip != null)
            {
                foreach (Position shipPos in sunkShip.GetPositions())
                    OpponentBoard.SetCellState(shipPos, CellState.Hit);
                foreach (Position surroundingPos in targetBoard.GetSurroundingPositions(sunkShip))
                    if (OpponentBoard.IsWithinBounds(surroundingPos) &&
                        (OpponentBoard.GetCellState(surroundingPos) == CellState.Unknown ||
                        OpponentBoard.GetCellState(surroundingPos) == CellState.RadarShip))
                        OpponentBoard.SetCellState(surroundingPos, CellState.Miss);
            }
        }
    }
}
