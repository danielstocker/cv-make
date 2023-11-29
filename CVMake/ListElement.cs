
namespace CVMake;

public class ListElement
{
    private string content = "";
    private int rank = 0;

    public ListElement(string content, int rank)
    {
        this.content = content;
        this.rank = rank;
    }
    internal int GetRank()
    {
        return this.rank;
    }
    internal string GetContent()
    {
        return this.content;
    }

    internal void SetRank(int newRank)
    {
        this.rank = newRank;
    }
}
