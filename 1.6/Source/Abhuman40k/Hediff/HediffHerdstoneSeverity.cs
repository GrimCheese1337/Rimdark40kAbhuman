using UnityEngine;
using Verse;

namespace Abhuman40k;

public class HediffHerdstoneSeverity : HediffWithComps
{
    private const int RecacheIntervalTicks = 120;

    private const float MinSeverity = 0.01f;

    private float lastKnownSeverity = MinSeverity;

    [Unsaved(false)]
    private int lastRecacheTick = -1;

    public override float Severity
    {
        get
        {
            // Off the map (caravan, transport pod) there is nothing to measure. Returning 0 here
            // made Hediff.ShouldRemove true and silently deleted the hediff, so hold the last
            // value instead.
            var map = pawn?.Map;
            if (map == null)
            {
                return lastKnownSeverity;
            }

            // This getter is on a very hot path, so the network is only re-measured a few times
            // a second.
            var ticksGame = Find.TickManager?.TicksGame ?? 0;
            if (lastRecacheTick >= 0 && ticksGame - lastRecacheTick < RecacheIntervalTicks)
            {
                return lastKnownSeverity;
            }

            lastRecacheTick = ticksGame;

            var fraction = map.GetComponent<MapComponent_HerdstoneNetwork>()?.PowerFractionFor(pawn) ?? 0f;

            // Nothing reaching the pawn right now: hold the last share so the buff runs out on
            // the hediff's own timer instead of snapping off the moment they step outside.
            if (fraction > 0f)
            {
                lastKnownSeverity = Mathf.Clamp(fraction, MinSeverity, 1f);
            }

            return lastKnownSeverity;
        }
        set => base.Severity = value;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastKnownSeverity, "lastKnownSeverity", MinSeverity);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            lastKnownSeverity = Mathf.Clamp(lastKnownSeverity, MinSeverity, 1f);
        }
    }
}
