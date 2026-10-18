using System;
namespace Scrapshift
{
    // The supplied project samples Area2D contacts at 60 physics ticks/second.
    // Rendering and input polling can run at a different rate.
    public static class ReferencePhysicsClock
    {
        public const float Step=1f/60;
        const double ExactStep=1.0/60;
        public static int Consume(ref double accumulator,float delta)
        {
            // Bound recovery from a long stall; do not run an unbounded loop.
            accumulator=Math.Min(8*ExactStep,accumulator+Math.Max(0,delta));
            int steps=Math.Min(8,(int)Math.Floor((accumulator+1e-8)/ExactStep));
            accumulator=Math.Max(0,accumulator-steps*ExactStep);return steps;
        }
    }
}
