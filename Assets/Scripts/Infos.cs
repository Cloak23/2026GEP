using System;

public enum RoomType
{
    Empty,
    Mine,
    Number,
    Item
}

public class SlotData
{
    int x, y;

    public RoomType type;
    public int mine_num;
    public int amount;
    public bool isVisible;

    public SlotData(int x = -1, int y = -1, RoomType type = RoomType.Empty, int mine_num = 0, int amount = 5, bool isVisible = false)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.mine_num = mine_num;
        this.amount = amount;
        this.isVisible = isVisible;
    }
}