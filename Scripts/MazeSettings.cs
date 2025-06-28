public static class MazeSettings
{
    public enum Algorithm
    {
        Kruskal,
        Backtracking,
        Prim,
        Eller,
        Wilson,
        AldousBroder,
        HuntAndKill,
        BinaryTree,
        RecursiveDivision,
        Sidewinder
    }

    public static Algorithm SelectedAlgorithm = Algorithm.Kruskal;
}
