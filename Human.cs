using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5InARowAI
{
    internal class Human : Player
    {
       

        

        public override float[] GetMoves(int[] boardints, bool UpperTurn, int[,] HumanView)
        {
            for(int j = HumanView.GetLength(1) - 1; j >= 0; j--)
            {
                
                for (int i = 0; i < HumanView.GetLength(0); i++)
                {
                    Console.Write(HumanView[i,j]);
                }
                Console.WriteLine();
            }
            Console.WriteLine("Upper Turn: " + UpperTurn.ToString());
            int x = Convert.ToInt32(Console.ReadLine());
            int y = Convert.ToInt32(Console.ReadLine());
            float[] Move = new float[Board.SizeSquared];
            Move[Board.CoordsToint(x, y)] = 1;
            return Move;
        }
    }
}
