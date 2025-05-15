using System;

namespace ShipBattle
{
    public class TacticianRole : IPlayerRole
    {
        public string Name => "Tactician";

        public void UseAbility(Player current, Player opponent, GameManager manager)
        {
            UndoLastMove(current, manager);
        }

        private void UndoLastMove(Player player, GameManager manager)
        {
            if (player.Role.Name != "Tactician")
            {
                Console.WriteLine("Neturite teisės atšaukti ėjimo.");
                return;
            }

            if (manager.MoveHistory.Count == 0)
            {
                Console.WriteLine("Nėra ėjimų, kuriuos būtų galima atšaukti.");
                return;
            }

            var lastMove = manager.MoveHistory.Pop();

            // Čia turi būti logika, kaip atšaukti ėjimą, pavyzdžiui:
            lastMove.Undo();

            Console.WriteLine("Paskutinis ėjimas atšauktas.");
        }
    }
}
