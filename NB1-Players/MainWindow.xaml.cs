using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NB1_Players
{
    public partial class MainWindow : Window
    {
        private PlayerDataService _dataService;
        private List<Player> _currentPlayers;
        private Player _selectedPlayer;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new PlayerDataService();
            LoadData();
            InitializeForm();
        }

        private void LoadData()
        {
            _currentPlayers = _dataService.GetAllPlayers();
            RefreshPlayerList();
        }

        private void InitializeForm()
        {
            TeamComboBox.ItemsSource = _dataService.GetTeams();
            PositionComboBox.ItemsSource = _dataService.GetPositions();
            NationalityComboBox.ItemsSource = _dataService.GetNationalities();
            ContractDatePicker.SelectedDate = DateTime.Now.AddYears(1);
            ClearForm();
        }

        private void RefreshPlayerList()
        {
            PlayersListBox.ItemsSource = null;
            PlayersListBox.ItemsSource = _currentPlayers;
        }

        private void ClearForm()
        {
            NameTextBox.Text = string.Empty;
            AgeTextBox.Text = string.Empty;
            PositionComboBox.Text = string.Empty;
            TeamComboBox.Text = string.Empty;
            ValueTextBox.Text = string.Empty;
            NationalityComboBox.Text = string.Empty;
            ContractDatePicker.SelectedDate = DateTime.Now.AddYears(1);
            NationalTeamCheckBox.IsChecked = false;

            UpdateButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            SaveButton.IsEnabled = true;
            DetailsPanel.Visibility = Visibility.Collapsed;
        }

        private void ShowPlayerDetails(Player player)
        {
            if (player != null)
            {
                SelectedPlayerName.Text = player.Name;
                SelectedPlayerDetails.Text = $"{player.Name}\nCsapat: {player.Team}\nPoszt: {player.Position}\nKor: {player.Age} év\nNemzetiség: {player.Nationality}\nPiaci érték: {player.Value:C0}\nSzerződés lejárta: {player.ContractUntil:yyyy.MM.dd.}\nVálogatott: {(player.IsNationalTeamPlayer ? "Igen" : "Nem")}";
                DetailsPanel.Visibility = Visibility.Visible;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(AgeTextBox.Text) ||
                string.IsNullOrWhiteSpace(PositionComboBox.Text) ||
                string.IsNullOrWhiteSpace(TeamComboBox.Text) ||
                string.IsNullOrWhiteSpace(ValueTextBox.Text) ||
                string.IsNullOrWhiteSpace(NationalityComboBox.Text) ||
                ContractDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Kérem töltse ki az összes kötelező mezőt!", "Hiányzó adatok", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(AgeTextBox.Text, out int age) || age < 16 || age > 50)
            {
                MessageBox.Show("Érvényes kort adjon meg (16-50)!", "Érvénytelen kor", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(ValueTextBox.Text, out decimal value) || value < 0)
            {
                MessageBox.Show("Érvényes piaci értéket adjon meg!", "Érvénytelen érték", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                Nationality = NationalityComboBox.Text.Trim(),
                ContractUntil = ContractDatePicker.SelectedDate.Value,
                IsNationalTeamPlayer = NationalTeamCheckBox.IsChecked ?? false
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
                ContractDatePicker.SelectedDate = _selectedPlayer.ContractUntil;
                NationalTeamCheckBox.IsChecked = _selectedPlayer.IsNationalTeamPlayer;

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
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                RefreshPlayerList();
            }
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
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
                MessageBox.Show("Játékos sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayer != null && ValidateForm())
            {
                var updatedPlayer = GetPlayerFromForm();
                _dataService.UpdatePlayer(updatedPlayer);
                LoadData();
                ClearForm();
                MessageBox.Show("Játékos adatai sikeresen frissítve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayer != null)
            {
                var result = MessageBox.Show($"Biztosan törölni szeretné a(z) {_selectedPlayer.Name} játékost?", "Törlés megerősítése", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _dataService.DeletePlayer(_selectedPlayer.Id);
                    LoadData();
                    ClearForm();
                    MessageBox.Show("Játékos sikeresen törölve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
        public string Name { get; set; }
        public int Age { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public decimal Value { get; set; }
        public DateTime ContractUntil { get; set; }
        public bool IsNationalTeamPlayer { get; set; }
        public string Nationality { get; set; }
    }

    public class PlayerDataService
    {
        private readonly string _filePath = "players.json";
        private List<Player> _players;

        public PlayerDataService()
        {
            LoadPlayers();
        }

        private void LoadPlayers()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    _players = JsonSerializer.Deserialize<List<Player>>(json);
                }
                catch
                {
                    _players = new List<Player>();
                }
            }
            else
            {
                _players = new List<Player>();
            }
        }

        private void SavePlayers()
        {
            string json = JsonSerializer.Serialize(_players, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public List<Player> GetAllPlayers() => _players;

        public List<string> GetTeams() => _players.Select(p => p.Team).Distinct().ToList();
        public List<string> GetPositions() => _players.Select(p => p.Position).Distinct().ToList();
        public List<string> GetNationalities() => _players.Select(p => p.Nationality).Distinct().ToList();

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
                existingPlayer.ContractUntil = player.ContractUntil;
                existingPlayer.IsNationalTeamPlayer = player.IsNationalTeamPlayer;
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
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _players;

            return _players.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Team.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Position.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }
}