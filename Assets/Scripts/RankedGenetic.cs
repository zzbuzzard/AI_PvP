using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There is a fixed number of points in total
// For a population of size N, there are 10N points in total.
public class RankedGenetic : Genetic
{
    class AI : IComparable<AI>
    {
        public AIPlayer player;
        public float score;

        public AI(AIPlayer player, float score)
        {
            this.player = player;
            this.score = score;
        }

        public int CompareTo(AI other)
        {
            return -score.CompareTo(other.score);
        }
    }

    List<AI> ais;
    int N;

    const float scorePerPop = 1.0f;
    const float minContribute = 0.25f;
    const float threshold = 0.4f;
    public const int FFA_size = 8;

    public RankedGenetic(List<AIPlayer> initialPopulation)
    {
        N = initialPopulation.Count;

        // Ensure N is divisible by FFA_size
        while (N%FFA_size != 0 && N>0)
        {
            N--;
        }

        ais = new List<AI>();
        for (int i=0; i<N; i++)
        {
            ais.Add(new AI(initialPopulation[i], scorePerPop));
        }
    }

    public override List<GenericPlayer> GetPopulation()
    {
        List<GenericPlayer> players = new List<GenericPlayer>();

        for (int i=0; i<N; i++)
        {
            players.Add(ais[i].player);
        }

        return players;
    }

    // 1) Do all matches in FFA groups
    // 2) Get scores for candidates and redistribute wealth + re-sort list
    // 3) Anyone who hits 0 wealth is removed from the game.
    // 4) Add new people as random breedings, taking the score from everyone else
    // 5) Repeat
    public override void Increment()
    {
        Debug.Log("YOU NOBHEAD");
        List<GenericPlayer> roundPlayers = new List<GenericPlayer>();
        List<Pair<float, int>> scores = new List<Pair<float, int>>();

        for (int j = 0; j < FFA_size; j++)
        {
            roundPlayers.Add(null);
            scores.Add(new Pair<float, int>(0.0f, 0));
        }


        for (int i=0; i<N/FFA_size; i++)
        {
            for (int j = 0; j < FFA_size; j++)
            {
                roundPlayers[j] = ais[i * FFA_size + j].player;
            }

            // TODO: Shuffle? but then issues in next loop
            Game.SimulateGame(roundPlayers);

            // Min score is the one with the largest index
            float minScore = ais[(i + 1) * FFA_size - 1].score;

            for (int j=0; j < FFA_size; j++)
            {
                scores[j].fst = Genetic.GetScore1(roundPlayers[j]);
                scores[j].snd = j;
            }
            scores.Sort();

            // scores[0] is smallest so worst
            // scores[FFA_size-1] is biggest so best

            float contribute = Math.Min(minScore, minContribute); // Everyone contributes this amount

            int totalUnits = (FFA_size * (FFA_size - 1)) / 2;
            float unitPoint = contribute * FFA_size / totalUnits;

            for (int j=0; j<FFA_size; j++)
            {
                int ind = scores[j].snd;
                ais[i * FFA_size + ind].score += unitPoint * j - contribute;
            }
        }

        ais.Sort();

        int removedCount = 0;
        float removedScore = 0;
        while (ais[ais.Count - 1].score < threshold)
        {
            removedCount++;
            removedScore += ais[ais.Count - 1].score;

            ais.RemoveAt(ais.Count - 1);
        }

        //Debug.Log("Top scorer has score " + ais[0].score + "\nRemoved " + removedCount);

        if (removedCount > 0)
        {
            float currentTotScore = N * scorePerPop - removedScore;
            float neededNewScore = removedCount * scorePerPop;

            // We need to scale currentTotScore so that it is GOAL - NEEDED
            // currentTotScore * alpha = GOAL - NEEDED

            float multiplier = (N * scorePerPop - neededNewScore) / currentTotScore;

            for (int i = 0; i < ais.Count; i++)
            {
                ais[i].score *= multiplier;
            }

            int mcount = ais.Count;
            for (int i=0; i<removedCount; i++)
            {
                // Pick two different as and bs
                int a = UnityEngine.Random.Range(0, mcount-1);
                int b = UnityEngine.Random.Range(a+1, mcount);

                // Breed and add to list with default score
                ais.Add(new AI(ais[a].player.BreedPlayer(ais[b].player), scorePerPop));
            }

            ais.Sort();
        }

    }
}
