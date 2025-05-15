using System;
using System.Collections.Generic;

namespace ShipBattle.Roles
{
    public class MedicRole : IPlayerRole
    {
        public string Name => "Medic";

        public void UseAbility(Player current, Player opponent, GameManager manager)
        {
            RepairShip(current);
        }

        private void RepairShip(Player player)
        {
            var damagedShips = player.OwnBoard.GetShips().FindAll(s => s.IsDamaged);
            if (damagedShips.Count == 0)
            {
                Console.WriteLine("Nėra pažeistų laivų, kuriuos galima pataisyti.");
                return;
            }

            Console.WriteLine("Pasirinkite laivą taisymui:");
            for (int i = 0; i < damagedShips.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Laivas (ilgis: {damagedShips[i].Length}, pažeistas: {damagedShips[i].GetDamageCount()} ląst.)");
            }

            Console.Write("Pasirinkimas: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= damagedShips.Count)
            {
                damagedShips[choice - 1].Repair();
                Console.WriteLine("Laivas pataisytas.");
            }
            else
            {
                Console.WriteLine("Neteisingas pasirinkimas.");
            }
        }
    }
}
