using VEF.Buildings;
using Verse;

namespace Abhuman40k;

public class CompProperties_CustomCauseHediff_Extra_AoE : CompProperties_CustomCauseHediff_AoE
{
    public GeneDef requiredGene;

    public bool sameFactionOnly;

    public int networkWeight = 1;

    public CompProperties_CustomCauseHediff_Extra_AoE()
    {
        compClass = typeof(CompCustomCauseHediff_Extra_AoE);
    }
}
