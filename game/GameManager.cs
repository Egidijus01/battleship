using ShipBattle.Roles;
using ShipBattle.Utils;
using ShipBattle.Exceptions;

namespace ShipBattle.Core
{
    public class GameManager
    {
        private IRules _rules;
        private ILogger _logger;
        private List<Player> _players;
        private int _currentPlayerIndex;
        private bool _gameOver;

        private Stack<MoveRecord> _moveHistory = new Stack<MoveRecord>();

        private bool _doubleShotAllowed = false;
        private int _shotsThisTurn = 0;

        public static int TotalShots = 0;

        public GameManager()
        {
            _rules = new Rules();
            _logger = new Logger();
            _players = new List<Player>();
            _currentPlayerIndex = 0;
        }

        public void StartGame()
        {
            Console.Clear();
            _rules.SetupRules();

            Console.Clear();
            DisplayRulesSummary();

            _players.Add(CreatePlayer("pirmojo"));
            _players.Add(CreatePlayer("antrojo"));

            AssignRolesAndRoleInstances();

            // Atsitiktinis pradžios žaidėjas
            Random rand = new Random();
            _currentPlayerIndex = rand.Next(2);

            Console.WriteLine($"Pirmas pradeda: {_players[_currentPlayerIndex].Name}");

            PlaceShipsForPlayers();

            _logger.LogGameStart(_players[0], _players[1], _rules);

            while (!_gameOver)
            {
                GameCycle();
            }
        }

        private void AssignRolesAndRoleInstances()
        {
            _players[0].Role = PlayerRole.Bomber;
            _players[0].RoleInstance = RoleFactory.CreateRole(_players[0].Role);

            _players[1].Role = PlayerRole.Spy;
            _players[1].RoleInstance = RoleFactory.CreateRole(_players[1].Role);
        }

        private void DisplayRulesSummary()
        {
            Console.WriteLine($"Žaidimo lenta: {_rules.BoardSize}x{_rules.BoardSize}\nLaivų skaičius:");
            foreach (var ship in _rules.Ships) Console.WriteLine($"- {ship.Key} ilgio laivai: {ship.Value}");
            Console.WriteLine($"Bombų skaičius:\n- Radarai: {_rules.RadarBombs}\n- Sprogstamosios bangos: {_rules.ExplosiveBombs}\n");
        }

        private Player CreatePlayer(string playerNumber)
        {
            Console.WriteLine($"Įveskite {playerNumber} žaidėjo vardą:");
            string name = Console.ReadLine();
            return new Player(name, _rules.BoardSize, _rules.Ships, _rules.RadarBombs, _rules.ExplosiveBombs, PlayerRole.Admiral);
        }

        private void PlaceShipsForPlayers()
        {
            foreach (var player in _players)
            {
                ExecuteShipPlacement(player);
                Console.Clear();
            }
        }

        private void ExecuteShipPlacement(Player player)
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

        private void PlaceSingleShip(Player player, int length, int remaining)
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
                    Console.Write("Įveskite koordinates (pvz., A1): ");
                    string input = Console.ReadLine().ToUpper();
                    if (string.IsNullOrWhiteSpace(input))
                        throw new EgidijusGagelaException("Įvestis negali būti tuščia", "Klaida įvedant laivo poziciją.");

                    Position position = Position.FromString(input);

                    Console.Write("Įveskite kryptį (H/V): ");
                    string direction = Console.ReadLine().ToUpper();
                    if (direction != "H" && direction != "V")
                        throw new EgidijusGagelaException("Neteisinga kryptis (turi būti H arba V)", $"Įvesta: {direction}");

