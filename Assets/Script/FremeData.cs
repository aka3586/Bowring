[System.Serializable]
public class FrameData
{
    public int firstThrow = -1;
    public int secondThrow = -1;
    public int thirdThrow = -1;   // ★追加
    public int frameScore;
    public bool isStrike;
    public bool isSpare;
    public bool isScoreFixed = false;
}