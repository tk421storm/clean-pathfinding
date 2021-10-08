
using Verse;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using RimWorld;
using System;
using System.Collections.Generic;
using static CleanPathfinding.ModSettings_CleanPathfinding;
 
namespace CleanPathfinding
{
    public class Mod_CleanPathfinding : Mod
	{	
		public Mod_CleanPathfinding(ModContentPack content) : base(content)
		{
			new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
			base.GetSettings<ModSettings_CleanPathfinding>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard options = new Listing_Standard();
			options.Begin(inRect);
			options.Label("Dirty terrain avoidance (Mod default: 8, Min: 0, Max: 30): " + bias, -1f, "Owl_BiasToolTip".Translate());
			bias = (int)options.Slider((float)bias, 0f, 30f);
			options.Label("Road attraction (Mod default: 4, Min: 0, Max: 4): " + roadBias, -1f, "Owl_RoadBiasToolTip".Translate());
			roadBias = (int)options.Slider((float)roadBias, 0f, 4f);
			options.Label("Extra pathfinding range (Mod default: 0, Min: 0, Max: 230): " + extraRange, -1f, "Owl_ExtraRangeToolTip".Translate());
			extraRange = (int)options.Slider((float)extraRange, 0f, 230f);
			options.CheckboxLabeled("Pathfinding should factor light", ref factorLight, "Owl_FactorLight".Translate());
			options.CheckboxLabeled("Pawn ignore rules if carrying another pawn", ref factorCarryingPawn, "Owl_FactorCarryingPawn".Translate());
			options.CheckboxLabeled("Pawns ignore rules if bleeding", ref factorBleeding, "Owl_FactorBleeding".Translate());
			options.End();
			base.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Clean Pathfinding";
		}

		public override void WriteSettings()
		{
			base.WriteSettings();
			UpdatePathCosts();
		}

		public static void UpdatePathCosts()
		{
			//Reset the cache
			terrainCache?.ToList().ForEach(x => x.Value[1] = 0);
			DefDatabase<TerrainDef>.AllDefs.Where(x => terrainCache.ContainsKey(x.GetHashCode()))?.ToList().ForEach(y => 
				y.extraNonDraftedPerceivedPathCost = terrainCache[y.GetHashCode()][0]);

			//Avoid filth
			if (bias > 0)
			{
				DefDatabase<TerrainDef>.AllDefs.Where(x => x.generatedFilth != null).ToList().ForEach(y => 
				{
					y.extraNonDraftedPerceivedPathCost += bias; 
					terrainCache[y.GetHashCode()][1] = bias;
				});
			}
			//Attraction to roads
			if (roadBias > 0)
			{
				DefDatabase<TerrainDef>.AllDefs.Where(x => x.tags != null && x.tags.Contains("Road")).ToList().ForEach(y => 
				{
					y.extraNonDraftedPerceivedPathCost -= roadBias;
					terrainCache[y.GetHashCode()][1] -= roadBias;
				});
			}

			//Debug
			//terrainCache.ToList().ForEach(x => Log.Message(x.Key.ToString() + " is " + x.Value[1].ToString()));
			//DefDatabase<TerrainDef>.AllDefs.ToList().ForEach(x => Log.Message("Cost for " + x.defName + " is now " + x.extraNonDraftedPerceivedPathCost));

			//Reset the extra pathfinding range curve
			if (extraRange > 0)
			{
				Custom_DistanceCurve = new SimpleCurve
				{
					{
						new CurvePoint(40f + extraRange, 1f),
						true
					},
					{
						new CurvePoint(120f + extraRange, 2.8f),
						true
				}};
			}
			else Custom_DistanceCurve = null;
			

			//If playing, update the pathfinders now
			if (Find.World != null) Find.Maps.ForEach(x => x.pathing.RecalculateAllPerceivedPathCosts());
		}

		public static Dictionary<int, int[]> terrainCache = new Dictionary<int, int[]>();
		public static SimpleCurve Custom_DistanceCurve;
	}

	public class ModSettings_CleanPathfinding : ModSettings
		{
		public override void ExposeData()
		{
			Scribe_Values.Look<int>(ref bias, "bias", 8, false);
			Scribe_Values.Look<int>(ref roadBias, "roadBias", 4, false);
			Scribe_Values.Look<int>(ref extraRange, "extraRange", 0, false);
			Scribe_Values.Look<bool>(ref factorLight, "factorLight", false, false);
			Scribe_Values.Look<bool>(ref factorCarryingPawn, "factorCarryingPawn", false, false);
			Scribe_Values.Look<bool>(ref factorBleeding, "factorBleeding", false, false);
			base.ExposeData();
		}

		static public int bias = 8;
		static public int roadBias = 4;
		static public int extraRange = 0;
		static public bool factorLight = false;
		static public bool factorCarryingPawn = false;
		static public bool factorBleeding = false;
	}
}
