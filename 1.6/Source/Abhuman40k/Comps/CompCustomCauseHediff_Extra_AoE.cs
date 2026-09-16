using VEF.Buildings;
using Verse;

namespace Abhuman40k;

public class CompCustomCauseHediff_Extra_AoE : CompCustomCauseHediff_AoE
{
    private new CompProperties_CustomCauseHediff_Extra_AoE Props => (CompProperties_CustomCauseHediff_Extra_AoE)props;

    /// <summary>Share this source carries of its network's fixed power pool.</summary>
    public int NetworkWeight => Props.networkWeight;

    /// <summary>
    /// Whether this source is currently projecting onto the pawn, using the same gene, faction
    /// and range checks that decide whether the hediff gets applied in the first place.
    /// </summary>
    public bool CoversPawn(Pawn pawn)
    {
        if (pawn is not { Spawned: true } || parent == null || pawn.Map != parent.MapHeld)
        {
            return false;
        }

        return IsPawnAffectedAndInRange(pawn, cacheRoom: false);
    }

    public override bool IsPawnAffected(Pawn target)
    {
        if (target.genes == null)
        {
            return false;
        }

        if (!target.genes.HasActiveGene(Props.requiredGene))
        {
            return false;
        }

        if (Props.sameFactionOnly && target.Faction != parent.Faction)
        {
            return false;
        }

        return base.IsPawnAffected(target);
    }

    protected override void TickInterval(int delta)
    {
        try
        {
            var worksInside = Props.worksInside;
            var worksOutside = Props.worksOutside;
            if (!worksInside)
            {
                if (!worksOutside)
                {
                    return;
                }
                tempWorkingRoom = parent.GetRoom();
                var room = tempWorkingRoom;
                if (room is { PsychologicallyOutdoors: false })
                {
                    return;
                }
            }
            else if (!worksOutside)
            {
                tempWorkingRoom = parent.GetRoom();
                if (tempWorkingRoom == null || tempWorkingRoom.PsychologicallyOutdoors)
                {
                    return;
                }
            }
            var allPawnsSpawned = parent.MapHeld.mapPawns.AllPawnsSpawned;
            foreach (var pawn in allPawnsSpawned)
            {
                if (IsPawnAffectedAndInRange(pawn, cacheRoom: true))
                {
                    GiveOrUpdateHediff(pawn);
                }
                if (pawn.carryTracker.CarriedThing is Pawn pawn2 && IsPawnAffectedAndInRange(pawn2, cacheRoom: true))
                {
                    GiveOrUpdateHediff(pawn2);
                }
            }
        }
        finally
        {
            tempWorkingRoom = null;
        }
    }
}
