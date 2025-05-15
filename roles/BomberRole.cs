using System;

namespace ShipBattle.Roles
{
    public class BomberRole : IPlayerRole
    {
        public string Name => "Bomber";
        public int ExplosiveBombs { get; private set; } = 2;

        public void UseAbility(Player current, Player opponent, GameManager manager)
        {
            Console.Write("Įveskite bombos centro koordinatę: ");
            string input = Console.ReadLine().ToUpper();
            Position center = Position.FromString(input);

            bool hit = UseExplosiveBomb(center, opponent.Board, current.OpponentBoard, manager);
            Console.WriteLine(hit ? "Bombardavimas pataikė!" : "Bombardavimas nepataikė.");
        }

        private bool UseExplosiveBomb(Position center, Board targetBoard, Board opponentFogBoard, GameManager manager)
        {
            if (ExplosiveBombs <= 0)
                throw new InvalidOperationException("Sprogstamų bombų nebeliko!");

            bool hitAnyShip = false;

            for (int row = center.Row - 1; row <= center.Row + 1; row++)
            {
                for (int col = center.Column - 1; col <= center.Column + 1; col++)
                {
                    Position bombPos = new Position(row, col);
                    if (!targetBoard.IsWithinBounds(bombPos)) continue;

                    var currentState = opponentFogBoard.GetCellState(bombPos);
                    if (currentState != CellState.Unknown) continue;

                    ShotResult result = targetBoard.TakeShot(bombPos);
                    if (result == ShotResult.Hit || result == ShotResult.Sunk)
                    {
                        hitAnyShip = true;
                        opponentFogBoard.SetCellState(bombPos, CellState.Hit);
                        manager.MarkSunkShipIfNeeded(bombPos, targetBoard, opponentFogBoard);
                    }
                    else
                    {
                        opponentFogBoard.SetCellState(bombPos, CellState.Miss);
                    }
                }
            }

            ExplosiveBombs--;
            return hitAnyShip;
        }
    }
}
