namespace GladeInvade.Shared;

public interface IScoreBoard
{
    void ClearDisplay();
    void SetDisplay(string characters);
}

public class NullScoreBoard : IScoreBoard
{
    public void ClearDisplay()
    {
    }

    public void SetDisplay(string characters)
    {
    }
}