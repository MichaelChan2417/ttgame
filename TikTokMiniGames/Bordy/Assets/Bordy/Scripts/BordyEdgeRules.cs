using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bordy
{
    /// <summary>
    /// Shared = / × placement rules.
    /// 1. A vertical "=" is drawn upright (rotated), never as a sideways bar.
    /// 2. A × next to an = pair on the same line is redundant: two equals already make
    ///    two identical icons, so the next cell cannot match (no 3-in-a-row). Strip it.
    ///
    /// = / × 放置规则：上下相同用竖等号；等号对旁边同一直线上的叉可由「不能连 3 个」推出，去掉。
    /// </summary>
    public static class BordyEdgeRules
    {
        public static EdgeConstraint[] StripRedundantCrosses(EdgeConstraint[] edges)
        {
            if (edges == null || edges.Length == 0)
                return edges ?? Array.Empty<EdgeConstraint>();

            var equals = new List<EdgeConstraint>();
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].MustMatch)
                    equals.Add(edges[i]);
            }

            if (equals.Count == 0)
                return edges;

            var kept = new List<EdgeConstraint>(edges.Length);
            for (int i = 0; i < edges.Length; i++)
            {
                var e = edges[i];
                if (!e.MustMatch && IsCollinearNeighborOfEquals(e, equals))
                    continue;
                kept.Add(e);
            }

            return kept.Count == edges.Length ? edges : kept.ToArray();
        }

        /// <summary>Rotate "=" 90° when the pair is vertical. / 上下两格相同时把等号竖过来。</summary>
        public static void OrientSymbol(RectTransform rt, in EdgeConstraint edge)
        {
            if (rt == null)
                return;
            rt.localEulerAngles = edge.MustMatch && !edge.Horizontal
                ? new Vector3(0f, 0f, 90f)
                : Vector3.zero;
        }

        private static bool IsCollinearNeighborOfEquals(EdgeConstraint cross, List<EdgeConstraint> equals)
        {
            for (int i = 0; i < equals.Count; i++)
            {
                var eq = equals[i];
                if (eq.Horizontal != cross.Horizontal)
                    continue;

                if (eq.Horizontal)
                {
                    if (eq.Row == cross.Row && Math.Abs(eq.Col - cross.Col) == 1)
                        return true;
                }
                else if (eq.Col == cross.Col && Math.Abs(eq.Row - cross.Row) == 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
