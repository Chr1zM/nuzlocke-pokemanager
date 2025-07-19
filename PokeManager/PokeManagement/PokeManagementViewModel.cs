using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Win32;
using PokeApiNet;
using PokeManager.PokeManagement.Saving;

namespace PokeManager.PokeManagement;

public class PokeManagementViewModel : INotifyPropertyChanged
{
    private readonly PokeApiClient pokeClient_ = new();

    #region Paging

    private int pageSize_ = 1500;

    public int PageSize
    {
        get => pageSize_;
        set
        {
            if (value == pageSize_)
                return;

            pageSize_ = value;
            OnPropertyChanged();
            _ = LoadPokemonsAsync();
        }
    }

    private int currentPage_;

    public int CurrentPage
    {
        get => currentPage_;
        set
        {
            if (value == currentPage_ || value <= 0)
                return;

            currentPage_ = value;
            OnPropertyChanged();
            _ = LoadPokemonsAsync();
        }
    }

    private int totalCount_;

    public int TotalCount
    {
        get => totalCount_;
        private set
        {
            totalCount_ = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPages));
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    #endregion Paging

    #region Location

    private ObservableCollection<LocationModel> sinnohLocations_ = [];

    public ObservableCollection<LocationModel> SinnohLocations
    {
        get => sinnohLocations_;
        set
        {
            sinnohLocations_ = value;
            OnPropertyChanged();
        }
    }

    #endregion Location

    #region Players

    public Player Player1 { get; set; } = new();
    public Player Player2 { get; set; } = new();

    #endregion Players

    #region Loading

    private bool isLoading_;

    public bool IsLoading
    {
        get => isLoading_;
        private set
        {
            isLoading_ = value;
            OnPropertyChanged();
        }
    }

    private double loadingPercentage_;

