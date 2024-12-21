public class CardState
{
    public CMSEntity model;
    public int rollValue;
    public InteractiveObject view;
    public bool isPlayed;
    public bool isDead;
    public int Sides => model.Get<TagSides>().sides;
    public CardId cardId;
    public bool isClaimed;
}