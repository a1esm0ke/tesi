using System.Collections.Generic;

public static class GameData
{
    // Lista globale di competitori
    public static List<string> Competitors = new List<string>();

    // Metodo per aggiungere un competitorio
    public static void AddCompetitor(string competitorId)
    {
        if (!Competitors.Contains(competitorId))
        {
            Competitors.Add(competitorId);
        }
    }
}
