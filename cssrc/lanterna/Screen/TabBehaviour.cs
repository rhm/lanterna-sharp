namespace Lanterna.Screen;

public enum TabBehaviour
{
    Ignore,
    ConvertToSpace,
    AlignToColumn4,
    AlignToColumn8
}

public static class TabBehaviourExtensions
{
    public static string GetTabReplacement(this TabBehaviour behaviour, int currentColumn)
    {
        return behaviour switch
        {
            TabBehaviour.Ignore => "",
            TabBehaviour.ConvertToSpace => " ",
            TabBehaviour.AlignToColumn4 => new string(' ', 4 - (currentColumn % 4)),
            TabBehaviour.AlignToColumn8 => new string(' ', 8 - (currentColumn % 8)),
            _ => throw new ArgumentOutOfRangeException(nameof(behaviour))
        };
    }
}