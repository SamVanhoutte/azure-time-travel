using System.Collections.Generic;
using System.Linq;
using EngineEventGenerator.Models;

namespace EngineEventGenerator
{
    public class CycleState
    {
        public int NextCycle;
        public Direction Direction;
        public IEnumerable<EngineCycle> Cycles { get; set; }

        public EngineCycle PopCycle()
        {
            var cycle = Cycles.First(c => c.Cycle == NextCycle);
            if (Direction == Direction.Down)
            {
                NextCycle -= 1;
                if (NextCycle < 1)
                {
                    Direction = Direction.Up;
                    NextCycle = 1;
                }
            }
            else
            {
                NextCycle += 1;
                if (NextCycle > Cycles.Count())
                {
                    Direction = Direction.Down;
                    NextCycle = Cycles.Count();
                }
            }

            return cycle;
        }
    }
}