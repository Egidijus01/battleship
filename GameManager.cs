namespace ShipBattle
{
    public class GameManager
    {
        private IRules _rules;
        private ILogger _logger;
        private IPlayer _player1;
        private IPlayer _player2;
        private IPlayer _currentPlayer;
        private IPlayer _opponentPlayer;
        private bool _gameOver;

        public static int TotalShots = 0;

        public GameManager()
        {
            _rules = new Rules();
            _logger = new Logger();
        }

        public void StartGame()
        {
            Console.Clear();
            _rules.SetupRules();

            Console.Clear();
            DisplayRulesSummary();

            _player1 = CreatePlayer("pirmojo");
            _player2 = CreatePlayer("antrojo");
            Random rand = new Random();
            if (rand.Next(2) == 0)
            {
                _currentPlayer = _player1;
                _opponentPlayer = _player2;
            }
            else
            {
                _currentPlayer = _player2;
                _opponentPlayer = _player1;
            }

            Console.WriteLine($"Pirmas pradeda: {_currentPlayer.Name}");
            PlaceShipsForPlayers();

            _logger.LogGameStart(_player1, _player2, _rules);

            while (!_gameOver)
            {
                GameCycle();
            }
        }

        private void DisplayRulesSummary()
        {
            Console.WriteLine($"Žaidimo lenta: {_rules.BoardSize}x{_rules.BoardSize}\nLaivų skaičius:");
            foreach (var ship in _rules.Ships) Console.WriteLine($"- {ship.Key} ilgio laivai: {ship.Value}");
            Console.WriteLine($"Bombų skaičius:\n- Radarai: {_rules.RadarBombs}\n- Sprogstamosios bangos: {_rules.ExplosiveBombs}\n");
        }

        private IPlayer CreatePlayer(string playerNumber)
        {
            Console.WriteLine($"Įveskite {playerNumber} žaidėjo vardą:");
            return new Player(Console.ReadLine(), _rules.BoardSize, _rules.Ships, _rules.RadarBombs, _rules.ExplosiveBombs);
        }

        private void PlaceShipsForPlayers()
        {
            ExecuteShipPlacement(_player1);
            Console.Clear();
            ExecuteShipPlacement(_player2);
        }

        private void ExecuteShipPlacement(IPlayer player)
        {
            Console.Clear();
            Console.WriteLine($"{player.Name}, išdėstykite savo laivus.\nGalimos kryptys: H (horizontaliai) arba V (vertikaliai)");

            foreach (var shipEntry in _rules.Ships)
            {
                for (int i = 0; i < shipEntry.Value; i++)
                {
                    PlaceSingleShip(player, shipEntry.Key, shipEntry.Value - i);
                }
            }

            Console.WriteLine($"{player.Name}, visi laivai išdėstyti!\nPaspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private void PlaceSingleShip(IPlayer player, int length, int remaining)
        {
            bool placed = false;
            while (!placed)
            {
                Console.Clear();
                player.PrintOwnBoard();
                Console.WriteLine($"Išdėstykite {length} ilgio laivą (liko: {remaining})");

                var (pos, dir) = GetShipPlacementDetails();
                try
                {
                    placed = AttemptShipPlacement(player, length, pos, dir);
                }
                catch (FormatException)
                {
                    HandleInvalidInput("Neteisingas koordinačių formatas");
                }
            }
        }

        private (Position pos, ShipDirection dir) GetShipPlacementDetails()
        {
            while (true)
            {
                try
                {
                    Console.Write("Įveskite koordinates (pvz., A1): ");
                    Position position = Position.FromString(Console.ReadLine().ToUpper());

                    Console.Write("Įveskite kryptį (H/V): ");
                    string direction = Console.ReadLine().ToUpper();
                    if (direction != "H" && direction != "V")
                        throw new FormatException();

                    return (position, direction == "H" ? ShipDirection.Horizontal : ShipDirection.Vertical);
                }
                catch (FormatException)
                {
                    HandleInvalidInput("Netinkama įvestis. Bandykite dar kartą.");
                }
            }
        }

        private bool AttemptShipPlacement(IPlayer player, int length, Position pos, ShipDirection dir)
        {
            Ship ship = new Ship(length, pos, dir);
            if (player.TryPlaceShip(ship))
            {
                _logger.LogShipPlacement(player.Name, ship);
                Console.WriteLine("Laivas sėkmingai išdėstytas!");
                return true;
            }

            HandleInvalidInput("Negalima pozicija");
            return false;
        }

        private void GameCycle()
        {
            Console.Clear();
            Console.WriteLine($"{_currentPlayer.Name} ėjimas\n");
            _currentPlayer.PrintOpponentBoard();
            _currentPlayer.PrintOwnBoard();
            Console.WriteLine($"\nRadarai: {_currentPlayer.RadarBombs}, Sprogstamosios bangos: {_currentPlayer.ExplosiveBombs}");

            bool maintainTurn = ExecutePlayerAction();

            if (_opponentPlayer.AreAllShipsSunk()) EndGame();
            if (!maintainTurn) SwitchPlayers();

            Console.WriteLine("Paspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private bool ExecutePlayerAction()
        {
            while (true)
            {
                Console.WriteLine("Pasirinkite veiksmą:\n1. Šauti\n2. Naudoti radarą\n3. Naudoti sprogstamąją bangą");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        return ExecutePositionAction(MakeShot);
                    case "2":
                        return ExecutePositionAction(UseRadar);
                    case "3":
                        return ExecutePositionAction(UseExplosiveBomb);
                    default:
                        HandleInvalidInput("Neteisingas pasirinkimas");
                        break;
                }
            }
        }

        private bool ExecutePositionAction(Func<Position, bool> action)
        {
            while (true)
            {
                try
                {
                    Console.Write("Įveskite koordinates: ");
                    Position position = Position.FromString(Console.ReadLine().ToUpper());
                    return action(position);
                }
                catch (FormatException)
                {
                    HandleInvalidInput("Neteisingas koordinačių formatas");
                }
                catch (InvalidOperationException ex)
                {
                    HandleInvalidInput(ex.Message);
                }
            }
        }

        private bool MakeShot(Position position)
        {
            ShotResult result = _currentPlayer.Shoot(position, _opponentPlayer.OwnBoard);
            _logger.LogShot(_currentPlayer.Name, position.ToString(), result);

            TotalShots++;

            Console.WriteLine(result switch
            {
                ShotResult.Hit => "Pataikėte!",
                ShotResult.Sunk => "Pataikėte ir nuskandinote laivą!",
                _ => "Nepataikėte!"
            });
            return result == ShotResult.Hit || result == ShotResult.Sunk;
        }

        private bool UseRadar(Position position)
        {
            if (_currentPlayer.RadarBombs <= 0)
                throw new InvalidOperationException("Nebeturite radarų");

            _currentPlayer.UseRadar(position, _opponentPlayer.OwnBoard);
            _logger.LogRadarUse(_currentPlayer.Name, position.ToString());
            Console.WriteLine("Radaras panaudotas");
            return false;
        }

        private bool UseExplosiveBomb(Position position)
        {
            if (_currentPlayer.ExplosiveBombs <= 0)
                throw new InvalidOperationException("Nebeturite bombų");

            bool hit = _currentPlayer.UseExplosiveBomb(position, _opponentPlayer.OwnBoard);
            _logger.LogExplosiveBombUse(_currentPlayer.Name, position.ToString(), hit);
            Console.WriteLine(hit ? "Pataikėte!" : "Nepataikėte!");
            return hit;
        }

        private void EndGame()
        {
            _gameOver = true;
            Console.Clear();
            Console.WriteLine($"Žaidimas baigtas! {_currentPlayer.Name} laimėjo!");
            Console.WriteLine($"Iš viso atlikta šūvių: {TotalShots}");
            _logger.LogGameEnd(_currentPlayer.Name);
            _logger.Dispose();
        }

        private void SwitchPlayers() => (_currentPlayer, _opponentPlayer) = (_opponentPlayer, _currentPlayer);

        private void HandleInvalidInput(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Paspauskite bet kurį klavišą...");
            Console.ReadKey();
        }
    }
}
