namespace OriBFArchipelago.MapTracker.Core
{
    enum LocationStatus
    {
        Unchecked = 1,
        UnsavedAndDied = 2,
        UnsavedCheck = 3,
        Checked = 4
    }
    enum GameDifficulty
    {
        Casual = 1,
        Standard = 2,
        Expert = 3,
        Master = 4,
        Glitched = 5
    }
    enum IconVisibilityEnum
    {
        None = 0,
        Original = 1,
        In_Logic = 2,
        All = 3
    }

    enum IconVisibilityLogicEnum
    {
        Game,
        Archipelago
    }

    enum MapVisibilityEnum
    {
        Not_Visible = 0,
        Visible = 1
    }

}
