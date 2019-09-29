#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Quiver.game;

#endregion

namespace Quiver.system
{
    public class pathfinding
    {
        //TODO: talk on sources
        /*
         
            A* PATHFINDING ALOGORITHM

            www.redblobgames.com/pathfinding/a-star/implementation.html

         */

        public static double Heuristic(vector a, vector b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public static void GeneratePathTo(vector start, vector goal, vector offset, ref List<vector> path)
        {
            path = Task.Run(() => GeneratePath(start, goal, offset)).Result;
        }

        private static List<vector> GeneratePath(vector start, vector goal, vector offset)
        {
            var path = new List<vector>();
            var open = new List<vector>();
            var closed = new List<vector>();
            var cameFrom = new Dictionary<vector, vector>();
            var costSoFar = new Dictionary<vector, double>();

            cameFrom.Clear();
            costSoFar.Clear();
            var frontier = new pqueue<vector>();

            if (world.IsSolid(new vector(goal.x, goal.y))) return path;

            var clock = new Stopwatch();
            //logger.WriteLine(string.Format("calculating path from {0} to {1}", start, goal));

            frontier.Enqueue(start, 0);
            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(goal))
                    break;

                if (clock.Elapsed.TotalSeconds > 1)
                    break;

                // 8 dir movement
                for (var i = 0; i < 8; i++)
                {
                    var next = GetNeighbours(current)[i];

                    // check all axis of movement
                    if (world.IsSolid(new vector(next.x, next.y)) || world.IsSolid(new vector(current.x, next.y)) ||
                        world.IsSolid(new vector(next.x, current.y)))
                        continue;

                    if (cameFrom.ContainsValue(next))
                        continue;

                    // set cost by heuristic
                    var newCost = costSoFar[current] + (next.x == current.x || next.y == current.y ? 0.5f : 0.46f) +
                                  engine.random.Next(0, 30) / 100;
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
            //clock.Dispose();

            return path;
        }

        private static vector[] GetNeighbours(vector v)
        {
            return new[]
            {
                new vector(v.x - 1, v.y), // left
                new vector(v.x + 1, v.y), // right
                new vector(v.x, v.y - 1), // up
                new vector(v.x, v.y + 1), // down
                new vector(v.x - 1, v.y - 1), // up, left
                new vector(v.x + 1, v.y - 1), // up, right
                new vector(v.x - 1, v.y + 1), // down, left
                new vector(v.x + 1, v.y + 1) // down, right
            };
        }
    }
}