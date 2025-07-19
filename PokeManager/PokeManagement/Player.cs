using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PokeManager.PokeManagement;

public class Player : INotifyPropertyChanged
{
    [JsonInclude]
    public ObservableCollection<PokeModel> Team { get; private set; } = [];
    [JsonInclude]
    public ObservableCollection<PokeModel> Storage { get; private set; } = [];

    public event PropertyChangedEventHandler PropertyChanged;

    public bool CanAddToTeam(PokeModel? pokemon, IEnumerable<PokeModel?> otherTeam)
    {
        if (Team.Count >= 6 || pokemon is null)
            return false;

        var usedTypes = Team.Concat(otherTeam)
            .Select(p => p.Types[0])
            .Distinct();

        return !usedTypes.Contains(pokemon.Types[0]);
    }

    public void AddToStorage(PokeModel pokemon)
    {
        if (Storage.Contains(pokemon) || Team.Contains(pokemon)) 
            return;

        Storage.Add(pokemon);
        OnPropertyChanged(nameof(Storage));
    }

    public void MoveToTeam(PokeModel pokemon, IEnumerable<PokeModel?> otherTeam)
    {
        if (!CanAddToTeam(pokemon, otherTeam) || !Storage.Remove(pokemon)) 
            return;

        Team.Add(pokemon);
        OnPropertyChanged(nameof(Team));
        OnPropertyChanged(nameof(Storage));
    }

    public void MoveToStorage(PokeModel pokemon)
    {
        if (!Team.Remove(pokemon))
            return;

        Storage.Add(pokemon);
        OnPropertyChanged(nameof(Team));
        OnPropertyChanged(nameof(Storage));
    }

    public void RemovePokemon(PokeModel pokemon)
    {
        if (!Team.Remove(pokemon) && !Storage.Remove(pokemon)) 
            return;

        OnPropertyChanged(nameof(Team));
        OnPropertyChanged(nameof(Storage));
    }

    protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
