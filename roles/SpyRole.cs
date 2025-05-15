using System;

namespace ShipBattle.Roles
{
    public class SpyRole : IPlayerRole
    {
        public string Name => "Spy";

        public void UseAbility(Player current, Player opponent, GameManager manager)
        {
            Console.Write("Įveskite langelio koordinatę eilutės/stulpelio atskleidimui (pvz. B3): ");
            string input = Console.ReadLine().ToUpper();
            Position pos = Position.FromString(input);

            RevealRowOrColumn(pos, opponent.Board);
        }

        private void RevealRowOrColumn(Position position, Board opponentBoard)
        {
            Console.WriteLine("Ar norite atskleisti eilutę (R) ar stulpelį (C)?");
            string choice = Console.ReadLine()?.Trim().ToUpper();

            if (choice == "R")
            {
                Console.WriteLine($"Eilutė {position.Row}:");
                for (int col = 0; col < opponentBoard.Size; col++)
                {
                    var cell = opponentBoard.GetCell(new Position(position.Row, col));
                    Console.Write($"{(cell.HasShip ? "S" : ".")} ");
                }
                Console.WriteLine();
            }
            else if (choice == "C")
            {
                Console.WriteLine($"Stulpelis {position.Column}:");
                for (int row = 0; row < opponentBoard.Size; row++)
                {
                    var cell = opponentBoard.GetCell(new Position(row, position.Column));
                    Console.WriteLine(cell.HasShip ? "S" : ".");
                }
            }
            else
            {
                Console.WriteLine("Neteisingas pasirinkimas (turi būti R arba C).");
            }
        }
    }
}
