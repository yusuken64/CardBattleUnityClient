using System.Collections.Generic;

public class TournamentStats
{
    // deckA -> deckB -> results
    private Dictionary<string, Dictionary<string, MatchupRecord>> matchups = new();

    public class MatchupRecord
    {
        public int Wins;
        public int Loss;
        public int Draws;
    }

    public void RecordMatch(string deckA, string deckB, string winner)
    {
        EnsureMatchup(deckA, deckB);
        var recordA = matchups[deckA][deckB];
        var recordB = matchups[deckB][deckA];

        if (winner == deckA)
        {
            recordA.Wins++;
            recordB.Loss++;
        }
        else if (winner == deckB)
        {
            recordA.Loss++;
            recordB.Wins++;
        }
        else
        {
            recordA.Draws++;
            recordB.Draws++;
        }
    }

    private void EnsureMatchup(string a, string b)
    {
        if (!matchups.ContainsKey(a))
            matchups[a] = new Dictionary<string, MatchupRecord>();

        if (!matchups[a].ContainsKey(b))
            matchups[a][b] = new MatchupRecord();

        // mirror entry
        if (!matchups.ContainsKey(b))
            matchups[b] = new Dictionary<string, MatchupRecord>();

        if (!matchups[b].ContainsKey(a))
            matchups[b][a] = new MatchupRecord();
    }

    public Dictionary<string, Dictionary<string, MatchupRecord>> GetData()
        => matchups;
}
