using ShipBattle.Core;

namespace ShipBattle.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Laivų Mūšis";
            Console.WriteLine("===== LAIVŲ MŪŠIS =====");

            GameManager gameManager = new GameManager();
            gameManager.StartGame();

            Console.WriteLine("Žaidimas baigtas. Paspauskite bet kurį klavišą, kad uždarytumėte programą...");
            Console.ReadKey();
        }
    }
}
