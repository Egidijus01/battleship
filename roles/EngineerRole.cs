using System;

namespace ShipBattle.Roles
{
    public class EngineerRole : IPlayerRole
    {
        public string Name => "Engineer";

        public void UseAbility(Player current, Player opponent, GameManager manager)
        {
            FortifyShip(current);
        }

        private void FortifyShip(Player player)
        {
            var ships = player.OwnBoard.GetShips();
            if (ships.Count == 0)
            {
                Console.WriteLine("Neturite nė vieno laivo, kurį būtų galima sutvirtinti.");
                return;
            }

            Console.WriteLine("Pasirinkite laivą sutvirtinimui:");
            for (int i = 0; i < ships.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Laivas (ilgis: {ships[i].Length})");
            }

            Console.Write("Pasirinkimas: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= ships.Count)
            {
                ships[choice - 1].Fortify();
                Console.WriteLine("Laivas sėkmingai sutvirtintas!");
            }
            else
            {
                Console.WriteLine("Neteisingas pasirinkimas.");
            }
        }
    }
}
