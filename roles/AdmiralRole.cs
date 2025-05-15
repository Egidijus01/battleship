using System;

namespace ShipBattle.Roles
{
    public class AdmiralRole : IPlayerRole
    {
        public string Name => "Admiral";
        public int RadarBombs { get; private set; } = 2;

        public void UseAbility(Player current, Player opponent, GameManager manager)
        {
            Console.Write("Įveskite radaro centro koordinatę: ");
            string input = Console.ReadLine().ToUpper();
            Position center = Position.FromString(input);

            UseRadar(center, opponent.Board, current.OpponentBoard);
        }

        private void UseRadar(Position position, Board targetBoard, Board opponentFogBoard)
        {
            if (RadarBombs <= 0)
                throw new InvalidOperationException("Radarų nebeliko!");

            int radarRadius = 1;

            for (int row = position.Row - radarRadius; row <= position.Row + radarRadius; row++)
            {
                for (int col = position.Column - radarRadius; col <= position.Column + radarRadius; col++)
                {
                    Position radarPos = new Position(row, col);

                    if (!targetBoard.IsWithinBounds(radarPos)) continue;

                    if (opponentFogBoard.GetCellState(radarPos) != CellState.Unknown) continue;

                    CellState realState = targetBoard.GetCellState(radarPos);
                    opponentFogBoard.SetCellState(radarPos,
                        realState == CellState.Ship ? CellState.RadarShip : CellState.Miss);
                }
            }

            RadarBombs--;
        }
    }
}
