using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace _5InARowAI
{
    enum MatchResult
    {
        Player1Wins = 2,
        Player2Wins = 0,
        Draw = 1
    }
    internal class Board
        
    {

        int MovesMade = 0;
        public const int SizeSquared = Settings.BoardSize * Settings.BoardSize;
        const int Size = Settings.BoardSize;
        public const int Empty = 0;
        public const int P1Lower = 1;
        public const int P2Lower = 2;
        public const int P1Upper = 3;
        public const int P2Upper = 4;

        public bool[] MoveLegal = new bool[Size * Size];
        public Board()
        {
            //P1View[CoordsToint(Settings.StartMove, Settings.StartMove)  + BoardSizeSquared * 1 ] = 1;
            //P2View[CoordsToint(Settings.StartMove, Settings.StartMove) + BoardSizeSquared * 2] = 1;
            //HumanView[Settings.StartMove, Settings.StartMove] = P1Lower;
        }

        public void MoveMade(int Move, int Type)
        {
            MovesMade++;
            
            int P1index = Move + SizeSquared * Type;
            int P2Type = Type % 2 == 0 ? Type - 1 : Type + 1;
            int P2index = Move + SizeSquared * P2Type;
            Debug.Assert(P1View[P1index] == Empty && P2View[P2index] == Empty);
            P1View[P1index] = Move;
            P2View[P2index] = P2index;
            intToCoords(Move, out int x, out int y);
            HumanView[x, y] = Type;
            List<(int, int)> Neighbors = new List<(int, int)> { (x + 1, y) ,(x-1,y),(x,y-1),(x,y+1)};
            foreach((int,int) position in Neighbors)
            {
                if (SafeGetVal(position.Item1, position.Item2) ==Empty)
                {
                    MoveLegal[CoordsToint(position.Item1, position.Item2)] = true;
                }
            }
            MoveLegal[Move] = false;
        }
        public MatchResult PlayMatch(Player P1, Player P2)
        {
            
            MoveLegal[Settings.StartMove] = true;
            bool P1Turn = true;
            bool LowerTurn = true;
           
            while (true)
            {
                float[] MoveWeights = P1Turn ?  P1.GetMoves(P1View, !LowerTurn, HumanView) : P2.GetMoves(P2View, !LowerTurn, HumanView) ;
                int[] indexes = Enumerable.Range(0, MoveWeights.Length).ToArray();
                
                Array.Sort(MoveWeights, indexes, s_Comparer);
                int TriedCount = 0;
                int Move;
                do
                {
                    Move = indexes[TriedCount];
                    TriedCount++;
                    if (TriedCount == indexes.Length)
                    {
                        return MatchResult.Draw;
                    }
                }
                while (!MoveLegal[Move] && Settings.EnforceLegalMoves);

                int MoveType = P1Turn ? (LowerTurn ? P1Lower : P1Upper) : (LowerTurn ? P2Lower: P2Upper);
                intToCoords(Move, out int x, out int y);
                if(WinningMove(x,y,MoveType))
                {
                    return P1Turn ? MatchResult.Player1Wins : MatchResult.Player2Wins;
                }
                MoveMade(Move, MoveType);
               

                P1Turn = !P1Turn;
                if(P1Turn)
                {
                    LowerTurn = !LowerTurn;
                }
            }
        }

        public bool WinningMove(int MoveX, int MoveY, int MoveType)
        {
            if(MovesMade < 4*(Settings.AMIRTW-1))
            {
                return false;
            }
            Debug.Assert(MoveType != Empty);
            int AmountinRow = 1;
            //Left Right
            for(int CurrentX = MoveX+1; SafeGetVal(CurrentX, MoveY) ==MoveType;CurrentX++)
            {
                AmountinRow++;
            }
            for (int CurrentX = MoveX - 1; SafeGetVal(CurrentX, MoveY) == MoveType; CurrentX--)
            {
                AmountinRow++;
            }
            if(AmountinRow>=Settings.AMIRTW)
            {
                return true;
            }



            //Up Down
            AmountinRow = 1;
            for (int CurrentY = MoveY + 1; SafeGetVal(MoveX, CurrentY) == MoveType; CurrentY++)
            {
                AmountinRow++;
            }
            for (int CurrentY = MoveY - 1; SafeGetVal(MoveX, CurrentY) == MoveType; CurrentY--)
            {
                AmountinRow++;
            }
            if (AmountinRow >= Settings.AMIRTW)
            {
                return true;
            }


            //Up-Right Down-Left
            AmountinRow = 1;
            
            for (int offset = 1; SafeGetVal(MoveX +offset, MoveY+offset) == MoveType; offset++)
            {
                AmountinRow++;
            }
            for (int offset = 1; SafeGetVal(MoveX - offset, MoveY - offset) == MoveType; offset++)
            {
                AmountinRow++;
            }
            if (AmountinRow >= Settings.AMIRTW)
            {
                return true;
            }

            //Up-Left Down-Right
            AmountinRow = 1;

            for (int offset = 1; SafeGetVal(MoveX - offset, MoveY + offset) == MoveType; offset++)
            {
                AmountinRow++;
            }
            for (int offset = 1; SafeGetVal(MoveX + offset, MoveY - offset) == MoveType; offset++)
            {
                AmountinRow++;
            }
            if (AmountinRow >= Settings.AMIRTW)
            {
                return true;
            }
            return false;
        }
        int SafeGetVal(int x, int y)
        {
            if (!ValSafe(x,y))
            {
                return  -1;
            }
            return HumanView[x, y];
        }

        bool ValSafe(int x, int y)
        {
          return  !(x < 0 || y < 0 || x >= Size || y >= Size);
     
        }

        
       public static int CoordsToint(int x, int y)
        {
            return x + y * Size;
        }

       public static void intToCoords(int Num, out int x, out int y)
       {
            x = Num % Size;
            y = Num / Size;
       }
        //AIs always receive input as if they are P1 so the P2 View is an inverted view of the board
        int[] P1View = new int[Settings.BoardSize * Settings.BoardSize*5];
        int[] P2View = new int[Settings.BoardSize * Settings.BoardSize*5];
        int[,] HumanView = new int[Settings.BoardSize, Settings.BoardSize];

        static private ReverseComparer s_Comparer = new ReverseComparer();

     
    }

    public class ReverseComparer : IComparer<float>
    {
        public int Compare(float object1, float object2)
        {
            return -((IComparable)object1).CompareTo(object2);
        }
    }


}
