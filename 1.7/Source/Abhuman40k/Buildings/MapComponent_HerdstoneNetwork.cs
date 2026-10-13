using System.Collections.Generic;
using Verse;

namespace Abhuman40k;

/// <summary>
/// Tracks the herdstone sources on a map and works out how much of a herd's fixed power pool
/// reaches a given pawn. Every source carries a share of one pool, so raising conduits spreads
/// the same total strength over more ground rather than adding to it.
/// </summary>
public class MapComponent_HerdstoneNetwork : MapComponent
{
    private const int RebuildIntervalTicks = 60;

    private readonly List<CompCustomCauseHediff_Extra_AoE> sources = [];

    private int lastRebuildTick = -1;

    public MapComponent_HerdstoneNetwork(Map map)
        : base(map)
    {
    }

    /// <summary>Fraction of the pawn's own herd network currently projecting onto them, 0 to 1.</summary>
    public float PowerFractionFor(Pawn pawn)
    {
        if (pawn?.Faction == null)
        {
            return 0f;
        }

        RebuildIfStale();

        var totalWeight = 0;
        var coveredWeight = 0;

        foreach (var source in sources)
        {
            if (source.parent?.Faction != pawn.Faction)
            {
                continue;
            }

            var weight = source.NetworkWeight;
            if (weight <= 0)
            {
                continue;
            }

            totalWeight += weight;

            if (source.CoversPawn(pawn))
            {
                coveredWeight += weight;
            }
        }

        if (totalWeight <= 0 || coveredWeight <= 0)
        {
            return 0f;
        }

        return (float)coveredWeight / totalWeight;
    }

    private void RebuildIfStale()
    {
        var ticksGame = Find.TickManager?.TicksGame ?? 0;
        if (lastRebuildTick >= 0 && ticksGame - lastRebuildTick < RebuildIntervalTicks)
        {
            return;
        }

        lastRebuildTick = ticksGame;
        sources.Clear();

        AddSources(Abhuman40kDefOf.BEWH_HerdstonePlayer);
        AddSources(Abhuman40kDefOf.BEWH_HerdstoneConduitPlayer);
        AddSources(Abhuman40kDefOf.BEWH_HerdstoneRaid);
    }

    private void AddSources(ThingDef def)
    {
        if (def == null)
        {
            return;
        }

        var things = map.listerThings.ThingsOfDef(def);
        foreach (var thing in things)
        {
            var comp = thing.TryGetComp<CompCustomCauseHediff_Extra_AoE>();
            if (comp != null)
            {
                sources.Add(comp);
            }
        }
    }
}
