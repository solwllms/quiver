using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using engine.game;
using SFML.System;

namespace engine.system
{
    public class Pathfinding
    {
        //TODO: talk on sources
        /*
         
            A* PATHFINDING ALOGORITHM

            www.redblobgames.com/pathfinding/a-star/implementation.html

         */

        public static double Heuristic(Vector a, Vector b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public static void GeneratePathTo(Vector start, Vector goal, Vector offset, ref List<Vector> path)
        {
            path = Task.Run(() => GeneratePath(start, goal, offset)).Result;
        }

        private static List<Vector> GeneratePath(Vector start, Vector goal, Vector offset)
        {
            var path = new List<Vector>();
            var open = new List<Vector>();
            var closed = new List<Vector>();
            var cameFrom = new Dictionary<Vector, Vector>();
            var costSoFar = new Dictionary<Vector, double>();

            cameFrom.Clear();
            costSoFar.Clear();
            var frontier = new Pqueue<Vector>();

            if (World.IsSolid(new Vector(goal.x, goal.y))) return path;

            var clock = new Clock();
            //logger.WriteLine(string.Format("calculating path from {0} to {1}", start, goal));

            frontier.Enqueue(start, 0);
            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(goal))
                    break;

                if (clock.ElapsedTime.AsSeconds() > 1)
                    break;

                // 8 dir movement
                for (var i = 0; i < 8; i++)
                {
                    var next = GetNeighbours(current)[i];

                    // check all axis of movement
                    if (World.IsSolid(new Vector(next.x, next.y)) || World.IsSolid(new Vector(current.x, next.y)) ||
                        World.IsSolid(new Vector(next.x, current.y)))
                        continue;

                    if (cameFrom.ContainsValue(next))
                        continue;

                    // set cost by heuristic
                    var newCost = costSoFar[current] + (next.x == current.x || next.y == current.y ? 0.5f : 0.46f) +
                                   Engine.random.Next(0, 30) / 100;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;

                        closed.Add(next);
                    }
                }
            }

            path.Clear();
            var c = goal;
            while (c != start)
            {
                open.Add(c);
                path.Add(c + offset);
                if (!cameFrom.ContainsKey(c))
                    return path;
                c = cameFrom[c];
            }

            path.Add(start + offset); // optional: for complete path
            path.Reverse();

            //logger.WriteLine(string.Format("path calculations finished. (length {0}, took {1}ms)", came_from.Count, clock.ElapsedTime.AsMilliseconds()));
            clock.Dispose();

            return path;
        }

        private static Vector[] GetNeighbours(Vector v)
        {
            return new[]
            {
                new Vector(v.x - 1, v.y), // left
                new Vector(v.x + 1, v.y), // right
                new Vector(v.x, v.y - 1), // up
                new Vector(v.x, v.y + 1), // down
                new Vector(v.x - 1, v.y - 1), // up, left
                new Vector(v.x + 1, v.y - 1), // up, right
                new Vector(v.x - 1, v.y + 1), // down, left
                new Vector(v.x + 1, v.y + 1) // down, right
            };
        }
    }
}