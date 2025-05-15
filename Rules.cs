namespace ShipBattle
{
    public interface IRules
    {
        int BoardSize { get; }
        Dictionary<int, int> Ships { get; }
        int RadarBombs { get; }
        int ExplosiveBombs { get; }

        void SetupRules();
    }

    public class Rules : IRules
    {
        public int BoardSize { get; private set; }
        public Dictionary<int, int> Ships { get; private set; }
        public int RadarBombs { get; private set; }
        public int ExplosiveBombs { get; private set; }

        private const int MIN_BOARD_SIZE = 8;
        private const int MAX_BOARD_SIZE = 20;

        public Rules()
        {
            ResetToDefaults();
        }

        public void SetupRules()
        {
            Console.WriteLine("===== ŽAIDIMO TAISYKLIŲ NUSTATYMAS =====");
            SetBoardSize();
            SetShipCounts();
            SetBombCounts();

            if (!ValidateShipsWillFit())
            {
                Console.WriteLine("Klaida: laivai netilps į lentą. Grįžtama į numatytuosius nustatymus.");
                ResetToDefaults();
            }
        }

        private int GetIntInput(string prompt, int defaultValue)
        {
            Console.Write($"{prompt} (default: {defaultValue}): ");
            return int.TryParse(Console.ReadLine(), out int val) ? val : defaultValue;
        }

        private void SetBoardSize()
        {
            while (true)
            {
                Console.WriteLine($"Įveskite lentos dydį ({MIN_BOARD_SIZE}-{MAX_BOARD_SIZE}):");
                BoardSize = GetIntInput($"Įveskite lentos dydį ({MIN_BOARD_SIZE}-{MAX_BOARD_SIZE})", BoardSize);
                if (BoardSize >= MIN_BOARD_SIZE && BoardSize <= MAX_BOARD_SIZE) break;

                Console.WriteLine("Neteisingas įvestis. Bandykite dar kartą.");
            }
        }

        private void SetShipCounts()
        {
            Console.WriteLine("Nustatykite laivų skaičių:");
            foreach (var ship in Ships.Keys)
            {
                Console.WriteLine($"Kiek {ship} ilgio laivų? (numatytasis: {Ships[ship]}):");
                GetIntInput($"Kiek {ship} ilgio laivų?", Ships[ship]);
            }
        }

        private void SetBombCounts()
        {
            RadarBombs = GetValidInput("Kiek radarų?", RadarBombs);
            ExplosiveBombs = GetValidInput("Kiek sprogstamųjų bangų?", ExplosiveBombs);
        }

        private int GetValidInput(string message, int defaultValue)
        {
            Console.WriteLine($"{message} (numatytasis: {defaultValue}):");
            string input = Console.ReadLine();
            return int.TryParse(input, out int value) && value >= 0 ? value : defaultValue;
        }

        private bool ValidateShipsWillFit()
        {
            int totalShipCells = 0;
            foreach (var ship in Ships)
            {
                totalShipCells += ship.Key * ship.Value;
            }

            int estimatedSpaceNeeded = totalShipCells * 3;
            return estimatedSpaceNeeded <= BoardSize * BoardSize;
        }

        private void ResetToDefaults()
        {
            BoardSize = 10;
            Ships = new Dictionary<int, int> { { 4, 1 }, { 3, 2 }, { 2, 3 }, { 1, 4 } };
            RadarBombs = 2;
            ExplosiveBombs = 2;
            Console.WriteLine("Taisyklės atstatytos į numatytąsias.");
        }
    }
}
