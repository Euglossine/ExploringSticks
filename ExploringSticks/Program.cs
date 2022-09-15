using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringSticks
{
    class SticksPosition
    {
        int _player1Large, _player1Small, _player2Large, _player2Small;
        public SticksPosition(int player1A, int player1B, int player2A, int player2B)
        {
            if (player1A > player1B)
            {
                _player1Large = player1A;
                _player1Small = player1B;
            }
            else
            {
                _player1Large = player1B;
                _player1Small = player1A;
            }
            if (player2A > player2B)
            {
                _player2Large = player2A;
                _player2Small = player2B;
            }
            else
            {
                _player2Large = player2B;
                _player2Small = player2A;
            }
        }
        public bool IsWonPlayer1()
        {
            return (_player2Large == 0) && (_player2Small == 0);
        }
        public bool IsWonPlayer2()
        {
            return (_player1Large == 0) && (_player1Small == 0);
        }
        public bool IsWon()
        {
            return IsWonPlayer1() || IsWonPlayer2();
        }

        public System.Collections.Generic.IEnumerable<SticksPosition> Moves()
        {
            // just do all the combos manually, there are only 5 potential
            // note that the moves swap the players (1 and 2) since player 2 now needs to move!
            // % is modulo (remainder)
            if (_player1Small != 0)
            {
                if (_player2Small != 0)
                    yield return new SticksPosition((_player2Small + _player1Small) % 5, _player2Large, _player1Small, _player1Large);
                if (_player2Large != 0)
                    yield return new SticksPosition(_player2Small, (_player2Large + _player1Small) % 5, _player1Small, _player1Large);
            }
            else if (_player1Large % 2 == 0) // bump!
            {
                yield return new SticksPosition(_player2Small , _player2Large, _player1Large/2, _player1Large/2);
            }
            if (_player1Large != 0)
            {
                if (_player2Small != 0)
                    yield return new SticksPosition((_player2Small + _player1Large) % 5, _player2Large, _player1Small, _player1Large);
                if (_player2Large != 0)
                    yield return new SticksPosition(_player2Small, (_player2Large + _player1Large) % 5, _player1Small, _player1Large);
            }
        }
        public System.Collections.Generic.IEnumerable<SticksPosition> MovesNoDupes()
        {
            HashSet<string> all = new HashSet<string>();
            foreach (SticksPosition cur in Moves())
            {
                if (!all.Contains(cur.Stringify()))
                {
                    all.Add(cur.Stringify());
                    yield return cur;
                }
            }
        }
        public string Stringify()
        {
            return "{" + _player1Small.ToString() + "," + _player1Large.ToString() + "}" + " , " + "{" + _player2Small.ToString() + "," + _player2Large.ToString() + "}";
        }
    }
    class Program
    {
        static HashSet<string> _all = new HashSet<string>();
        static int GoReachablity(SticksPosition pos)
        {
            if (pos.IsWonPlayer1() || pos.IsWonPlayer2())
                return 0;
            if (_all.Contains(pos.Stringify()))
                return 0;
            _all.Add(pos.Stringify());
            //Console.WriteLine(pos.Stringify());
            int total = 0;
            foreach (SticksPosition cur in pos.Moves())
            {
                total += GoReachablity(cur);
            }
            return total + 1;
        }
        static void Main(string[] args)
        {
            // NOTE: most functions assume that _all is freshly created
            FindWonAndLostPositions();
            SticksPosition initialPosition = new SticksPosition(1, 1, 1, 1);
            Console.WriteLine(MaximumDepth(initialPosition, 0));
            //AssemblyIndex(initialPosition);
            //ReachableSticksPositions();
            Console.ReadKey();
        }
        static int _max = 0;
        static Stack<SticksPosition> _stackCurrent = new Stack<SticksPosition>();

        static void FindWonAndLostPositions()
        {
            //create all of the positions
            List<SticksPosition> unkOutcomePositions = new List<SticksPosition>();
            HashSet<string> lostPositions = new HashSet<string>();
            HashSet<string> wonPositions = new HashSet<string>();

            for (int i = 0; i < 5; ++i)
                for (int j = i; j < 5; ++j)
                {
                    for (int k = 0; k < 5; ++k)
                        for (int m = k; m < 5; ++m)
                        {
                            // not possible for it to be my turn but the other guy is dead
                            if ((k == 0) && (m == 0))
                                continue;
                            SticksPosition pos = new SticksPosition(i, j, k, m);

                            // current player has lost
                            if ((i == 0) && (j == 0))
                                lostPositions.Add(pos.Stringify());
                            else
                                unkOutcomePositions.Add(pos);

                        }
                }

            List<SticksPosition> positionsTemp = new List<SticksPosition>();
            bool bContinue;
            do
            {
                foreach (SticksPosition pos in unkOutcomePositions)
                {
                    bool isLost = true; // will change if any outcome is not won by other player
                    bool isWon = false; // will change if any outcome is lost by other player, since we'd choose that one
                    foreach (SticksPosition posResult in pos.Moves())
                    {
                        if (lostPositions.Contains(posResult.Stringify()))
                        {
                            isWon = true;
                            break;
                        }
                        if (!wonPositions.Contains(posResult.Stringify())) // at least one outcome is not won by the other player 
                        {
                            isLost = false;
                        }
                    }
                    if (isWon)
                    {
                        wonPositions.Add(pos.Stringify());
                    }
                    else if (isLost)
                    {
                        lostPositions.Add(pos.Stringify());
                    }
                    else
                    {
                        positionsTemp.Add(pos);
                    }
                }
                bContinue = (positionsTemp.Count != unkOutcomePositions.Count); // at least one position has been classified
                unkOutcomePositions = positionsTemp;
                positionsTemp = new List<SticksPosition>();
            } while (bContinue);
            Console.WriteLine(unkOutcomePositions.Count);
        }

        static int MaximumDepth(SticksPosition pos, int depth)
        {
            if (_all.Contains(pos.Stringify()))
                return depth-1;
            
            _all.Add(pos.Stringify());
            _stackCurrent.Push(pos);

            if (depth > _max)
            {
                Console.WriteLine(depth + "-----------------");
                foreach (SticksPosition cur in _stackCurrent)
                    Console.WriteLine(depth + ": " + cur.Stringify());
                _max = depth;
            }

            int maxDepth = depth;

            foreach (SticksPosition cur in pos.MovesNoDupes())
            {
                if (!cur.IsWon())
                {
                    //Console.WriteLine(depth+": "+cur.Stringify());
                    maxDepth = Math.Max(maxDepth, MaximumDepth(cur, depth + 1));
                }
            }

            _all.Remove(pos.Stringify());
            _stackCurrent.Pop();
            return maxDepth;
        }
        /// see https://en.wikipedia.org/wiki/Assembly_theory
        static void AssemblyIndex(SticksPosition pos)
        {
            Stack<SticksPosition> curSet = new Stack<SticksPosition>();
            //HashSet<string> curSetStr = new HashSet<string>();
            Stack<SticksPosition> nxtSet = new Stack<SticksPosition>();
            //HashSet<string> nxtSetStr = new HashSet<string>(); 
            
            curSet.Push(pos);
            int iLevel = 0;
            do
            {
                Console.WriteLine("Level " + iLevel);
                while (curSet.Count > 0)
                {
                    SticksPosition sposLevel = curSet.Pop();
                    Console.WriteLine(sposLevel.Stringify());
                    foreach (SticksPosition cur in sposLevel.Moves())
                    {
                        if (!_all.Contains(cur.Stringify()))
                        {
                            if (!cur.IsWon())
                               nxtSet.Push(cur);
                            _all.Add(cur.Stringify());
                        }
                    }
                }
                Stack<SticksPosition> temp = curSet;
                curSet = nxtSet;
                nxtSet = temp;
                ++iLevel;
            } while (curSet.Count > 0);
        }

        static void ReachableSticksPositions()
        {
            SticksPosition initialPosition = new SticksPosition(1, 1, 1, 1);
            GoReachablity(initialPosition);
            Console.WriteLine("There are " + _all.Count + " unique playable positions in Sticks");
            Console.WriteLine("These are the unreachable positions");
            int cIter = 0;
            for (int i = 0; i < 5; ++i)
                for (int j = i; j < 5; ++j)
                {
                    //Console.WriteLine(i + "," + j);
                    if ((i == 0) && (j == 0))
                        continue;
                    for (int k = 0; k < 5; ++k)
                        for (int m = k; m < 5; ++m)
                        {
                            if ((k == 0) && (m == 0))
                                continue;
                            ++cIter;
                            SticksPosition pos = new SticksPosition(i, j, k, m);
                            if (!_all.Contains(pos.Stringify()))
                                Console.WriteLine(pos.Stringify());
                        }
                }
            Console.WriteLine(cIter + " total possible hand states");
        }

    }
}
