using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using static CleanPathfinding.ModSettings_CleanPathfinding;
 
namespace CleanPathfinding
{
	#region Harmony


	[HarmonyPatch(typeof(PathFinder))]

	static class Patch_PathFinder_Finalize
	{
		/* here to trouble shoot memory leaks
		 * Soyuz caught this error. Please don't report this to the RocketMan team unless you're certain RocketMan caused this error. with error System.OutOfMemoryException: Out of memory
[Ref 3B9F4383]
 at (wrapper managed-to-native) System.Object.__icall_wrapper_ves_icall_array_new_specific(intptr,int)
 at System.Collections.Generic.List`1[T].set_Capacity (System.Int32 value) [0x00021] in <eae584ce26bc40229c1b1aa476bfa589>:0 
 at System.Collections.Generic.List`1[T].EnsureCapacity (System.Int32 min) [0x00036] in <eae584ce26bc40229c1b1aa476bfa589>:0 
 at System.Collections.Generic.List`1[T].Add (T item) [0x00010] in <eae584ce26bc40229c1b1aa476bfa589>:0 
 at Verse.AI.PawnPath.AddNode (Verse.IntVec3 nodePosition) [0x00000] in <957a20e0be784a65bc32cf449445b937>:0 
 at Verse.AI.PathFinder.FinalizedPath (System.Int32 finalIndex, System.Boolean usedRegionHeuristics) [0x00036] in <957a20e0be784a65bc32cf449445b937>:0 
 at Verse.AI.PathFinder.FindPath (Verse.IntVec3 start, Verse.LocalTargetInfo dest, Verse.TraverseParms traverseParms, Verse.AI.PathEndMode peMode, Verse.AI.PathFinderCostTuning tuning) [0x005a5] in <957a20e0be784a65bc32cf449445b937>:0 
     - TRANSPILER Owlchemist.CleanPathfinding.tmp: IEnumerable`1 CleanPathfinding.Patch_PathFinder:Transpiler(IEnumerable`1 instructions)
     - TRANSPILER OskarPotocki.VanillaFurnitureExpanded.Security: IEnumerable`1 VFESecurity.Patch_PathFinder+FindPath:Transpiler(IEnumerable`1 instructions)
 at Verse.AI.PathFinder.FindPath (Verse.IntVec3 start, Verse.LocalTargetInfo dest, Verse.Pawn pawn, Verse.AI.PathEndMode peMode, Verse.AI.PathFinderCostTuning tuning) [0x0003e] in <957a20e0be784a65bc32cf449445b937>:0 
 at Verse.AI.Pawn_PathFollower.GenerateNewPath () [0x0005e] in <957a20e0be784a65bc32cf449445b937>:0 
     - PREFIX OskarPotocki.VFECore: Boolean VFECore.PhasingPatches:GenerateNewPath_Prefix(Pawn_PathFollower __instance, Pawn ___pawn, LocalTargetInfo ___destination, PathEndMode ___peMode, PawnPath& __result)
 at Verse.AI.Pawn_PathFollower.TrySetNewPath () [0x00000] in <957a20e0be784a65bc32cf449445b937>:0 
 at Verse.AI.Pawn_PathFollower.TryEnterNextPathCell () [0x00303] in <957a20e0be784a65bc32cf449445b937>:0 
     - PREFIX juanlopez2008.LightsOut: Void LightsOut.Patches.Lights.DetectPawnRoomChange:Prefix(Pawn ___pawn, Room& __state)
     - POSTFIX OskarPotocki.VFECore: Void VFECore.PhasingPatches:UnfogEnteredCells(Pawn_PathFollower __instance, Pawn ___pawn)
     - POSTFIX juanlopez2008.LightsOut: Void LightsOut.Patches.Lights.DetectPawnRoomChange:Postfix(Pawn ___pawn, Room& __state)
 at Verse.AI.Pawn_PathFollower.PatherTick () [0x00404] in <957a20e0be784a65bc32cf449445b937>:0 
     - PREFIX Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_PathFollower_Patch+Pawn_PathFollower_PatherTick:Prefix(Pawn_PathFollower __instance)
     - POSTFIX Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_PathFollower_Patch+Pawn_PathFollower_PatherTick:Postfix(Pawn_PathFollower __instance)
     - FINALIZER Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_PathFollower_Patch+Pawn_PathFollower_PatherTick:Finalizer(Exception __exception)
 at Verse.Pawn.Tick () [0x000d8] in <957a20e0be784a65bc32cf449445b937>:0 
     - TRANSPILER Krkr.RocketMan.Soyuz: IEnumerable`1 Soyuz.Patches.Pawn_Tick_Patch:Transpiler(IEnumerable`1 instructions, ILGenerator generator)
     - POSTFIX Roolo.DualWield: Void DualWield.HarmonyInstance.Pawn_Tick:Postfix(Pawn __instance)
     - FINALIZER Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_Tick_Patch:Finalizer(Pawn __instance, Exception __exception)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch7 (string)
RocketMan.Logger:Debug (string,System.Exception,string)
Soyuz.Patches.Pawn_Tick_Patch:Finalizer (Verse.Pawn,System.Exception)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Pawn.Tick_Patch2 (Verse.Pawn)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickList.Tick_Patch2 (Verse.TickList)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch4 (Verse.TickManager)
Verse.TickManager:TickManagerUpdate ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Game.UpdatePlay_Patch2 (Verse.Game)
Verse.Root_Play:Update ()
		*/
		static MethodBase TargetMethod()
        {
			return typeof(PathFinder).GetMethod("FinalizedPath", BindingFlags.NonPublic | BindingFlags.Instance);
        }

