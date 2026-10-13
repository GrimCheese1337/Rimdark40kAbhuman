using Verse;

namespace Abhuman40k;

public class Building_HerdstoneConduit : Building
{
    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        Abhuman40kUtils.DrawLinesToBuildingsOfDef(this, Abhuman40kDefOf.BEWH_HerdstonePlayer);
    }
}
