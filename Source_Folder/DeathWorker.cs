﻿using RimWorld;
using System.Linq;
using Verse;

namespace AvaliMod
{
    // the _Test seems to indicate this either needs to be renamed, or removed
    public class DeathActionWorker_Test : DeathActionWorker
    {
        private readonly bool enableDebug = LoadedModManager.GetMod<RimValiMod>().GetSettings<RimValiModSettings>().enableDebugMode;

        public override void PawnDied(Corpse corpse)
        {
            if (enableDebug)
            {
                Log.Message("Death detected");
            }
            if (corpse.InnerPawn.def.HasComp(typeof(PackComp)))
            {
                if (!(RimValiUtility.Driver.Packs == null) && RimValiUtility.Driver.Packs.Count > 0)
                {
                    if (enableDebug)
                    {
                        Log.Message("I got here, packs exist.");
                    }
                    Pawn pawn = corpse.InnerPawn;
                    // I changed this from GetPack to GetPackWithoutSelf (the former doesn't exist anymore)
                    // Is that correct?
                    // -Toby
                    AvaliPack pack = pawn.GetPackWithoutSelf();
                    if (pack != null)
                    {
                        if (enableDebug)
                        {
                            Log.Message("The pawn had a pack");
                        }
                        DeathDate deathDate = new DeathDate(pawn)
                        {
                            //Log.Message(GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x).ToString());
                            day = GenDate.DayOfYear(1, Find.WorldGrid.LongLatOf(corpse.Map.Tile).x)
                        };
                        if (corpse.InnerPawn != null && enableDebug)
                        {
                            Log.Message("Pawn is not null");
                        }

                        deathDate.deadPawn = pawn;
                        if (enableDebug)
                        {
                            Log.Message("I got the deathdate");
                            Log.Message(deathDate.day.ToString());
                        }
                        pack.pawns.Remove(pawn);
                        if (pawn == pack.leaderPawn)
                        {
                            if (pack.pawns.Count > 0)
                            {
                                pack.leaderPawn = pack.pawns.First();
                            }
                        }
                        pack.deathDates.Add(deathDate);

                        //pack.size--;
                        if (enableDebug)
                        {
                            Log.Message(pack.GetAllNonNullPawns.Count.ToString());
                        }
                        foreach (Pawn packmate in pack.GetAllNonNullPawns)
                        {
                            Log.Message("On: " + packmate.Name.ToString());
                            if ((pawn != packmate) && !packmate.Dead)
                            {
                                Log.Message(packmate.Name.ToString() + " is not dead, adding thought");
                                PackComp comp = packmate.TryGetComp<PackComp>();
                                if (comp.Props.deathThought != null)
                                {
                                    packmate.needs.mood.thoughts.memories.TryGainMemory(comp.Props.deathThought, pawn);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}