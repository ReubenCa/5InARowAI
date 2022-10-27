using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace _5InARowAI
{
    internal class AI : Player
    {
        private static int nextID;
        private static int NextID 
            {
            get
            {
                nextID++;
                return nextID;
            }
            }
    
        public int ID { get; private set; }
        public AI()
        {
            ID = NextID;
            RandomiseArray(Weights);
            RandomiseArray(FirstLayerWeights);
            RandomiseArray(UpperTurnWeights);
            RandomiseArray(Biases);
        }

        public void Write(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                WriteArray(sw, Weights);
                WriteArray(sw, FirstLayerWeights);
                WriteArray(sw, UpperTurnWeights);
                WriteArray(sw, Biases);
                sw.Close();
            }
        }

        public AI(string path)
        {
            ID = NextID;
            using (StreamReader sr = new StreamReader(path))
            {
                Weights = Read3Darray(Weights.GetLength(0), Weights.GetLength(1), Weights.GetLength(2), sr);
                FirstLayerWeights = Read2Darray(FirstLayerWeights.GetLength(0), FirstLayerWeights.GetLength(1), sr);
                UpperTurnWeights = Read1Darray(UpperTurnWeights.GetLength(0), sr);
                Biases = Read2Darray(Biases.GetLength(0), Biases.GetLength(1), sr);
                sr.Close();
            }
        }


        public void WriteArray(StreamWriter sw, System.Array Arr)
        {
            foreach(var item in Arr)
            {
                //decimal num = Convert.ToDecimal(item);
                string str = ((float)item).ToString("N64");
                sw.WriteLine(str);
            }
            
        }
        public float[] Read1Darray(int Length, StreamReader sr)
        {
            float[] arr = new float[Length];
            for(int i =0; i < Length; i++)
            {
                arr[i] = (float)Convert.ToDecimal(sr.ReadLine());
            }
            return arr;
        }

        public float[,] Read2Darray(int Length1,int Length2, StreamReader sr)
        {
            float[,] arr = new float[Length1, Length2];
            for (int i = 0; i < Length1; i++)
            {
                for (int j = 0; j < Length2; j++)
                {
                    string s = sr.ReadLine();
                    decimal num = Convert.ToDecimal(s);
                    float num2 = (float)num;
                    arr[i,j] = num2;
                }
            }
            return arr;
        }

        public float[,,] Read3Darray(int Length1, int Length2, int Length3, StreamReader sr)
        {
            float[,,] arr = new float[Length1, Length2, Length3];
            for (int i = 0; i < Length1; i++)
            {
                for (int j = 0; j < Length2; j++)
                {
                    for (int k = 0; k < Length2; k++)
                    {
                        string s = sr.ReadLine();
                        decimal num = Convert.ToDecimal(s);
                        float num2 = (float)num;
                        arr[i, j,k] = num2;
                    }
                }
            }
            return arr;
        }

        public AI(AI AI1, AI AI2)
        {
            Weights = CombineArrays(AI1.Weights, AI2.Weights);
            FirstLayerWeights = CombineArrays(AI1.FirstLayerWeights, AI2.FirstLayerWeights);
            UpperTurnWeights = (CombineArrays(AI1.UpperTurnWeights, AI2.UpperTurnWeights));
            Biases = CombineArrays(AI1.Biases, AI2.Biases);
        }


        private float[,,] CombineArrays(float[,,] A, float[,,] B)
        {
            float[,,] C = new float[A.GetLength(0),A.GetLength(1),A.GetLength(2)];
            for (int a = 0; a < A.GetLength(0); a++)
            {
                for (int b = 0; b < A.GetLength(1); b++)
                {
                    for (int c = 0; c < A.GetLength(2); c++)
                    {
                        C[a, b, c] = (A[a, b, c] + B[a, b, c])/2 + Distribution(0, 0.2f);
                    }
                }
            }
            return C;
        }
        private float[,] CombineArrays(float[,] A, float[,] B)
        {
            float[,] C = new float[A.GetLength(0), A.GetLength(1)];
            for (int a = 0; a < A.GetLength(0); a++)
            {
                for (int b = 0; b < A.GetLength(1); b++)
                {
                    C[a, b] = (A[a, b] + B[a, b]) / 2 + Distribution(0, 0.2f);
                }
            }
            return C;
        }
        private float[] CombineArrays(float[] A, float[] B)
        {
            float[] C = new float[A.GetLength(0)];
            for (int a = 0; a< A.Length; a++)
            {
                C[a] = (A[a] + B[a]) / 2 + Distribution(0, 0.2f);
            }
            return C;
        }




        private void RandomiseArray(float[,,] A)
        {
            for (int a = 0; a < A.GetLength(0); a++)
            {
                for (int b = 0; b < A.GetLength(1); b++)
                {
                    for (int c = 0; c < A.GetLength(2); c++)
                    {
                        A[a, b, c] = Distribution(Settings.DisMean, Settings.DisStdDev);
                    }
                }
            }
        }
        private void RandomiseArray(float[,] A)
        {
            for (int a = 0; a < A.GetLength(0); a++)
            {
                for (int b = 0; b < A.GetLength(1); b++)
                {    
                        A[a, b] = Distribution(Settings.DisMean, Settings.DisStdDev);
                }
            }
        }
        private void RandomiseArray(float[] A)
        {
            for(int i = 0; i < A.Length; i++)
            {
                A[i] = Distribution(Settings.DisMean, Settings.DisStdDev);
            }
        }

        const int BoardSizeSquared = Settings.BoardSize * Settings.BoardSize;
        /// <summary>
        /// [A,B,C] A - which Layer, B- Which node C - weight from Which Node in previous Layer
        /// </summary>
        float[,,] Weights = new float[Settings.LayerCount,Settings.BoardSize*Settings.BoardSize, Settings.BoardSize * Settings.BoardSize];

        float[,] FirstLayerWeights = new float[Settings.BoardSize * Settings.BoardSize, 5 * Settings.BoardSize * Settings.BoardSize];
        
        //Weight of connection from each node in first layer to the input node with upperTurn
        float[] UpperTurnWeights = new float[BoardSizeSquared];
        /// <summary>
        /// [A,B] A - Layer, B - Node
        /// </summary>
        float[,] Biases = new float[Settings.LayerCount+1, Board.SizeSquared];
        public override float[] GetMoves(int[] Board, bool UpperTurn, int[,] HumanView)
        {
           
            float[] CurrentLayerValues = new float[BoardSizeSquared];
            
            for(int i = 0; i < BoardSizeSquared; i++)
            {
                float total = 0;
                int Length = Board.Length;
                for(int j = 0; j < Length; j++)
                {
                    total += FirstLayerWeights[i, j] * Board[j];
                }
                if (UpperTurn)
                {
                    total += UpperTurnWeights[i];
                }
               CurrentLayerValues[i] = ActivationFunction(total + Biases[0, i]);
            }

            for(int Layer = 0; Layer < Settings.LayerCount; Layer++)
            {
                float[] PreviousLayerValues = CurrentLayerValues;
                CurrentLayerValues = new float[BoardSizeSquared];
                for(int Node = 0; Node < BoardSizeSquared; Node++)
                {
                    float total = 0;
                    for(int PrevNode = 0; PrevNode < BoardSizeSquared; PrevNode++)
                    {
                        total += Weights[Layer, Node, PrevNode] * PreviousLayerValues[PrevNode];
                    }
                    CurrentLayerValues[Node] = ActivationFunction(total + Biases[Layer + 1, Node]) ;
                }
            }
            return CurrentLayerValues;


        }
        static float ActivationFunction(float a)
        {
            return (float)Math.Tanh(a);
        }

        static Random rand = new Random(); //reuse this if you are generating many
        static float Distribution(float mean, float stdDev)
        {
           
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *  Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return (float)randNormal;
        }
    }
}
