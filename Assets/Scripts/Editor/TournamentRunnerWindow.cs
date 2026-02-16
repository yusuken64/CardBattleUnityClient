using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TournamentRunnerWindow : EditorWindow
{
    private TournamentRunner runner;
    private bool isRunning = false; // tracks if a fight is in progress

    private TournamentStats stats = new TournamentStats();
    private int fightsToRun = 100;
    private int fightsPerBatch = 5;
    private Vector2 matrixScroll;
    private bool cancelRequested = false;
    private string savePath = "TournamentCheckpoint.json";

    [MenuItem("Game/Tournament Runner")]
    public static void ShowWindow()
    {
        GetWindow<TournamentRunnerWindow>("Tournament Runner");
    }

    private void OnGUI()
    {
        minSize = new Vector2(600, 300);

        runner = (TournamentRunner)EditorGUILayout.ObjectField("Runner", runner, typeof(TournamentRunner), true) as TournamentRunner;

        EditorGUI.BeginDisabledGroup(isRunning); // disable while running
        if (runner != null && GUILayout.Button(isRunning ? "Running..." : "Run Fight Async"))
        {
            RunFightAsync();
        }

        EditorGUI.EndDisabledGroup(); EditorGUILayout.Space();
        fightsToRun = EditorGUILayout.IntField("Fights To Run", fightsToRun);
        fightsPerBatch = EditorGUILayout.IntField("Fights Per Batch", fightsPerBatch);

        EditorGUI.BeginDisabledGroup(isRunning);
        if (runner != null && GUILayout.Button("Run Tournament Async"))
        {
            RunTournamentAsync();
        }
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("Reset Stats"))
        {
            stats = new TournamentStats();
        }
        EditorGUILayout.Space();
        GUILayout.Label("Checkpoint File", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField(savePath);

        if (GUILayout.Button("Choose...", GUILayout.Width(90)))
        {
            string chosen = EditorUtility.SaveFilePanel(
                "Choose Tournament File",
                "",
                "TournamentMatrix.csv",
                "csv");

            if (!string.IsNullOrEmpty(chosen))
                savePath = chosen;
        }

        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Export Matrix To CSV"))
        {
            ExportCsv(null);
        }
        if (isRunning && GUILayout.Button("Cancel Tournament"))
        {
            cancelRequested = true;
        }
        DrawMatchupMatrix();
    }

    private async void RunTournamentAsync()
    {
        isRunning = true;
        Repaint();

        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < fightsToRun; i++)
        {
            if (cancelRequested) { break; }
            runner.RunFightAndReturnWinner(out string deckA, out string deckB, out string winner);
            stats.RecordMatch(deckA, deckB, winner);

            if (i % fightsPerBatch == 0)
            {
                EditorUtility.DisplayProgressBar(
                    "Running Tournament",
                    $"Fight {i + 1} / {fightsToRun}",
                    (float)(i + 1) / fightsToRun);

                Repaint();
                ExportCsv(savePath);
                await Task.Yield();
            }
        }

        EditorUtility.ClearProgressBar();

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Tournament finished in {stopwatch.Elapsed.TotalSeconds:F2}s");

        isRunning = false;
        Repaint();
    }

    private void RunFightAsync()
    {
        isRunning = true;

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Run the original fight
        runner.RunFight();

        stopwatch.Stop();
        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
        UnityEngine.Debug.Log($"Fight finished in {elapsedSeconds:F2} seconds");
        isRunning = false;
        Repaint();
    }

    private void DrawMatchupMatrix()
    {
        var data = stats.GetData();
        if (data.Count == 0)
            return;

        var decks = data.Keys.OrderBy(x => x).ToList();

        const float cellWidth = 80;
        const float rowHeaderWidth = 140;
        const float cellHeight = 35;

        GUILayout.Space(10);
        GUILayout.Label("Matchup Matrix", EditorStyles.boldLabel);

        // Begin scroll view
        matrixScroll = EditorGUILayout.BeginScrollView(matrixScroll);

        // Header row
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(rowHeaderWidth); // empty corner cell

        foreach (var deck in decks)
            GUILayout.Label(deck, GUILayout.Width(cellWidth));

        EditorGUILayout.EndHorizontal();

        // Matrix rows
        foreach (var rowDeck in decks)
        {
            EditorGUILayout.BeginHorizontal();

            // Row header
            GUILayout.Label(rowDeck, GUILayout.Width(rowHeaderWidth));

            foreach (var colDeck in decks)
            {
                if (data.TryGetValue(rowDeck, out var row) &&
                    row.TryGetValue(colDeck, out var record))
                {
                    string text = $"{record.Wins}-{record.Loss}";
                    GUILayout.Label(text, GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                }
                else
                {
                    GUILayout.Label("-", GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
    private void ExportCsv(string path)
    {
        var data = stats.GetData();
        if (data.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No tournament data to export.");
            return;
        }

        var totalWins = GetTotalWins(data);

        var decks = data.Keys
            .OrderByDescending(deck => totalWins[deck]) // strongest first
            .ToList();

        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.SaveFilePanel(
                "Save Matchup Matrix",
                "",
                "MatchupMatrix.csv",
                "csv");
        }

        if (string.IsNullOrEmpty(path))
            return;

        StringBuilder csv = new StringBuilder();

        // Header row
        csv.Append("Deck");
        foreach (var deck in decks)
            csv.Append($",{deck}");
        csv.AppendLine();

        // Rows
        foreach (var rowDeck in decks)
        {
            csv.Append(rowDeck);

            foreach (var colDeck in decks)
            {
                if (data.TryGetValue(rowDeck, out var row) &&
                    row.TryGetValue(colDeck, out var record))
                {
                    // Format: WinsA-WinsB (Draws)
                    csv.Append($",\"{record.Wins}\"");
                }
                else
                {
                    csv.Append(","); // empty cell
                }
            }

            csv.AppendLine();
        }

        File.WriteAllText(path, csv.ToString());

        UnityEngine.Debug.Log($"CSV exported to: {path}");
        EditorUtility.RevealInFinder(path);
    }

    private Dictionary<string, int> GetTotalWins(
        Dictionary<string, Dictionary<string, TournamentStats.MatchupRecord>> data)
    {
        var totals = new Dictionary<string, int>();

        foreach (var deck in data.Keys)
        {
            int wins = 0;

            foreach (var opponent in data[deck].Values)
                wins += opponent.Wins;

            totals[deck] = wins;
        }

        return totals;
    }
}
