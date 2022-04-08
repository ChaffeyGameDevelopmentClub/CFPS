public interface IAlliable {
    enum Alliance {
        Neutral, //Attack nothing
        Enemy, //Attack player
        Player, //Attack enemies
        None //Attack everything
    }

    public void setAlliance(Alliance alliance);
}