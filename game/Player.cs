namespace ShipBattle.Core
{
    public abstract class Player : IPlayer
    {
        public string Name { get; protected set; }
        public Board OwnBoard { get; protected set; }
        public Board OpponentBoard { get; protected set; }
        public PlayerRole Role { get; protected set; }

        protected bool _hasUsedSpecialEffect = false;

        protected IPlayerRole _roleImplementation;

        protected List<Ship> _ships;

        public Player(string name, int boardSize, PlayerRole role, IPlayerRole roleImplementation)
        {
            Name = name;
            OwnBoard = new Board(boardSize);
            OpponentBoard = new Board(boardSize, true);
            Role = role;
            _roleImplementation = roleImplementation;
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

        public ShotResult Shoot(Position position, Board targetBoard)
        {
            var cellState = OpponentBoard.GetCellState(position);
            if (cellState != CellState.Unknown && cellState != CellState.RadarShip)
                throw new InvalidOperationException("Jau šauta į šią poziciją!");

            ShotResult result = targetBoard.TakeShot(position);
            UpdateOpponentBoard(position, result, targetBoard);
            return result;
        }

        public void UseSpecialEffect(Position position, Player opponent, GameManager manager)
        {
            if (_hasUsedSpecialEffect)
            {
                Console.WriteLine("Specialus gebėjimas jau panaudotas.");
                return;
            }

            _roleImplementation.UseAbility(this, opponent, manager, position);
            _hasUsedSpecialEffect = true;
        }

        public bool AreAllShipsSunk() => _ships.All(s => s.IsSunk);

        public void PrintOwnBoard() => OwnBoard.Print(true);

        public void PrintOpponentBoard() => OpponentBoard.Print(false);

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
                    foreach (var pos in sunkShip.GetPositions())
                        OpponentBoard.SetCellState(pos, CellState.Hit);

                    foreach (var surrounding in targetBoard.GetSurroundingPositions(sunkShip))
                    {
                        if (OpponentBoard.IsWithinBounds(surrounding) &&
                            (OpponentBoard.GetCellState(surrounding) == CellState.Unknown ||
                             OpponentBoard.GetCellState(surrounding) == CellState.RadarShip))
                            OpponentBoard.SetCellState(surrounding, CellState.Miss);
                    }
                }
            }
        }
    }
}
