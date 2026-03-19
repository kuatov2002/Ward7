/// <summary>
/// Checks whether a fact/card/event has been discovered by the player through their choices.
/// </summary>
public static class DiscoveryHelper
{
    /// <summary>
    /// Returns true if the player has made the choice that reveals this fact.
    /// alwaysVisible items are always discovered.
    /// If requiredChoiceId is empty, any choice of that type counts.
    /// </summary>
    public static bool IsDiscovered(bool alwaysVisible, ChoiceType requiredType, string requiredId,
        int week, DailyChoiceService choices)
    {
        if (alwaysVisible) return true;

        if (string.IsNullOrEmpty(requiredId))
        {
            // Any choice of this type reveals it
            return choices.IsChosen(week, requiredType);
        }

        // Specific choice required
        return choices.GetSelected(week, requiredType) == requiredId;
    }
}