                    return (position, direction == "H" ? ShipDirection.Horizontal : ShipDirection.Vertical);
                }
                catch (EgidijusGagelaException ex)
                {
                    Console.WriteLine($"Klaida: {ex.Message}");
                    Console.WriteLine("Bandykite dar kartą.");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Netinkamas koordinačių formatas. Bandykite dar kartą.");
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

        private void GameCycle()
        {
            Console.Clear();
            Player currentPlayer = _players[_currentPlayerIndex];
            Player opponentPlayer = _players[(_currentPlayerIndex + 1) % 2];

            Console.WriteLine($"{currentPlayer.Name} ėjimas (Rolė: {currentPlayer.Role})\n");
            currentPlayer.PrintOpponentBoard();
            currentPlayer.PrintOwnBoard();
            Console.WriteLine($"\nRadarai: {currentPlayer.RadarBombs}, Sprogstamosios bangos: {currentPlayer.ExplosiveBombs}");

            _shotsThisTurn = 0;
            _doubleShotAllowed = false;

            bool maintainTurn = false;
            do
            {
                maintainTurn = ExecutePlayerAction(currentPlayer, opponentPlayer);

                if (opponentPlayer.AreAllShipsSunk())
                {
                    EndGame(currentPlayer);
                    return;
                }

                _shotsThisTurn++;
                if (!_doubleShotAllowed || _shotsThisTurn >= 2)
                    break;

                Console.WriteLine("Galite atlikti dar vieną šūvį (dėl FastShip rolės).");
            }
            while (maintainTurn && _doubleShotAllowed && _shotsThisTurn < 2);

            if (!maintainTurn)
                SwitchPlayers();

            Console.WriteLine("Paspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private bool ExecutePlayerAction(Player currentPlayer, Player opponentPlayer)
        {
            while (true)
            {
                Console.WriteLine("Pasirinkite veiksmą:\n1. Šauti\n2. Naudoti specialų gebėjimą");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        return ExecutePositionAction(currentPlayer, opponentPlayer, MakeShot);

                    case "2":
                        UseSpecialAbility(currentPlayer, opponentPlayer);
                        return false;

                    default:
                        HandleInvalidInput("Neteisingas pasirinkimas");
                        break;
                }
            }
        }

        private bool ExecutePositionAction(Player currentPlayer, Player opponentPlayer, Func<Player, Player, Position, bool> action)
        {
            while (true)
            {
                try
                {
                    Console.Write("Įveskite koordinates: ");
                    Position position = Position.FromString(Console.ReadLine().ToUpper());
                    return action(currentPlayer, opponentPlayer, position);
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

        private bool MakeShot(Player shooter, Player target, Position position)
        {
            ShotResult result = shooter.Shoot(position, target.OwnBoard);
            _logger.LogShot(shooter.Name, position.ToString(), result);
            TotalShots++;

            _moveHistory.Push(new MoveRecord(shooter, target, position, result));

            Console.WriteLine(result switch
            {
                ShotResult.Hit => "Pataikėte!",
                ShotResult.Sunk => "Pataikėte ir nuskandinote laivą!",
                _ => "Nepataikėte!"
            });

            return result == ShotResult.Hit || result == ShotResult.Sunk;
        }

        private void UseSpecialAbility(Player currentPlayer, Player opponentPlayer)
        {
            if (currentPlayer == null) return;

            if (currentPlayer.Role == PlayerRole.FastShip)
            {
                if (_doubleShotAllowed)
                {
                    Console.WriteLine("Specialus gebėjimas jau aktyvuotas šiam ėjimui.");
                    return;
                }
                _doubleShotAllowed = true;
                Console.WriteLine("Greitas laivas leidžia šauti du kartus šį ėjimą!");
                currentPlayer.RoleInstance.UseAbility(currentPlayer, opponentPlayer, this);
                return;
            }

            Console.WriteLine("Pasirinkite specialų gebėjimą naudoti:");
            Console.WriteLine("1. Radaras");
            Console.WriteLine("2. Sprogstamoji bomba");
            Console.WriteLine("3. Šnipas (atveria eilutę arba stulpelį)");
            Console.WriteLine("4. Inžinierius (sutvirtina laivą)");
            Console.WriteLine("5. Taktikas (atšaukia paskutinį ėjimą)");
            Console.WriteLine("6. Medikas (pataiso laivą)");
            Console.Write("Jūsų pasirinkimas: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ExecutePositionAction(currentPlayer, opponentPlayer, (cur, opp, pos) =>
                    {
                        cur.UseRadar(pos, opp.OwnBoard);
                        _logger.LogRadarUse(cur.Name, pos.ToString());
                        Console.WriteLine("Radaras panaudotas.");
                        return false;
                    });
                    break;

                case "2":
                    ExecutePositionAction(currentPlayer, opponentPlayer, (cur, opp, pos) =>
                    {
                        bool hit = cur.UseExplosiveBomb(pos, opp.OwnBoard);
                        _logger.LogExplosiveBombUse(cur.Name, pos.ToString(), hit);
                        Console.WriteLine(hit ? "Pataikėte!" : "Nepataikėte!");
                        return hit;
                    });
                    break;

                case "3":
                    ExecutePositionAction(currentPlayer, opponentPlayer, (cur, opp, pos) =>
                    {
                        cur.RoleInstance.UseAbility(cur, opp, this, pos);
                        Console.WriteLine("Šnipas panaudojo šnipinėjimo įgūdžius!");
                        return false;
                    });
                    break;

                case "4":
                    currentPlayer.RoleInstance.UseAbility(currentPlayer, opponentPlayer, this);
                    Console.WriteLine("Inžinierius sutvirtino vieną laivą!");
                    break;

                case "5":
                    currentPlayer.RoleInstance.UseAbility(currentPlayer, opponentPlayer, this);
                    break;

                case "6":
                    currentPlayer.RoleInstance.UseAbility(currentPlayer, opponentPlayer, this);
                    Console.WriteLine("Medikas pataisė laivą!");
                    break;

                default:
                    Console.WriteLine("Neteisingas pasirinkimas.");
                    break;
            }
        }

        public void UndoLastMove(Player player)
        {
            if (_moveHistory.Count == 0)
            {
                Console.WriteLine("Nėra ėjimų, kuriuos būtų galima atšaukti.");
                return;
            }

            var lastMove = _moveHistory.Pop();

            if (lastMove.Shooter != player)
            {
                Console.WriteLine("Galite atšaukti tik savo paskutinį ėjimą.");
                _moveHistory.Push(lastMove);
                return;
            }

            lastMove.Target.OwnBoard.RevertShot(lastMove.ShotPosition, lastMove.Result);
            Console.WriteLine("Paskutinis ėjimas atšauktas.");

            // Jei reikia, pakeisti žaidėjo einamumą
            if (_players[_currentPlayerIndex] != player)
                SwitchPlayers();
        }

        private void EndGame(Player winner)
        {
            _gameOver = true;
            Console.Clear();
            Console.WriteLine($"Žaidimas baigtas! {winner.Name} laimėjo!");
            Console.WriteLine($"Iš viso atlikta šūvių: {TotalShots}");
            _logger.LogGameEnd(winner.Name);
            _logger.Dispose();
        }

        private void SwitchPlayers()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % 2;
        }

        private void HandleInvalidInput(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Paspauskite bet kurį klavišą...");
            Console.ReadKey();
        }

        private class MoveRecord
        {
            public Player Shooter { get; }
            public Player Target { get; }
            public Position ShotPosition { get; }
            public ShotResult Result { get; }

            public MoveRecord(Player shooter, Player target, Position position, ShotResult result)
            {
                Shooter = shooter;
                Target = target;
                ShotPosition = position;
                Result = result;
            }
        }
    }

    public static class RoleFactory
    {
        public static IPlayerRole CreateRole(PlayerRole role)
        {
            return role switch
            {
                PlayerRole.FastShip => new FastShipRole(),
                PlayerRole.Tactician => new TacticianRole(),
                PlayerRole.Bomber => new BomberRole(),
                PlayerRole.Spy => new SpyRole(),
                PlayerRole.Engineer => new EngineerRole(),
                PlayerRole.Medic => new MedicRole(),
                PlayerRole.Admiral => new AdmiralRole(),
                _ => throw new ArgumentException("Nežinoma rolė"),
            };
        }
    }
}
