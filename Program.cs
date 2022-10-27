using System.Diagnostics;
namespace _5InARowAI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Debug.Assert(GenerationSize % 2 == 0);
            Debug.Assert(GenerationSize % Divisions == 0);
            //new Board().PlayMatch(new AI("TopAIGeneration11000.AI"), new Human());
            Train();
        }

        const int GenerationSize = 1000;
        const float DrawProprotion = 0.2f;
        const float WinBaseChange = 50;//Maximum Change of elo, min is half this
        const float WinRatingCoefficient = 1.01f; //higher coefficient makes a lower difference in elo get closer to the maximum elo change
        const float StartingFitness = 1000f;
        const int InitialRoundGames = 10;
        const int Divisions = 20;
        const int DivisionGames = 10;
        const uint GenerationCap = uint.MaxValue;
        static void Train()
        {
            AIData[] AIs = new AIData[GenerationSize];
            for(int i = 0;i<GenerationSize;i++)
            {
                AIs[i] = new AIData();
                AIs[i].AI = new AI();
                AIs[i].fitness = StartingFitness;
            }


            for(int i = 0; i < GenerationCap; i++)
            {
                DateTime start = DateTime.Now;
                Play(AIs);
                AIs = Evolve(AIs);
                Console.WriteLine("Generation {0} Complete - Took {1} MilliSeconds", i + 1, (DateTime.Now - start).TotalMilliseconds);
                if(i%100==0)
                {
                    AIs[0].AI.Write("TopAIGeneration" + i + ".AI");
                }
                if(i%25==0)
                {
                    int total = 0;
                    for (int j = 0; j < 100; j++)
                    {
                        AI a = new AI();
                        MatchResult MR = new Board().PlayMatch(AIs[j / 10].AI, a);
                        total += (int)MR;
                        MatchResult MR2 = new Board().PlayMatch(a, AIs[j / 10].AI);
                        total += (2 - (int)MR2);

                        
                    }
                    Console.WriteLine("Winrate Against Random AIs = {0}%", ((double)total) / 4);
                }
            }

        }
        readonly static Random random = new Random();
        static void Play(AIData[] AIs)
        {
            foreach(AIData dat in AIs)
            {
                dat.fitness = StartingFitness;
            }
            //Randomly play k games
            //Split into smaller leagues based on fitness
            //Repeat
            for (int j = 0; j < InitialRoundGames; j++)
            {
                Queue<(AIData, AIData)> Schedule = new Queue<(AIData, AIData)>();
            
                int[] indexes = Enumerable.Range(0, GenerationSize).ToArray().OrderBy(x => random.Next()).ToArray();
                for (int i = 0; i < GenerationSize; i += 2)
                {
                    Schedule.Enqueue((AIs[indexes[i]], AIs[indexes[i + 1]]));
                }
                RunSchedule(Schedule);
            }
            Array.Sort(AIs, AIComparer);//Best AI is AIs[0]
            int AIsPerDivision = GenerationSize / Divisions;
            for(int i = 0; i < DivisionGames; i++)
            {
                Queue<(AIData, AIData)> Schedule = new Queue<(AIData, AIData)>();
                for (int LowestIndex = 0; LowestIndex < Divisions; LowestIndex+= AIsPerDivision)
                {
                    //int Lowestindex = k * AIsPerDivision;

                    int[] indexes = Enumerable.Range(LowestIndex, AIsPerDivision).ToArray().OrderBy(x => random.Next()).ToArray();
                    for (int j = 0; j < indexes.Length; j += 2)
                    {
                        Schedule.Enqueue((AIs[indexes[j]], AIs[indexes[j + 1]]));
                    }
                }
                RunSchedule(Schedule);
                Array.Sort(AIs, AIComparer);
            }
            
        }
        public const int  Combine = 600;
        public const int CombinePoolSize = 5;
        //public const float Mutate = 100f;
        public const int Keep = 100;
        public const int New = 300;
        static AIData[] Evolve(AIData[] AIs)
        {
            int nextindex;

            AIData[] NewGen = new AIData[GenerationSize];
            for(nextindex = 0; nextindex < New; nextindex++)
            {
                AIData dat = new AIData();
                dat.AI = new AI();
               
                NewGen[nextindex] = dat;
                
            }
            for(int i = 0; i < Keep; i++)
            {
                NewGen[nextindex] = AIs[i];
                nextindex++;
            }
            //Get Total Fitness
            //Generate Hashset of numbers between that and total fitness
            //go through each AI 
            //Remove all numbers between totalprevfitness and totalprevfitness + this AIs fitness
            //for each number removed add AI to breeding Q
            //Randomise order of breeding Q;
            //Breed;
            float totalfitness = 0;
            for(int i = 0; i < CombinePoolSize; i++)
            {
                totalfitness += AIs[i].fitness;
            }
            float[] RandomNums = new float[Combine*2];
            for(int i = 0; i < Combine*2; i++)
            {
                RandomNums[i] = (float)random.NextDouble() * totalfitness;
            }
            Array.Sort(RandomNums);
            int CurrentStartIndex = 0;
            float prevtotalfitness = 0;
            int NextBreedIndex = 0;
            int[] BreedingIndexs = new int[Combine*2];
            for(int i = 0; i <CombinePoolSize; i++)
            {
                prevtotalfitness += AIs[i].fitness;
                while (CurrentStartIndex < RandomNums.Length && RandomNums[CurrentStartIndex] < prevtotalfitness)
                {
                    CurrentStartIndex++;
                    BreedingIndexs[NextBreedIndex] = i;
                    NextBreedIndex++;
                }
            }
            Debug.Assert(NextBreedIndex == BreedingIndexs.Length);
            BreedingIndexs = BreedingIndexs.OrderBy(x => random.Next()).ToArray();
            for (int i = 0; i < Combine*2; i+=2)
            {
                AIData dat = new AIData();
                dat.AI = new AI(AIs[BreedingIndexs[i]].AI, AIs[BreedingIndexs[i + 1]].AI);
                NewGen[nextindex] = dat;
                nextindex++;
            }
            Debug.Assert(nextindex == NewGen.Length);
            return NewGen;
        }

        public static void  PlayMatch(AIData P1, AIData P2)
        {
            //Console.WriteLine("Playing Match {0} vs {1}", P1.AI.ID, P2.AI.ID);
            Board b = new Board();
            MatchResult MR = b.PlayMatch(P1.AI, P2.AI);
            b = new Board();
            MatchResult MR2 = b.PlayMatch(P2.AI, P1.AI);
            float P1Score = ((int)MR + (int)MR2);//1 point for draw 2 for win
            //Console.WriteLine("Finished Match {0} vs {1}, Result: {2}", P1.AI.ID, P2.AI.ID, MR.ToString());
            if (P1Score == 2)
            {
                float DrawChange = (P1.fitness - P2.fitness)*DrawProprotion;
                P1.fitness -= DrawChange;
                P2.fitness += DrawChange;
                return;
            }
            float WinChange = WinBaseChange / (1 + MathF.Pow(WinRatingCoefficient,(MathF.Abs(P1.fitness - P2.fitness))));
            WinChange = WinChange * (P1Score-2);
            P1.fitness += WinChange;
            P2.fitness -= WinChange;
        }

        static public void RunSchedule(Queue<(AIData, AIData)> Schedule)
        {
            Parallel.ForEach(Schedule, (a, b) => PlayMatch(a.Item1, a.Item2));
        }

        public class AIData
        {
            public AI AI;
            public float fitness;
            
        }
        private static AIDataSorter AIComparer = new AIDataSorter();
        public class AIDataSorter : IComparer<AIData>
        {
            int IComparer<AIData>.Compare(AIData a, AIData b)
            {
                if(a.fitness == b.fitness)
                {
                    return 0;
                }
                return a.fitness < b.fitness ? 1 : -1;
            }
        }
    }
}