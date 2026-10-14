using System;
using Core40k;
using Verse;

namespace Abhuman40k;

public class PlaceWorker_BeastmanInColonyLimited : PlaceWorker
{
    private static readonly SimpleCurve AllowedConduitAmountCurve =
    [
        new CurvePoint(0f, 0f),
        new CurvePoint(2f, 1f),
        new CurvePoint(4f, 2f),
        new CurvePoint(5f, 3f),
        new CurvePoint(10f, 6f),
        new CurvePoint(20f, 9f),
    ];

    /// <summary>Spawned free colonists on the map with the Herdstone Affinity gene.</summary>
    public static int AffinityPawnCount(Map map)
    {
        var count = 0;
        foreach (var pawn in map.mapPawns.FreeColonistsSpawned)
        {
            if (pawn.genes != null && pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_BeastmanHerdstoneAffinity))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>Number of herdstone markers allowed for the given number of Herdstone Affinity colonists.</summary>
    public static int AllowedConduits(int affinityPawns)
    {
        return (int)Math.Floor(AllowedConduitAmountCurve.Evaluate(affinityPawns));
    }

    /// <summary>Herdstone Affinity colonists needed before one more marker is allowed, or -1 if already at the maximum.</summary>
    public static int AffinityPawnsForNextConduit(int affinityPawns)
    {
        var allowed = AllowedConduits(affinityPawns);
        var lastPointPawns = (int)Math.Ceiling(AllowedConduitAmountCurve.Points[AllowedConduitAmountCurve.PointsCount - 1].x);
        for (var pawns = affinityPawns + 1; pawns <= lastPointPawns; pawns++)
        {
            if (AllowedConduits(pawns) > allowed)
            {
                return pawns;
            }
        }
        return -1;
    }

    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var conduitCount = map.listerBuildings.CountBuildingColonistOfDef(Abhuman40kDefOf.BEWH_HerdstoneConduitPlayer);
        var affinityPawns = AffinityPawnCount(map);
        var allowedConduits = AllowedConduits(affinityPawns);

        if (conduitCount < allowedConduits)
        {
            return true;
        }

        var neededPawns = AffinityPawnsForNextConduit(affinityPawns);
        if (neededPawns < 0)
        {
            return "BEWH.Abhuman.PlacementLimit.HerdstoneConduitMax".Translate(allowedConduits);
        }

        if (allowedConduits == 0)
        {
            return "BEWH.Abhuman.PlacementLimit.HerdstoneConduitNone".Translate(neededPawns, affinityPawns);
        }

        return "BEWH.Abhuman.PlacementLimit.HerdstoneConduitLimit".Translate(conduitCount, allowedConduits, affinityPawns, neededPawns);
    }
}