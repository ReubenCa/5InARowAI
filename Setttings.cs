using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5InARowAI
{
    public  static class Settings
    {
        public const int BoardSize = 5;
        public const int AMIRTW = 3;
        public const int StartMove = 24;
        public const int LayerCount = 4;
        public const bool EnforceLegalMoves = true;
        public const float DisMean = 0f;
        public const float DisStdDev = 1f;
    }
    
}