		[HarmonyPrefix]
		static bool FinalizedPath_Prefix(ref PathFinder __instance, ref PawnPath __result, int finalIndex, bool usedRegionHeuristics)
		{
			PawnPath emptyPawnPath = __instance.map.pawnPathPool.GetEmptyPawnPath();
			int num = finalIndex;

			int max = 0;
			for (; ; )
			{
				int parentIndex = PathFinder.calcGrid[num].parentIndex;
				emptyPawnPath.AddNode(__instance.map.cellIndices.IndexToCell(num));
				if (num == parentIndex)
				{
					break;
				}
				if (max>1000)
                {

					TraverseParms traverse = __instance.traverseParms;
					Log.Warning("bailing out of path calculation for "+traverse.pawn+" with "+traverse.pawn.TicksPerMoveCardinal+"/"+traverse.pawn.TicksPerMoveDiagonal+" TicksPerMoveCardinal/Diagonal after 1000 path nodes added to prevent mem leaks, on num " + num + " aiming for " + parentIndex);
					//__result = PawnPath.NotFound;

					return true;
				}
				num = parentIndex;
				max++;
			}
			emptyPawnPath.SetupFound((float)PathFinder.calcGrid[finalIndex].knownCost, usedRegionHeuristics);
			__result = emptyPawnPath;

			return false;
		}

	}
	[HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath), new Type[] {
		typeof(IntVec3),
		typeof(LocalTargetInfo),
		typeof(TraverseParms),
		typeof(PathEndMode),
		typeof(PathFinderCostTuning) })]
	static class Patch_PathFinder
	{ 

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int offset = -1, objectsFound = 0;
			bool ran = false, searchForObjects = false, thresholdReplaced = false;

			var method_regionModeThreshold = AccessTools.Field(typeof(ModSettings_CleanPathfinding), nameof(ModSettings_CleanPathfinding.regionModeThreshold));
			var field_extraDraftedPerceivedPathCost = AccessTools.Field(typeof(TerrainDef), nameof(TerrainDef.extraDraftedPerceivedPathCost));
			var field_extraNonDraftedPerceivedPathCost = AccessTools.Field(typeof(TerrainDef), nameof(TerrainDef.extraNonDraftedPerceivedPathCost));

			object[] objects = new object[3];
            foreach (var code in instructions)
            {
				//Replace region-mode pathing threshold
				if (!thresholdReplaced && code.opcode == OpCodes.Ldc_I4 && code.OperandIs(100000))
                {
					code.opcode = OpCodes.Ldsfld;
					code.operand = method_regionModeThreshold;
                }
                yield return code;
				if (!searchForObjects && code.opcode == OpCodes.Ldfld && code.OperandIs(field_extraDraftedPerceivedPathCost))
                {
                    searchForObjects = true;
                    continue;
                }

				//Record which local variables extraNonDraftedPerceivedPathCost is using, instead of blindly pulling from the local array ourselves which may jumble
				if (searchForObjects && objectsFound < 3 && code.opcode == OpCodes.Ldloc_S)
                {
					objects[objectsFound++] = code.operand;
					//As of 4/2024, object 0 should be 46, object 1 should be 12, and object 2 should be 43
				}

                if (offset == -1 && code.opcode == OpCodes.Ldfld && code.OperandIs(field_extraNonDraftedPerceivedPathCost))
                {
                    offset = 0;
                    continue;
                }
				
                if (offset > -1 && ++offset == 2)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0); //pawn
                    yield return new CodeInstruction(OpCodes.Ldloc_S, objects[1]); //topGrid
                    yield return new CodeInstruction(OpCodes.Ldloc_S, objects[2]); //topGrid index number
                    yield return new CodeInstruction(OpCodes.Ldelem_Ref); //TerrainDef within the grid
					yield return new CodeInstruction(OpCodes.Ldloc_S, objects[0]); //Pathcost total
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //start position (not used by adjust cost?)
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PathFinder), nameof(PathFinder.map)));
                    yield return new CodeInstruction(OpCodes.Ldloc_S, objects[2]); //cell location
                    yield return new CodeInstruction(OpCodes.Call, typeof(CleanPathfindingUtility).GetMethod(nameof(CleanPathfindingUtility.AdjustCosts)));
                    yield return new CodeInstruction(OpCodes.Stloc_S, objects[0]);

                    ran = true;
                } 
            }
            
            if (!ran) Log.Warning("[Clean Pathfinding] Transpiler could not find target. There may be a mod conflict, or RimWorld updated?");
        }
    }
    
	#endregion

    public static class CleanPathfindingUtility
	{	
		public static Dictionary<ushort, int> terrainCache = new Dictionary<ushort, int>(), terrainCacheOriginalValues = new Dictionary<ushort, int>();
		public static SimpleCurve Custom_DistanceCurve;
		public static MapComponent_DoorPathing cachedComp; //the last map component used by the door cost adjustments. It will be reused if the map hash is the same
		static bool lastFactionHostileCache, lastPawnReversionCache;
		public static int cachedMapID = -1, lastFactionID = -1;
		static int loggedOnTick, calls, lastTerrainCacheCost, lastPawnID;
		static ushort lastTerrainDefID;

		public static void UpdatePathCosts()
		{
			try
			{
				//Reset the cache
				List<string> report = new List<string>();
				foreach (ushort key in terrainCache.Keys.ToList())
				{
					if (terrainCacheOriginalValues.TryGetValue(key, out int originalValue)) terrainCache[key] = originalValue;
					else terrainCache[key] = 0;
				}

				var list = DefDatabase<TerrainDef>.AllDefsListForReading;
				var length = list.Count;
				for (int i = 0; i < length; i++)
				{
					TerrainDef terrainDef = list[i];
				
					ushort index = terrainDef.shortHash;
					//Reset to original value
					if (terrainCache.ContainsKey(index)) terrainDef.extraNonDraftedPerceivedPathCost = terrainCacheOriginalValues[index];
					else continue;
					
					//Attraction to roads
					if (!Setup.safetyNeeded && roadBias > 0 && (terrainDef.tags?.Contains("CleanPath") ?? false))
					{
						terrainDef.extraNonDraftedPerceivedPathCost -= roadBias;
						terrainCache[index] += roadBias;
					}
					else
					{
						//Avoid filth
						if (bias != 0 && terrainDef.generatedFilth != null)
						{
							terrainDef.extraNonDraftedPerceivedPathCost += bias; 
							terrainCache[index] -= bias;
						}

						//Clean but natural terrain bias
						if (naturalBias > 0 && terrainDef.generatedFilth == null && (terrainDef.defName.Contains("_Rough")))
						{
							terrainDef.extraNonDraftedPerceivedPathCost += naturalBias; 
							terrainCache[index] -= naturalBias;
						}
					}

					//Debug
					if (logging && Prefs.DevMode)
					{
						report.Add(terrainDef.defName + ": " + terrainDef.extraNonDraftedPerceivedPathCost);
					}
				}

				//Debug print
				if (logging && Prefs.DevMode)
				{
					report.Sort();
					Log.Message("[Clean Pathfinding] Terrain report:\n" + string.Join("\n - ", report));
				}

				//Reset the extra pathfinding range curve
				Custom_DistanceCurve = new SimpleCurve
				{
					{ new CurvePoint(40f + heuristicAdjuster, 1f), true },
					{ new CurvePoint(120f + (heuristicAdjuster * 3), 3f), true }
				};

				//If playing, update the pathfinders now
				if (Current.ProgramState == ProgramState.Playing) foreach (Map map in Find.Maps) map.pathing.RecalculateAllPerceivedPathCosts();
			}
			catch (System.Exception ex)
			{                
				Log.Error("[Clean Pathfinding] Error processing settings, skipping...\n" + ex);
			}
		}
		static public float AdjustCosts(Pawn pawn, TerrainDef def, float cost, Map map, int index)
        {
			if (pawn == null) goto skipAdjustment;

			//Do not do cost adjustments if...
			bool revert = lastPawnReversionCache;

			//Is not this the last pawn we checked?
			if (pawn.thingIDNumber != lastPawnID)
			{
				lastPawnID = pawn.thingIDNumber;
				var faction = pawn.Faction;

				revert = ((faction == null || pawn.def.race.intelligence == Intelligence.Animal) || // Animal or other entity?
				(!faction.def.isPlayer && IsHostileFast(faction)) || //They are hostile
				(factorCarryingPawn && pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null && pawn.carryTracker.CarriedThing.def.category == ThingCategory.Pawn) ||  //They are carrying someone
				(factorBleeding && pawn.health.hediffSet.cachedBleedRate > 0.1f)); //They are bleeding

				lastPawnReversionCache = revert;
			}

			if (!revert)
			{
				//Factor in door pathing
				if (doorPathing)
				{
					int doorCost = 0;
					if (cachedMapID == map.uniqueID) doorCost = cachedComp.doorCostGrid[index];
					else if (DoorPathingUtility.compCache.TryGetValue(map.uniqueID, out cachedComp))
					{
						cachedMapID = map.uniqueID;
						doorCost = cachedComp.doorCostGrid[index];
					}
					if (doorCost < 0) goto skipAdjustment;
					cost += doorCost;
				}
				//...And then light pathing
				if (factorLight && GameGlowAtFast(map, index) < 0.3f) cost += darknessPenalty;
			}
			//Revert if needed, check if cache is available
			else if (def.shortHash == lastTerrainDefID) cost += lastTerrainCacheCost;
			//If not, use and set...
			else
			{
				lastTerrainDefID = def.shortHash;
				if (terrainCache.TryGetValue(def.shortHash, out lastTerrainCacheCost))
				{
					//Double-revert back to 0 if this is a clean path (value returned as greater than 0) and this is our faction, meaning it's probably a hauling animal
					if (lastTerrainCacheCost > 0 && pawn.factionInt != null && pawn.factionInt.def.isPlayer) lastTerrainCacheCost = 0;
					cost += lastTerrainCacheCost;
				}
				else lastTerrainCacheCost = 0; //Record any terrain defs that are not modified to avoid looking them up again
			}
			
			//Logging and debugging stuff
			if (logging && Verse.Prefs.DevMode)
			{
				++calls;
				if (Current.gameInt.tickManager.ticksGameInt != loggedOnTick)
				{
					loggedOnTick = Current.gameInt.tickManager.ticksGameInt;
					if (calls != 0) Log.Message("[Clean Pathfinding] Calls last pathfinding: " + calls);
					calls = 0;
				}
				if (cost < 0) cost = 0;
				var cell = map.cellIndices.IndexToCell(index);
				if (!map.debugDrawer.debugCells.Any(x => x.c == cell)) map.debugDrawer.FlashCell(cell, cost, cost.ToString());
			}
			skipAdjustment:
			if (cost < 0) return 0;
			return cost;

			#region embedded methods
			bool IsHostileFast(Faction faction)
			{
				//Check and set cache
				if (Current.gameInt.tickManager.ticksGameInt % 600 == 0) lastFactionID = -1; // Reset every 10th second
				if (faction.loadID == lastFactionID) return lastFactionHostileCache;
				else lastFactionID = faction.loadID;

				//Look through their relationships table and look up the player faction, then record to cache
				var relations = faction.relations;
				for (int i = relations.Count; i-- > 0;)
				{
					var tmp = relations[i];
					if (tmp.other == Current.gameInt.worldInt.factionManager.ofPlayer)
					{
						lastFactionHostileCache = tmp.kind == FactionRelationKind.Hostile;
						break;
					}
				}
				return lastFactionHostileCache;
			}

			float GameGlowAtFast(Map map, int index)
			{
				float daylight = 0f;
				//If there's no roof, they're outside, so factory the daylight
				if (map.roofGrid.roofGrid[index] == null)
				{
					daylight = map.skyManager.curSkyGlowInt;
					if (daylight == 1f) return 1f;
				}
#if v1_5

#else
				ColorInt color = map.glowGrid.glowGrid[index];
#endif
				UnityEngine.Color32 color = map.glowGrid.VisualGlowAt(index);
				if (color.a == 1) return 1;

				return (float)(color.r + color.g + color.b) * 0.0047058823529412f; //n / 3f / 255f * 3.6f pre-computed, since I guess the assembler doesn't optimize this
			}

#endregion
        }
	}
}