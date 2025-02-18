﻿using System;
using Verse;
using RimWorld;
namespace AvaliMod
{
    public class PackLossThoughtWorker : ThoughtWorker
    {
        int stageOne = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().stageOneDaysPackloss;
        int stageTwo = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().stageTwoDaysPackloss;
        int stageThree = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().stageThreeDaysPackloss;
        int dayLen = 60000;
        public void UpdatePackLoss(Pawn pawn)
        {
            PackComp packComp = pawn.TryGetComp<PackComp>();

            if (packComp.ticksSinceLastInpack == 0 && (pawn.GetPackWithoutSelf() == null || pawn.GetPackWithoutSelf().GetAllNonNullPawns.EnumerableNullOrEmpty() || pawn.GetPackWithoutSelf().GetAllNonNullPawns.Count == 0))
            {
                packComp.inPack = false;
            }
            else if (pawn.story.traits.HasTrait(AvaliDefs.AvaliPackBroken) || (pawn.GetPackWithoutSelf() != null && pawn.GetPackWithoutSelf().GetAllNonNullPawns.Count > 1))
            {
                packComp.inPack = true;
            }
        }
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if (RimValiUtility.Driver==null || !RimValiUtility.Driver.PawnHasPack(p))
            {
                return ThoughtState.Inactive;
            }
            
            PackComp packComp = p.TryGetComp<PackComp>();
            AvaliThoughtDriver thoughtComp = p.TryGetComp<AvaliThoughtDriver>();

            if (thoughtComp == null||packComp == null ||p.health.hediffSet.hediffs.Any(x => thoughtComp.Props.packLossPreventers.Contains(x.def)) || !LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().packLossEnabled){
                return ThoughtState.Inactive; 
            }
            UpdatePackLoss(p);
            
            int timeAlone = packComp.ticksSinceLastInpack;
            if (timeAlone == Find.TickManager.TicksGame || timeAlone == 0){ return ThoughtState.Inactive; }
            if (timeAlone >= dayLen*stageThree)
            {
                if (packComp.lastDay+3 < GenDate.DaysPassed)
                {
                    if (new Random().Next(0, 100) < RimValiMod.settings.packBrokenChance * (RimValiUtility.Driver.GetPackCount(p) / 2) && RimValiMod.settings.canGetPackBroken && !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken)){p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));}
                    packComp.lastDay = GenDate.DaysPassed;
                }
              

                return ThoughtState.ActiveAtStage(2);
            }
            if (timeAlone >= dayLen*stageTwo)
            {

                if (packComp.lastDay + 3 < GenDate.DaysPassed)
                {
                    if (new Random().Next(0, 100) < (RimValiMod.settings.packBrokenChance*(RimValiUtility.Driver.GetPackCount(p)/3)) && RimValiMod.settings.canGetPackBroken && !p.story.traits.HasTrait(AvaliDefs.AvaliPackBroken)){p.story.traits.GainTrait(new Trait(AvaliDefs.AvaliPackBroken));}
                    packComp.lastDay = GenDate.DaysPassed;
                }
                return ThoughtState.ActiveAtStage(1);
            }
            if (timeAlone >= dayLen*stageOne)
            {
                return ThoughtState.ActiveAtStage(0);
            }
          
            return ThoughtState.Inactive;
        }
    }
}