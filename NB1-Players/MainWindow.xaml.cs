using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NB1_Players
{
    public partial class MainWindow : Window
    {
        private PlayerDataService _dataService;
        private List<Player> _currentPlayers = new List<Player>();
        private Player? _selectedPlayer;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new PlayerDataService();
            LoadData();
            InitializeForm();

            SearchTextBox.GotFocus += SearchTextBox_GotFocus;
            SearchTextBox.LostFocus += SearchTextBox_LostFocus;
            SetSearchWatermark();
        }
        private void SetSearchWatermark()
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Keresés...";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Keresés...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Keresés...";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        private void LoadData()
        {
            var players = _dataService.GetAllPlayers();
            if (players != null)
            {
                _currentPlayers = players;
            }
            else
            {
                _currentPlayers = new List<Player>();
            }
            RefreshPlayerList();
        }

        private void InitializeForm()
        {
            TeamComboBox.ItemsSource = _dataService.GetTeams();
            PositionComboBox.ItemsSource = _dataService.GetPositions();
            NationalityComboBox.ItemsSource = _dataService.GetNationalities();
            ClearForm();
        }

        private void RefreshPlayerList()
        {
            if (PlayersListBox != null)
            {
                PlayersListBox.ItemsSource = null;
                PlayersListBox.ItemsSource = _currentPlayers ?? new List<Player>();
            }
        }

        private void ClearForm()
        {
            NameTextBox.Text = string.Empty;
            AgeTextBox.Text = string.Empty;
            PositionComboBox.Text = string.Empty;
            TeamComboBox.Text = string.Empty;
            ValueTextBox.Text = string.Empty;
            NationalityComboBox.Text = string.Empty;

            UpdateButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            SaveButton.IsEnabled = true;
            DetailsPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowPlayerDetails(Player? player)
        {
            if (player != null)
            {
                SelectedPlayerName.Text = player.Name;
                SelectedPlayerDetails.Text = $"🏷️ Név: {player.Name}\n" +
                                            $"🏟️ Csapat: {player.Team}\n" +
                                            $"🎯 Poszt: {player.Position}\n" +
                                            $"🎂 Kor: {player.Age} év\n" +
                                            $"🌍 Nemzetiség: {player.Nationality}\n" +
                                            $"💰 Piaci érték: {player.Value:C0}";
                DetailsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                DetailsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(AgeTextBox.Text) ||
                string.IsNullOrWhiteSpace(PositionComboBox.Text) ||
                string.IsNullOrWhiteSpace(TeamComboBox.Text) ||
                string.IsNullOrWhiteSpace(ValueTextBox.Text) ||
                string.IsNullOrWhiteSpace(NationalityComboBox.Text))
            {
                MessageBox.Show("Kérem töltse ki az összes kötelező mezőt!", "Hiányzó adatok",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(AgeTextBox.Text, out int age) || age < 16 || age > 50)
            {
                MessageBox.Show("Érvényes kort adjon meg (16-50)!", "Érvénytelen kor",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(ValueTextBox.Text, out decimal value) || value < 0)
            {
                MessageBox.Show("Érvényes piaci értéket adjon meg!", "Érvénytelen érték",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private Player GetPlayerFromForm()
        {
            return new Player
            {
                Id = _selectedPlayer?.Id ?? 0,
                Name = NameTextBox.Text.Trim(),
                Age = int.Parse(AgeTextBox.Text),
                Position = PositionComboBox.Text.Trim(),
                Team = TeamComboBox.Text.Trim(),
                Value = decimal.Parse(ValueTextBox.Text),
                Nationality = NationalityComboBox.Text.Trim()
            };
        }

        private void PlayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedPlayer = PlayersListBox.SelectedItem as Player;

            if (_selectedPlayer != null)
            {
                NameTextBox.Text = _selectedPlayer.Name;
                AgeTextBox.Text = _selectedPlayer.Age.ToString();
                PositionComboBox.Text = _selectedPlayer.Position;
                TeamComboBox.Text = _selectedPlayer.Team;
                ValueTextBox.Text = _selectedPlayer.Value.ToString();
                NationalityComboBox.Text = _selectedPlayer.Nationality;

                UpdateButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                SaveButton.IsEnabled = false;
                ShowPlayerDetails(_selectedPlayer);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchResults = _dataService.SearchPlayers(SearchTextBox.Text);
            PlayersListBox.ItemsSource = searchResults;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || SearchTextBox.Text == "Keresés...")
            {
                RefreshPlayerList();
            }
            else
            {
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "Keresés...";
            SearchTextBox.Foreground = Brushes.Gray;
            RefreshPlayerList();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            PlayersListBox.SelectedItem = null;
            _selectedPlayer = null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                var newPlayer = GetPlayerFromForm();
                _dataService.AddPlayer(newPlayer);
                LoadData();
                ClearForm();
                MessageBox.Show($"✅ {newPlayer.Name} sikeresen hozzáadva!", "Siker",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayer == null)
            {
                MessageBox.Show("Kérem válasszon ki egy játékost a szerkesztéshez!",
                              "Nincs kiválasztott játékos",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            if (_selectedPlayer != null && ValidateForm())
            {
                var updatedPlayer = GetPlayerFromForm();
                _dataService.UpdatePlayer(updatedPlayer);
                LoadData();
                ClearForm();
                MessageBox.Show($"{updatedPlayer.Name} adatai frissítve!",
                              "Siker",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayer == null)
            {
                MessageBox.Show("Kérem válasszon ki egy játékost a törléshez!",
                              "Nincs kiválasztott játékos",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            var playerNameToDelete = _selectedPlayer.Name;

            var result = MessageBox.Show($"Biztosan törölni szeretné a(z) {playerNameToDelete} játékost?",
                                       "⚠️ Törlés megerősítése",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _dataService.DeletePlayer(_selectedPlayer.Id);
                LoadData();
                ClearForm();

                MessageBox.Show($"{playerNameToDelete} sikeresen törölve!",
                              "Siker",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Nationality { get; set; } = string.Empty;
    }

    public class PlayerDataService
{
    private readonly string _filePath = "players.json";
    private readonly string _defaultFilePath = "defaultPlayers.json";
    private List<Player> _players;

    public PlayerDataService()
    {
        _players = new List<Player>();
        LoadPlayers();
    }

    private void LoadPlayers()
    {
        try
        {
            if (File.Exists(_defaultFilePath))
            {
                string defaultJson = File.ReadAllText(_defaultFilePath);
                var defaultPlayers = JsonSerializer.Deserialize<List<Player>>(defaultJson);
                
                if (defaultPlayers != null && defaultPlayers.Count > 0)
                {
                    _players = defaultPlayers;
                    
                    if (File.Exists(_filePath))
                    {
                        File.Delete(_filePath);
                    }
                    
                    SavePlayers();
                    return;
                }
            }
        }
        catch
        {
        }

        if (File.Exists(_filePath))
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                var players = JsonSerializer.Deserialize<List<Player>>(json);
                if (players != null)
                {
                    _players = players;
                    return;
                }
            }
            catch
            {
            }
        }

        _players = new List<Player>();
    }

    private void SavePlayers()
    {
        try
        {
            string json = JsonSerializer.Serialize(_players, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
        }
    }

    public List<Player> GetAllPlayers() => _players ?? new List<Player>();

    public List<string> GetTeams()
    {
        var teams = _players.Select(p => p.Team).Distinct().ToList();
        if (teams.Count == 0)
        {
            teams = new List<string> { "DVSC", "FTC", "Paks", "Várda", "ETO", "PAFC", "MTK", "Zalaegerszeg", "Újpest", "Diósgyőr", "Nyíregyháza", "Kazincbarcika" };
        }
        return teams;
    }

    public List<string> GetPositions()
    {
        var positions = _players.Select(p => p.Position).Distinct().ToList();
        if (positions.Count == 0)
        {
            positions = new List<string> { "Kapus", "Jobbhátvéd", "Balhátvéd", "Középhátvéd", "Védekező középpályás", "Középpályás", "Jobbszélső", "Balszélső", "Támadó középpályás", "Csatár" };
        }
        return positions;
    }

        public List<string> GetNationalities()
        {
            var nationalities = new List<string>
    {
        "Afganisztán", "Albánia", "Algéria", "Andorra", "Angola",
        "Antigua és Barbuda", "Argentína", "Örményország", "Ausztrália",
        "Ausztria", "Azerbajdzsán", "Bahamák", "Bahrein", "Banglades",
        "Barbados", "Fehéroroszország", "Belgium", "Belize", "Benin",
        "Bhután", "Bolívia", "Bosznia és Hercegovina", "Botswana",
        "Brazília", "Brunei", "Bulgária", "Burkina Faso", "Burundi",
        "Kambodzsa", "Kamerun", "Kanada", "Zöld-foki-szigetek",
        "Közép-afrikai Köztársaság", "Csád", "Chile", "Kína",
        "Kolumbia", "Comore-szigetek", "Kongói Köztársaság",
        "Kongói Demokratikus Köztársaság", "Costa Rica", "Cote d'Ivoire",
        "Horvátország", "Kuba", "Ciprus", "Cseh Köztársaság",
        "Dánia", "Dzsibuti", "Dominika", "Dominikai Köztársaság",
        "Kelet-Timor (Timor-Leste)", "Ecuador", "Egyiptom",
        "El Salvador", "Egyenlítői Guinea", "Eritrea", "Észtország",
        "Etiópia", "Fidzsi", "Finnország", "Franciaország",
        "Gabon", "Gambia", "Grúzia", "Németország", "Ghána",
        "Görögország", "Grenada", "Guatemala", "Guinea",
        "Bissau-Guinea", "Guyana", "Haiti", "Honduras", "Magyarország",
        "Izland", "India", "Indonézia", "Irán", "Irak", "Írország",
        "Izrael", "Olaszország", "Jamaica", "Japán", "Jordánia",
        "Kazahsztán", "Kenya", "Kiribati", "Észak-Korea",
        "Dél-Korea", "Koszovó", "Kuvait", "Kirgizisztán", "Laosz",
        "Lettország", "Libanon", "Lesotho", "Libéria", "Líbia",
        "Liechtenstein", "Litvánia", "Luxemburg", "Macedónia",
        "Madagaszkár", "Malawi", "Malajzia", "Maldív-szigetek",
        "Mali", "Málta", "Marshall-szigetek", "Mauritánia",
        "Mauritius", "Mexikó", "Mikronéziai Szövetségi Államok",
        "Moldova", "Monaco", "Mongólia", "Montenegró", "Marokkó",
        "Mozambik", "Mianmar (Burma)", "Namíbia", "Nauru", "Nepál",
        "Hollandia", "Új-Zéland", "Nicaragua", "Niger", "Nigéria",
        "Norvégia", "Omán", "Pakisztán", "Palau", "Panama",
        "Pápua Új-Guinea", "Paraguay", "Peru", "Fülöp-szigetek",
        "Lengyelország", "Portugália", "Katar", "Románia",
        "Oroszország", "Ruanda", "Saint Kitts és Nevis", "Santa Lucia",
        "Saint Vincent és és a Grenadine-szigetek", "Szamoa",
        "San Marino", "São Tomé és Príncipe", "Szaúd-Arábia",
        "Szenegál", "Szerbia", "Seychelle-szigetek", "Sierra Leone",
        "Szingapúr", "Szlovákia", "Szlovénia", "Salamon-szigetek",
        "Szomália", "Dél-Afrika", "Dél-Szudán", "Spanyolország",
        "Srí Lanka", "Szudán", "Suriname", "Szváziföld", "Svédország",
        "Svájc", "Szíria", "Tajvan", "Tádzsikisztán", "Tanzánia",
        "Thaiföld", "Togo", "Tonga", "Trinidad és Tobago",
        "Tunézia", "Törökország", "Türkmenisztán", "Tuvalu",
        "Uganda", "Ukrajna", "Egyesült Arab Emírségek",
        "Egyesült Királyság", "Egyesült Államok", "Uruguay",
        "Üzbegisztán", "Vanuatu", "Vatikán (Vatikánváros) (Holy See)",
        "Venezuela", "Vietnam", "Jemen", "Zambia", "Zimbabwe"
    };

            return nationalities.OrderBy(n => n).ToList();
        }

        public void AddPlayer(Player player)
    {
        player.Id = _players.Count > 0 ? _players.Max(p => p.Id) + 1 : 1;
        _players.Add(player);
        SavePlayers();
    }

    public void UpdatePlayer(Player player)
    {
        var existingPlayer = _players.FirstOrDefault(p => p.Id == player.Id);
        if (existingPlayer != null)
        {
            existingPlayer.Name = player.Name;
            existingPlayer.Age = player.Age;
            existingPlayer.Position = player.Position;
            existingPlayer.Team = player.Team;
            existingPlayer.Value = player.Value;
            existingPlayer.Nationality = player.Nationality;
            SavePlayers();
        }
    }

    public void DeletePlayer(int id)
    {
        var player = _players.FirstOrDefault(p => p.Id == id);
        if (player != null)
        {
            _players.Remove(player);
            SavePlayers();
        }
    }

    public List<Player> SearchPlayers(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm == "Keresés...")
            return _players;

        return _players.Where(p =>
            p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Team.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Position.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Nationality.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }
}
}