
namespace PokeManager.PokeManagement
{
    public class PokeModel
    {
        public string Name { get; set; }
        public List<string> Types { get; set; } = new();

        public override bool Equals(object? obj)
        {
            if (obj is not PokeModel other)
                return false;
            return Name == other.Name;
        }
    }
}
