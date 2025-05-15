namespace ShipBattle.Utils
{
    public interface ILogger : IDisposable
    {
        void LogGameStart(IPlayer player1, IPlayer player2, IRules rules);
        void LogShipPlacement(string playerName, Ship ship);
        void LogShot(string playerName, string coordinates, ShotResult result);
        void LogRadarUse(string playerName, string coordinates);
        void LogExplosiveBombUse(string playerName, string coordinates, bool hit);
        void LogGameEnd(string winnerName);
    }

    public sealed class Logger : ILogger
    {
        private readonly StreamWriter _logWriter;

        public Logger()
        {
            if (!Directory.Exists("log"))
            {
                Directory.CreateDirectory("log");
            }
            _logWriter = new StreamWriter($"log/game_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        public void LogGameStart(IPlayer player1, IPlayer player2, IRules rules)
        {
            LogMessage("=== ŽAIDIMO PRADŽIA ===");
            LogMessage($"Žaidėjai: {player1.Name} vs {player2.Name}");
            LogMessage($"Lentos dydis: {rules.BoardSize}x{rules.BoardSize}");
            foreach (var ship in rules.Ships)
            {
                LogMessage($"Laivai {ship.Key} ilgio: {ship.Value}");
            }
            LogMessage($"Radarų skaičius: {rules.RadarBombs}");
            LogMessage($"Sprogstamųjų bangų skaičius: {rules.ExplosiveBombs}");
        }

        public void LogShipPlacement(string playerName, Ship ship)
        {
            LogMessage($"Žaidėjas {playerName} išdėstė {ship.Length} ilgio laivą nuo {ship.StartPosition} kryptimi {ship.Direction}");
        }

        public void LogShot(string playerName, string coordinates, ShotResult result)
        {
            string resultText = result switch
            {
                ShotResult.Hit => "pataikė į laivą",
                ShotResult.Miss => "nepataikė",
                ShotResult.Sunk => "nuskandino laivą",
                _ => "nežinomas rezultatas"
            };
            LogMessage($"Žaidėjas {playerName} šovė į {coordinates} ir {resultText}");
        }

        public void LogRadarUse(string playerName, string coordinates)
        {
            LogMessage($"Žaidėjas {playerName} panaudojo radarą pozicijoje {coordinates}");
        }

        public void LogExplosiveBombUse(string playerName, string coordinates, bool hit)
        {
            LogMessage($"Žaidėjas {playerName} panaudojo sprogstamąją bangą pozicijoje {coordinates} ir {(hit ? "pataikė" : "nepataikė")}");
        }

        public void LogGameEnd(string winnerName)
        {
            LogMessage("=== ŽAIDIMO PABAIGA ===");
            LogMessage($"Laimėtojas: {winnerName}");
            LogMessage($"Baigta: {DateTime.Now}");
        }

        private void LogMessage(string message)
        {
            _logWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            _logWriter.Flush();
        }

        public void Dispose()
        {
            _logWriter.Dispose();
        }
    }
}
