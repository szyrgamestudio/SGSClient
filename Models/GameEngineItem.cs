namespace SGSClient.Models
{
    public class GameEngineItem
    {
        public int Id
        {
            get; set;
        }
        public KeyValuePair<int, string> Pair
        {
            get; set;
        }

        public GameEngineItem(int id, KeyValuePair<int, string> pair)
        {
            Id = id;
            Pair = pair;
        }

        public override string ToString()
        {
            return Pair.Value; // Zwraca nazwę jako wartość do wyświetlenia w ComboBox
        }

    }
}