    public double LoadingPercentage
    {
        get => loadingPercentage_;
        set
        {
            if (!(Math.Abs(loadingPercentage_ - value) > 0.1))
                return;

            loadingPercentage_ = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LoadingPercentageText));
        }
    }

    public string LoadingPercentageText => $"Lade Pokémons... {Math.Round(LoadingPercentage)}%";

    #endregion Loading

    private ObservableCollection<PokeModel> pokemons_ = [];

    public ObservableCollection<PokeModel> Pokemons
    {
        get => pokemons_;
        private set
        {
            pokemons_ = value;
            OnPropertyChanged();
        }
    }

    private string searchText_;

    public string SearchText
    {
        get => searchText_;
        set
        {
            if (searchText_ == value)
                return;

            searchText_ = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    private ObservableCollection<PokeModel> filteredPokemons_ = [];

    public ObservableCollection<PokeModel> FilteredPokemons
    {
        get => filteredPokemons_;
        set
        {
            filteredPokemons_ = value;
            OnPropertyChanged();
        }
    }

    private PokeModel selectedPokemon_;

    public PokeModel SelectedPokemon
    {
        get => selectedPokemon_;
        set
        {
            selectedPokemon_ = value;
            OnPropertyChanged();
        }
    }

    public ICommand NextPageCommand { get; }
    public ICommand PreviousPageCommand { get; }

    public ICommand AddToPlayer1StorageCommand { get; }
    public ICommand AddToPlayer2StorageCommand { get; }
    public ICommand MoveToTeamCommand { get; }
    public ICommand MoveToStorageCommand { get; }
    public ICommand RemoveFromListCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }

    public PokeManagementViewModel()
    {
        AddToPlayer1StorageCommand = new RelayCommand(_ => Player1.AddToStorage(SelectedPokemon), _ => SelectedPokemon != null);
        AddToPlayer2StorageCommand = new RelayCommand(_ => Player2.AddToStorage(SelectedPokemon), _ => SelectedPokemon != null);

        MoveToTeamCommand = new RelayCommand(p => MoveToTeam((PokeModel)p));
        MoveToStorageCommand = new RelayCommand(p => MoveToStorage((PokeModel)p));

        RemoveFromListCommand = new RelayCommand(p => RemoveFromAnyList((PokeModel)p));

        NextPageCommand = new RelayCommand(_ => CurrentPage++, _ => CurrentPage < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => CurrentPage--, _ => CurrentPage > 1);

        SaveCommand = new RelayCommand(_ => SaveToFile());
        LoadCommand = new RelayCommand(_ => LoadFromFile());

        CurrentPage = 1; // implicitly loads the first page of Pokémons
        _ = LoadSinnohLocationsAsync();
    }


    #region PokeManagement

    private async Task LoadPokemonsAsync()
    {
        IsLoading = true;
        LoadingPercentage = 0;

        Pokemons.Clear();
        FilteredPokemons.Clear();

        var offset = (CurrentPage - 1) * PageSize;
        var page = await pokeClient_.GetNamedResourcePageAsync<Pokemon>(PageSize, offset);
        TotalCount = page.Count;
        if (CurrentPage > TotalPages)
        {
            // TODO: workaround => don't call LoadPokemonsAsync again
            CurrentPage = 1;
            return;
        }

        var pokemons = new List<PokeModel>();

        for (var i = 0; i < page.Results.Count; i++)
        {
            var res = page.Results[i];
            var pokemon = await pokeClient_.GetResourceAsync(res);

            Debug.WriteLine($"Pokemon {i + 1}/{page.Results.Count}");
            LoadingPercentage = (i + 1) * 100.0 / page.Results.Count;

            pokemons.Add(new PokeModel
            {
                Name = pokemon.Name,
                Types = pokemon.Types.Select(t => t.Type.Name).ToList()
            });
        }

        Pokemons = new ObservableCollection<PokeModel>(pokemons.OrderBy(n => n.Name));
        ApplyFilter();

        IsLoading = false;
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredPokemons = new ObservableCollection<PokeModel>(Pokemons);
        }
        else
        {
            var lowerSearch = SearchText.ToLower();

            var filtered = Pokemons.Where(p =>
                p.Name.ToLower().Contains(lowerSearch) ||
                p.Types.Any(t => t.ToLower().Contains(lowerSearch))
            ).ToList();

            FilteredPokemons = new ObservableCollection<PokeModel>(filtered);
        }
    }

    private void MoveToTeam(PokeModel pokemon)
    {
        if (Player1.Storage.Contains(pokemon) && Player1.CanAddToTeam(pokemon, Player2.Team))
            Player1.MoveToTeam(pokemon, Player2.Team);
        else if (Player2.Storage.Contains(pokemon) && Player2.CanAddToTeam(pokemon, Player1.Team))
            Player2.MoveToTeam(pokemon, Player1.Team);
    }

    private void MoveToStorage(PokeModel pokemon)
    {
        Player1.MoveToStorage(pokemon);
        Player2.MoveToStorage(pokemon);
    }

    private void RemoveFromAnyList(PokeModel poke)
    {
        Player1.RemovePokemon(poke);
        Player2.RemovePokemon(poke);
    }

    #endregion Pokemanagement

    #region Saving

    public void SaveToFile()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = "json",
            FileName = "nuzlocke.json"
        };

        if (dialog.ShowDialog() != true)
            return;

        var save = new SaveData
        {
            Player1 = Player1,
            Player2 = Player2,
            Locations = SinnohLocations.ToList()
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(save, options));
    }

    public void LoadFromFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() != true || !File.Exists(dialog.FileName))
            return;

        var json = File.ReadAllText(dialog.FileName);
        var save = JsonSerializer.Deserialize<SaveData>(json);

        if (save is null)
            return;

        Player1.Team.Clear();
        Player1.Storage.Clear();
        foreach (var poke in save.Player1.Team)
            Player1.Team.Add(poke);
        foreach (var poke in save.Player1.Storage)
            Player1.Storage.Add(poke);

        Player2.Team.Clear();
        Player2.Storage.Clear();
        foreach (var poke in save.Player2.Team)
            Player2.Team.Add(poke);
        foreach (var poke in save.Player2.Storage)
            Player2.Storage.Add(poke);

        SinnohLocations = new ObservableCollection<LocationModel>(save.Locations);
    }

    #endregion Saving

    #region LocationManagement

    private async Task LoadSinnohLocationsAsync()
    {
        try
        {
            var region = await pokeClient_.GetResourceAsync<Region>("sinnoh");

            var locations = new List<LocationModel>();

            foreach (var res in region.Locations)
            {
                var location = await pokeClient_.GetResourceAsync(res);

                locations.Add(new LocationModel
                {
                    Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(location.Name.Replace("-", " ")),
                    IsVisited = false
                });
            }

            SinnohLocations = new ObservableCollection<LocationModel>(locations.OrderBy(l => l.Name));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Fehler beim Laden der Sinnoh-Orte: {ex.Message}");
        }
    }

    #endregion LocationManagement

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}