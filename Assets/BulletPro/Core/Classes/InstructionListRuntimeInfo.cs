﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	public struct LoopInfo
	{
		public int indexOfBeginInstruction;
		public int iterationsRemaining;
		public int iterationsFinished;
		public bool endless;
	}

	public enum InstructionResult { None, InterruptedItself, RebootedItself }

	// Struct held by PatternRuntimeInfo, handles logic and loops
	public struct InstructionListRuntimeInfo
	{
		// instruction-related vars
		public int indexOfNextInstruction;
		public bool isDone;
		float waitTimeLeft;
		float instructionDelay;
		int endLoopTagsWanted; // helps skip everything until we stumble across the end of current loop. Skipping means avoiding a loop that user wants played "0 times".
		public Stack<LoopInfo> loops, tempStack;
		bool shouldBreakLoopNow; // avoids infinite endless loops without a Wait instruction in it
		bool hadWaitBlockDuringLoop; // checks whether we saw a Wait block during an endless loop
		public SolvedInstruction[] instructions;
		public PatternInstructionList dynamicInstructions; // used as fallback if a parameter has to be rerolled at every instruction

		// allow data transmission through structs
		public Bullet bullet;
		public int patternIndex;
		public int indexOfThisList;

		// regarding rerolling of dynamic parameters
		int numberOfRerolls;
		bool recentlyChangedSeed;

		public void Init(SolvedInstruction[] newInstructions, PatternInstructionList dynamicInstList, Bullet emitter, int indexOfPattern, int indexOfList, float instDelay)
		{
			isDone = false;
			indexOfNextInstruction = 0;
			loops = new Stack<LoopInfo>();
			tempStack = new Stack<LoopInfo>();
			loops.Clear();
			tempStack.Clear();
			endLoopTagsWanted = 0;
			waitTimeLeft = 0;
			instructionDelay = instDelay;
			if (instructionDelay < 0) instructionDelay = 0;
			shouldBreakLoopNow = false;
			hadWaitBlockDuringLoop = false;

			dynamicInstructions = dynamicInstList;

			bullet = emitter;
			patternIndex = indexOfPattern;
			indexOfThisList = indexOfList;
			instructions = newInstructions;

			numberOfRerolls = 1;
			recentlyChangedSeed = false;

			bool empty = false;
			if (instructions == null) empty = true;
			else if (instructions.Length == 0) empty = true;
			if (empty) isDone = true;	

			if (bullet == null)	isDone = true;
		}

		public void Update()
		{
			if (isDone) return;

			waitTimeLeft -= Time.deltaTime;

			shouldBreakLoopNow = false;

			// keep executing instructions one by one in the same frame until one of them is a Wait()
			while (waitTimeLeft <= 0)
			{
				// end of behaviour : auto-repair missing EndLoop tags at the end of the instruction list
				if (indexOfNextInstruction == instructions.Length)
				{
					while ((indexOfNextInstruction == instructions.Length) && (loops.Count > 0))
					{
						if (EndLoop())
							indexOfNextInstruction++;
					}

					if (indexOfNextInstruction == instructions.Length)
					{
						isDone = true;
						break;
					}
				}

				// If we just exited an endless loop with no Wait block, this bool is true and we break the loop, which means "wait for next frame".
				if (shouldBreakLoopNow) break;

				// actual instruction call
				InstructionResult res = DoInstruction(indexOfNextInstruction);

				// Hard-break the loop if the pattern rebooted itself, without even incrementing the instruction index.
				if (res == InstructionResult.RebootedItself)
					break;

				// Apply base instruction delay, if any
				if (instructionDelay > 0)
				if (bullet)
				if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].delaylessInstructions != null)
				if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].delaylessInstructions.Contains(instructions[indexOfNextInstruction].instructionType))
				{
					waitTimeLeft += instructionDelay;
					hadWaitBlockDuringLoop = true;
				}

				indexOfNextInstruction++;

				// Still hard-break the loop if the pattern paused itself without rebooting, but this time we incremented the index.
				if (res == InstructionResult.InterruptedItself) break;

				if (shouldBreakLoopNow) break;
			}
		}

		// A giant conditional structure that executes the instruction among the 100+ possible types.
		// Returns whether the pattern interrupted/rebooted itself due to a flow function.
		InstructionResult DoInstruction(int index)
		{
			SolvedInstruction ip = instructions[index];
			PatternInstruction rawInst = dynamicInstructions[index];
			if (!ip.enabled) return InstructionResult.None;

			// if we're currently skipping a loop, that means we only want to process BeginLoop and EndLoop blocks.
			if (endLoopTagsWanted > 0)
			{
				if (ip.instructionType == PatternInstructionType.BeginLoop) endLoopTagsWanted++; // handle nesting
				if (ip.instructionType == PatternInstructionType.EndLoop) EndLoop();

				return InstructionResult.None;
			}

			#region main
			
			// Wait
			if (ip.instructionType == PatternInstructionType.Wait)
			{
				if (IsRerollNecessary(ip, 0))
					instructions[index].waitTime = bullet.dynamicSolver.SolveDynamicFloat(rawInst.waitTime, 1258491 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].compensateSmallWaits)
					waitTimeLeft += instructions[index].waitTime;
				else waitTimeLeft = instructions[index].waitTime;
				if (instructions[index].waitTime > 0) hadWaitBlockDuringLoop = true;
			}

			// Shoot
			else if (ip.instructionType == PatternInstructionType.Shoot)
			{
				if (IsRerollNecessary(ip, 0))				
					ip.shot = bullet.dynamicSolver.SolveDynamicShot(rawInst.shot, 9090478 * (numberOfRerolls++), ParameterOwner.Pattern);

				float timeOffset = 0;
				if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].deltaTimeDisplacement)
					timeOffset = waitTimeLeft;
				bullet.modulePatterns.patternRuntimeInfo[patternIndex].Shoot(ip.shot, timeOffset);
				
				bullet.emitter.shootInstruction.Invoke();
			}

			// Begin Loop
			else if (ip.instructionType == PatternInstructionType.BeginLoop)
			{
				if (index == instructions.Length - 1) return InstructionResult.None;

				if (IsRerollNecessary(ip, 0))
					ip.iterations = bullet.dynamicSolver.SolveDynamicInt(rawInst.iterations, 9874120 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				LoopInfo li = new LoopInfo();
				li.indexOfBeginInstruction = index;
				li.endless = ip.endless;
				li.iterationsRemaining = ip.iterations;
				li.iterationsFinished = 0;
				loops.Push(li);

				// if it's a non-endless loop that must be done zero times, it means we're skipping it.
				if (li.iterationsRemaining < 1 && !li.endless)
					endLoopTagsWanted = 1;

				// if it's an endless loop, we need to start checking for Wait blocks
				if (li.endless)
					hadWaitBlockDuringLoop = false;
			}

			// End Loop
			else if (ip.instructionType == PatternInstructionType.EndLoop)
			{
				EndLoop();
			}

			// Play Audio
			else if (ip.instructionType == PatternInstructionType.PlayAudio)
			{
				if (IsRerollNecessary(ip, 0))
					ip.audioClip = bullet.dynamicSolver.SolveDynamicObjectReference(rawInst.audioClip, 6330581 * (numberOfRerolls++), ParameterOwner.Pattern) as AudioClip;

				if (ip.audioClip != null)
					bullet.audioManager.PlayLocal(ip.audioClip);
			}

			// Play VFX
			else if (ip.instructionType == PatternInstructionType.PlayVFX)
			{
				if (ip.vfxFilterType == VFXFilterType.Index)
				{
					if (IsRerollNecessary(ip, 0))
						ip.vfxIndex = bullet.dynamicSolver.SolveDynamicInt(rawInst.vfxIndex, 9874721 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.moduleVFX.PlayVFX(ip.vfxIndex);
				}
				else // if VFXFilterType.Tag
				{
					if (IsRerollNecessary(ip, 0))
						ip.vfxTag = bullet.dynamicSolver.SolveDynamicString(rawInst.vfxTag, 2890600 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.moduleVFX.PlayVFX(ip.vfxTag);
				}
			}

			// Stop VFX
			else if (ip.instructionType == PatternInstructionType.StopVFX)
			{
				if (ip.vfxFilterType == VFXFilterType.Index)
				{
					if (IsRerollNecessary(ip, 0))
						ip.vfxIndex = bullet.dynamicSolver.SolveDynamicInt(rawInst.vfxIndex, 3254133 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.moduleVFX.StopVFX(ip.vfxIndex);
				}
				else // if VFXFilterType.Tag
				{
					if (IsRerollNecessary(ip, 0))
						ip.vfxTag = bullet.dynamicSolver.SolveDynamicString(rawInst.vfxTag, 8796501 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.moduleVFX.StopVFX(ip.vfxTag);
				}
			}

			// Die
			else if (ip.instructionType == PatternInstructionType.Die)
			{
				bullet.Die(true);
			}

			#endregion

			#region transform

			#region transform / misc
			
			// Enable Movement
			else if (ip.instructionType == PatternInstructionType.EnableMovement)
			{
				bullet.moduleMovement.Enable();
			}

			// Disable Movement
			else if (ip.instructionType == PatternInstructionType.DisableMovement)
			{
				bullet.moduleMovement.Disable();
			}

			// Attach To Emitter
			else if (ip.instructionType == PatternInstructionType.AttachToEmitter)
			{
				if (bullet.subEmitter) bullet.self.SetParent(bullet.subEmitter.self, true);
				else if (bullet.emitter)
					if (bullet.emitter.patternOrigin)
						bullet.self.SetParent(bullet.emitter.patternOrigin, true);
			}

			// Detach From Emitter
			else if (ip.instructionType == PatternInstructionType.DetachFromEmitter)
			{
				bullet.self.SetParent(null);
			}

			#endregion

			#region transform / position

			// Move (world)
			else if (ip.instructionType == PatternInstructionType.TranslateGlobal)
			{
				if (IsRerollNecessary(ip, 0))
					ip.globalMovement = bullet.dynamicSolver.SolveDynamicVector2(rawInst.globalMovement, 6417182 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.Translate(ip.globalMovement.x, ip.globalMovement.y, 0, Space.World);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4568567 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 14145861 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionTranslateGlobal(
						bullet, ip.instructionDuration, ip.operationCurve, ip.globalMovement));
				}
			}

			// Move (self)
			else if (ip.instructionType == PatternInstructionType.TranslateLocal)
			{
				if (IsRerollNecessary(ip, 0))
					ip.localMovement = bullet.dynamicSolver.SolveDynamicVector2(rawInst.localMovement, 4568935 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.Translate(ip.localMovement.x, ip.localMovement.y, 0, Space.Self);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4505567 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6371151 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionTranslateLocal(
						bullet, ip.instructionDuration, ip.operationCurve, ip.localMovement));
				}		
			}

			// Set Position (world)
			else if (ip.instructionType == PatternInstructionType.SetWorldPosition)
			{
				if (IsRerollNecessary(ip, 0))
					ip.globalMovement = bullet.dynamicSolver.SolveDynamicVector2(rawInst.globalMovement, 4567951 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.SetGlobalPosition(ip.globalMovement);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4877411 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2560693 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionPositionSetGlobal(
						bullet, ip.instructionDuration, ip.operationCurve, ip.globalMovement));
				}
			}

			// Set Position (local)
			else if (ip.instructionType == PatternInstructionType.SetLocalPosition)
			{
				if (IsRerollNecessary(ip, 0))
					ip.localMovement = bullet.dynamicSolver.SolveDynamicVector2(rawInst.localMovement, 4785651 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.SetLocalPosition(ip.localMovement);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1584003 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 9899461 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionTranslateLocal(
						bullet, ip.instructionDuration, ip.operationCurve, ip.localMovement));
				}		
			}

			// Set Speed
			else if (ip.instructionType == PatternInstructionType.SetSpeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.speedValue = bullet.dynamicSolver.SolveDynamicFloat(rawInst.speedValue, 4756641 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.baseSpeed = ip.speedValue;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8777763 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1254785 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionSpeedSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.speedValue));
				}		
			}

			// Multiply Speed
			else if (ip.instructionType == PatternInstructionType.MultiplySpeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 4847743 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.baseSpeed *= ip.factor;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5621471 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6885923 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionSpeedMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.factor));
				}		
			}

			#endregion

			#region transform / rotation

			// Rotate
			else if (ip.instructionType == PatternInstructionType.Rotate)
			{
				if (IsRerollNecessary(ip, 0))
					ip.rotation = bullet.dynamicSolver.SolveDynamicFloat(rawInst.rotation, 4141021 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.Rotate(ip.rotation);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8997451 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2015901 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.microActions.Add(new MicroActionRotate(
						bullet, ip.instructionDuration, ip.operationCurve, ip.rotation));
				}
			}

			// Set Rotation (world)
			else if (ip.instructionType == PatternInstructionType.SetWorldRotation)
			{
				if (IsRerollNecessary(ip, 0))
					ip.rotation = bullet.dynamicSolver.SolveDynamicFloat(rawInst.rotation, 4755433 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.SetGlobalRotation(ip.rotation);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1420019 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1450007 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionRotationSetGlobal(
						bullet, ip.instructionDuration, ip.operationCurve, ip.rotation));
				}
			}

			// Set Rotation (local)
			else if (ip.instructionType == PatternInstructionType.SetLocalRotation)
			{
				if (IsRerollNecessary(ip, 0))
					ip.rotation = bullet.dynamicSolver.SolveDynamicFloat(rawInst.rotation, 1369591 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.SetLocalRotation(ip.rotation);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1255401 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1723749 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionRotationSetLocal(
						bullet, ip.instructionDuration, ip.operationCurve, ip.rotation));
				}
			}

			// Set Angular Speed
			else if (ip.instructionType == PatternInstructionType.SetAngularSpeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.speedValue = bullet.dynamicSolver.SolveDynamicFloat(rawInst.speedValue, 4565287 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.baseAngularSpeed = ip.speedValue;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 2565855 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1101171 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionAngularSpeedSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.speedValue));
				}
			}

			// Multiply Angular Speed
			else if (ip.instructionType == PatternInstructionType.MultiplyAngularSpeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 9654681 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.baseAngularSpeed *= ip.factor;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1353843 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4170805 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionAngularSpeedMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.factor));
				}
			}

			#endregion

			#region transform / scale

			// Set Scale
			else if (ip.instructionType == PatternInstructionType.SetScale)
			{
				if (IsRerollNecessary(ip, 0))
					ip.scaleValue = bullet.dynamicSolver.SolveDynamicFloat(rawInst.scaleValue, 4235753 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.baseScale = ip.scaleValue;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5951423 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4025863 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionScaleSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.scaleValue));
				}
			}

			// Multiply Scale
			else if (ip.instructionType == PatternInstructionType.MultiplyScale)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 1485207 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleMovement.baseScale *= ip.factor;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 9582461 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4247581 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionScaleMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.factor));
				}
			}

			#endregion

			#endregion

			#region homing

			// Enable Homing
			else if (ip.instructionType == PatternInstructionType.EnableHoming)
			{
				bullet.moduleHoming.Enable();
			}

			// Disable Homing
			else if (ip.instructionType == PatternInstructionType.DisableHoming)
			{
				bullet.moduleHoming.Disable();
			}

			// Turn to Target
			else if (ip.instructionType == PatternInstructionType.TurnToTarget)
			{
				if (IsRerollNecessary(ip, 0))
					ip.turnIntensity = bullet.dynamicSolver.SolveDynamicFloat(rawInst.turnIntensity, 4714581 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleHoming.LookAtTarget(ip.turnIntensity);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1542879 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1047457 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionTurnToTarget(
						bullet, ip.instructionDuration, ip.operationCurve, ip.turnIntensity));
				}		
			}

			// Change Target
			else if (ip.instructionType == PatternInstructionType.ChangeTarget)
			{
				if (IsRerollNecessary(ip, 0))
					ip.preferredTarget = (PreferredTarget)bullet.dynamicSolver.SolveDynamicEnum(rawInst.preferredTarget, 4556249 * (numberOfRerolls++), ParameterOwner.Pattern);

				bullet.moduleHoming.RefreshTarget(ip.preferredTarget);
			}

			// Set Homing Speed
			else if (ip.instructionType == PatternInstructionType.SetHomingSpeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.speedValue = bullet.dynamicSolver.SolveDynamicFloat(rawInst.speedValue, 4578539 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleHoming.homingAngularSpeed = ip.speedValue;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1010143 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4258653 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionHomingSpeedSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.speedValue));
				}
			}

			// Multiply Homing Speed
			else if (ip.instructionType == PatternInstructionType.MultiplyHomingSpeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 4758511 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleHoming.homingAngularSpeed *= ip.factor;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 6557581 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4385679 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionHomingSpeedMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.factor));
				}
			}

			// Change Homing Tag
			else if (ip.instructionType == PatternInstructionType.ChangeHomingTag)
			{
				if (IsRerollNecessary(ip, 0))
					ip.collisionTag = bullet.dynamicSolver.SolveDynamicString(rawInst.collisionTag, 1204509 * (numberOfRerolls++), ParameterOwner.Pattern);

				bullet.moduleHoming.homingTags[ip.collisionTag] = (ip.collisionTagAction == CollisionTagAction.Add);
			}

			#endregion

			#region curves - controls

			// Enable Curve
			else if (ip.instructionType == PatternInstructionType.EnableCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.enabled = true;
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.enabled = true;
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.enabled = true;
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.enabled = true;
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.enabled = true;
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.enabled = true;
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.enabled = true;
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.enabled = true;
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.enabled = true;
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.enabled = true;
			}

			// Disable Curve
			else if (ip.instructionType == PatternInstructionType.EnableCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.enabled = false;
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.enabled = false;
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.enabled = false;
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.enabled = false;
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.enabled = false;
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.enabled = false;
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.enabled = false;
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.enabled = false;
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.enabled = false;
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.enabled = false;
			}

			// Play Curve
			else if (ip.instructionType == PatternInstructionType.PlayCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.Play();
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.Play();
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.Play();
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.Play();
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.Play();
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.Play();
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.Play();
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.Play();
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.Play();
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.Play();
			}

			// Pause Curve
			else if (ip.instructionType == PatternInstructionType.PauseCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.Pause();
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.Pause();
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.Pause();
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.Pause();
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.Pause();
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.Pause();
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.Pause();
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.Pause();
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.Pause();
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.Pause();
			}

			// Rewind Curve
			else if (ip.instructionType == PatternInstructionType.RewindCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.Rewind();
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.Rewind();
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.Rewind();
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.Rewind();
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.Rewind();
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.Rewind();
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.Rewind();
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.Rewind();
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.Rewind();
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.Rewind();
			}

			// Reset Curve
			else if (ip.instructionType == PatternInstructionType.ResetCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.Reset();
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.Reset();
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.Reset();
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.Reset();
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.Reset();
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.Reset();
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.Reset();
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.Reset();
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.Reset();
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.Reset();
			}

			// Stop Curve
			else if (ip.instructionType == PatternInstructionType.StopCurve)
			{
				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.Stop();
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.Stop();
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.Stop();
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.Stop();
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.Stop();
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.Stop();
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.Stop();
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.Stop();
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.Stop();
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.Stop();
			}

			#endregion

			#region curves - values

			// Set Curve
			else if (ip.instructionType == PatternInstructionType.SetCurveValue)
			{
				if (IsRerollNecessary(ip, 0))
					ip.newCurveValue = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.newCurveValue, 1593461 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.curve = ip.newCurveValue;
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.curve = ip.newCurveValue;
			}

			// Set Wrap Mode
			else if (ip.instructionType == PatternInstructionType.SetWrapMode)
			{
				if (IsRerollNecessary(ip, 0))
					ip.newWrapMode = (WrapMode)bullet.dynamicSolver.SolveDynamicEnum(rawInst.newWrapMode, 6603417 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.curveAffected == PatternCurveType.Alpha)
					bullet.moduleRenderer.alphaOverLifetime.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.Color)
					bullet.moduleRenderer.colorOverLifetime.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.Homing)
					bullet.moduleHoming.homingOverLifetime.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.Speed)
					bullet.moduleMovement.speedOverLifetime.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					bullet.moduleMovement.angularSpeedOverLifetime.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.Scale)
					bullet.moduleMovement.scaleOverLifetime.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.AnimX)
					bullet.moduleMovement.moveXFromAnim.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.AnimY)
					bullet.moduleMovement.moveYFromAnim.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.AnimAngle)
					bullet.moduleMovement.rotateFromAnim.wrapMode = ip.newWrapMode;
				else if (ip.curveAffected == PatternCurveType.AnimScale)
					bullet.moduleMovement.scaleFromAnim.wrapMode = ip.newWrapMode;
			}

			// Set Period
			else if (ip.instructionType == PatternInstructionType.SetPeriod)
			{
				float newValue = 0;
				if (ip.newPeriodType == CurvePeriodType.BulletTotalLifetime)
					newValue = bullet.moduleLifespan.lifespan;
				else if (ip.newPeriodType == CurvePeriodType.RemainingLifetime)
					newValue = bullet.moduleLifespan.lifespan - bullet.timeSinceAlive;
				else if (ip.newPeriodType == CurvePeriodType.FixedValue)
				{
					if (IsRerollNecessary(ip, 0))
						ip.newPeriodValue = bullet.dynamicSolver.SolveDynamicFloat(rawInst.newPeriodValue, 4145821 * (numberOfRerolls++), ParameterOwner.Pattern);
					newValue = ip.newPeriodValue;
				}

				if (ip.instructionTiming == InstructionTiming.Progressively)
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4578639 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4152507 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCurvePeriodSet(
						bullet, ip.instructionDuration, ip.operationCurve, newValue, ip.curveAffected));
				}
				else
				{
					if (ip.curveAffected == PatternCurveType.Alpha)
					{
						bullet.moduleRenderer.alphaOverLifetime.period = newValue;
						bullet.moduleRenderer.alphaOverLifetime.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.Color)
					{
						bullet.moduleRenderer.colorOverLifetime.period = newValue;
						bullet.moduleRenderer.colorOverLifetime.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.Homing)
					{
						bullet.moduleHoming.homingOverLifetime.period = newValue;
						bullet.moduleHoming.homingOverLifetime.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.Speed)
					{
						bullet.moduleMovement.speedOverLifetime.period = newValue;
						bullet.moduleMovement.speedOverLifetime.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					{
						bullet.moduleMovement.angularSpeedOverLifetime.period = newValue;
						bullet.moduleMovement.angularSpeedOverLifetime.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.Scale)
					{
						bullet.moduleMovement.scaleOverLifetime.period = newValue;
						bullet.moduleMovement.scaleOverLifetime.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.AnimX)
					{
						bullet.moduleMovement.moveXFromAnim.period = newValue;
						bullet.moduleMovement.moveXFromAnim.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.AnimY)
					{
						bullet.moduleMovement.moveYFromAnim.period = newValue;
						bullet.moduleMovement.moveYFromAnim.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.AnimAngle)
					{	
						bullet.moduleMovement.rotateFromAnim.period = newValue;
						bullet.moduleMovement.rotateFromAnim.periodIsLifespan = false;
					}
					else if (ip.curveAffected == PatternCurveType.AnimScale)
					{
						bullet.moduleMovement.scaleFromAnim.period = newValue;
						bullet.moduleMovement.scaleFromAnim.periodIsLifespan = false;
					}
				}
			}

			// Multiply Period
			else if (ip.instructionType == PatternInstructionType.MultiplyPeriod)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 2666677 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Progressively)
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4251063 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 5899477 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCurvePeriodMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.factor, ip.curveAffected));
				}
				else
				{
					if (ip.curveAffected == PatternCurveType.Alpha)
					{
						if (bullet.moduleRenderer.alphaOverLifetime.periodIsLifespan)
						{
							bullet.moduleRenderer.alphaOverLifetime.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleRenderer.alphaOverLifetime.periodIsLifespan = false;
						}
						else bullet.moduleRenderer.alphaOverLifetime.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.Color)
					{
						if (bullet.moduleRenderer.colorOverLifetime.periodIsLifespan)
						{
							bullet.moduleRenderer.colorOverLifetime.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleRenderer.colorOverLifetime.periodIsLifespan = false;
						}
						else bullet.moduleRenderer.colorOverLifetime.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.Homing)
					{
						if (bullet.moduleHoming.homingOverLifetime.periodIsLifespan)
						{
							bullet.moduleHoming.homingOverLifetime.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleHoming.homingOverLifetime.periodIsLifespan = false;
						}
						else bullet.moduleHoming.homingOverLifetime.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.Speed)
					{
						if (bullet.moduleMovement.speedOverLifetime.periodIsLifespan)
						{
							bullet.moduleMovement.speedOverLifetime.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.speedOverLifetime.periodIsLifespan = false;
						}
						else bullet.moduleMovement.speedOverLifetime.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.AngularSpeed)
					{
						if (bullet.moduleMovement.angularSpeedOverLifetime.periodIsLifespan)
						{
							bullet.moduleMovement.angularSpeedOverLifetime.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.angularSpeedOverLifetime.periodIsLifespan = false;
						}
						else bullet.moduleMovement.angularSpeedOverLifetime.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.Scale)
					{
						if (bullet.moduleMovement.scaleOverLifetime.periodIsLifespan)
						{
							bullet.moduleMovement.scaleOverLifetime.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.scaleOverLifetime.periodIsLifespan = false;
						}
						else bullet.moduleMovement.scaleOverLifetime.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.AnimX)
					{
						if (bullet.moduleMovement.moveXFromAnim.periodIsLifespan)
						{
							bullet.moduleMovement.moveXFromAnim.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.moveXFromAnim.periodIsLifespan = false;
						}
						else bullet.moduleMovement.moveXFromAnim.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.AnimY)
					{
						if (bullet.moduleMovement.moveYFromAnim.periodIsLifespan)
						{
							bullet.moduleMovement.moveYFromAnim.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.moveYFromAnim.periodIsLifespan = false;
						}
						else bullet.moduleMovement.moveYFromAnim.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.AnimAngle)
					{	
						if (bullet.moduleMovement.rotateFromAnim.periodIsLifespan)
						{
							bullet.moduleMovement.rotateFromAnim.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.rotateFromAnim.periodIsLifespan = false;
						}
						else bullet.moduleMovement.rotateFromAnim.period *= ip.factor;
					}
					else if (ip.curveAffected == PatternCurveType.AnimScale)
					{
						if (bullet.moduleMovement.scaleFromAnim.periodIsLifespan)
						{
							bullet.moduleMovement.scaleFromAnim.period = bullet.moduleLifespan.lifespan * ip.factor;
							bullet.moduleMovement.scaleFromAnim.periodIsLifespan = false;
						}
						else bullet.moduleMovement.scaleFromAnim.period *= ip.factor;
					}
				}
			}

			// Set Curve Raw Time
			else if (ip.instructionType == PatternInstructionType.SetCurveRawTime)
			{
				if (IsRerollNecessary(ip, 0))
					ip.curveRawTime = bullet.dynamicSolver.SolveDynamicFloat(rawInst.curveRawTime, 4174211 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Progressively)
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5423657 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4211029 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCurveRawTimeSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.curveRawTime, ip.curveAffected));
				}
				else
				{
					if (ip.curveAffected == PatternCurveType.Alpha)
						bullet.moduleRenderer.alphaOverLifetime.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.Color)
						bullet.moduleRenderer.colorOverLifetime.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.Homing)
						bullet.moduleHoming.homingOverLifetime.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.Speed)
						bullet.moduleMovement.speedOverLifetime.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.AngularSpeed)
						bullet.moduleMovement.angularSpeedOverLifetime.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.Scale)
						bullet.moduleMovement.scaleOverLifetime.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.AnimX)
						bullet.moduleMovement.moveXFromAnim.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.AnimY)
						bullet.moduleMovement.moveYFromAnim.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.AnimAngle)
						bullet.moduleMovement.rotateFromAnim.SetRawTime(ip.curveRawTime);
					else if (ip.curveAffected == PatternCurveType.AnimScale)
						bullet.moduleMovement.scaleFromAnim.SetRawTime(ip.curveRawTime);
				}
			}

			// Set Curve Ratio
			else if (ip.instructionType == PatternInstructionType.SetCurveRatio)
			{
				if (IsRerollNecessary(ip, 0))
					ip.curveTime = bullet.dynamicSolver.SolveDynamicSlider01(rawInst.curveTime, 4125637 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Progressively)
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4256899 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4575103 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCurveRatioSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.curveTime, ip.curveAffected));
				}
				else
				{
					if (ip.curveAffected == PatternCurveType.Alpha)
						bullet.moduleRenderer.alphaOverLifetime.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.Color)
						bullet.moduleRenderer.colorOverLifetime.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.Homing)
						bullet.moduleHoming.homingOverLifetime.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.Speed)
						bullet.moduleMovement.speedOverLifetime.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.AngularSpeed)
						bullet.moduleMovement.angularSpeedOverLifetime.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.Scale)
						bullet.moduleMovement.scaleOverLifetime.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.AnimX)
						bullet.moduleMovement.moveXFromAnim.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.AnimY)
						bullet.moduleMovement.moveYFromAnim.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.AnimAngle)
						bullet.moduleMovement.rotateFromAnim.SetRatio(ip.curveTime);
					else if (ip.curveAffected == PatternCurveType.AnimScale)
						bullet.moduleMovement.scaleFromAnim.SetRatio(ip.curveTime);
				}
			}

			#endregion

			#region graphics

			// Turn Visible
			else if (ip.instructionType == PatternInstructionType.TurnVisible)
			{
				bullet.moduleRenderer.Enable();
			}

			// Turn Invisible
			else if (ip.instructionType == PatternInstructionType.TurnInvisible)
			{
				bullet.moduleRenderer.Disable();
			}

			// Play Animation
			else if (ip.instructionType == PatternInstructionType.PlayAnimation)
			{
				bullet.moduleRenderer.animated = true;
			}

			// Pause Animation
			else if (ip.instructionType == PatternInstructionType.PauseAnimation)
			{
				bullet.moduleRenderer.animated = false;
			}

			// Reboot Animation
			else if (ip.instructionType == PatternInstructionType.RebootAnimation)
			{
				bullet.moduleRenderer.SetFrame(0);
				bullet.moduleRenderer.animated = true;
			}

			#endregion

			#region color

			// Set Color
			else if (ip.instructionType == PatternInstructionType.SetColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.color = bullet.dynamicSolver.SolveDynamicColor(rawInst.color, 1472471 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor = ip.color;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 7008401 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6809099 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionColorReplace(
						bullet, ip.instructionDuration, ip.operationCurve, ip.color));
				}
			}

			// Add Color
			else if (ip.instructionType == PatternInstructionType.AddColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.color = bullet.dynamicSolver.SolveDynamicColor(rawInst.color, 4235373 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor += ip.color;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1252943 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4150071 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionColorAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.color));
				}
			}

			// Multiply Color
			else if (ip.instructionType == PatternInstructionType.MultiplyColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.color = bullet.dynamicSolver.SolveDynamicColor(rawInst.color, 3600417 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor *= ip.color;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4856701 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 8564103 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionColorMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.color));
				}
			}

			// Overlay Color
			else if (ip.instructionType == PatternInstructionType.OverlayColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.color = bullet.dynamicSolver.SolveDynamicColor(rawInst.color, 4567811 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor = bullet.moduleRenderer.BlendColors(bullet.moduleRenderer.startColor, ip.color, ColorBlend.AlphaBlend);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4165427 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 5462133 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionColorOverlay(
						bullet, ip.instructionDuration, ip.operationCurve, ip.color));
				}
			}

			// Set Alpha
			else if (ip.instructionType == PatternInstructionType.SetAlpha)
			{
				if (IsRerollNecessary(ip, 0))
					ip.alpha = bullet.dynamicSolver.SolveDynamicSlider01(rawInst.alpha, 5478133 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor = new Color(
						bullet.moduleRenderer.startColor.r,
						bullet.moduleRenderer.startColor.g,
						bullet.moduleRenderer.startColor.b,
						Mathf.Clamp01(ip.alpha));
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8787143 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 8545211 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionAlphaSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.alpha));
				}
			}

			// Add Alpha
			else if (ip.instructionType == PatternInstructionType.AddAlpha)
			{
				if (IsRerollNecessary(ip, 0))
					ip.alpha = bullet.dynamicSolver.SolveDynamicSlider01(rawInst.alpha, 4175255 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor = new Color(
						bullet.moduleRenderer.startColor.r,
						bullet.moduleRenderer.startColor.g,
						bullet.moduleRenderer.startColor.b,
						Mathf.Clamp01(bullet.moduleRenderer.startColor.a + ip.alpha));
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5674711 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2539957 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionAlphaAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.alpha));
				}
			}

			// Multiply Alpha
			else if (ip.instructionType == PatternInstructionType.MultiplyAlpha)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 5004177 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.startColor = new Color(
						bullet.moduleRenderer.startColor.r,
						bullet.moduleRenderer.startColor.g,
						bullet.moduleRenderer.startColor.b,
						Mathf.Clamp01(bullet.moduleRenderer.startColor.a * ip.factor));
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5000419 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 5336817 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionAlphaMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.factor));
				}
			}

			// Set Lifetime Gradient
			else if (ip.instructionType == PatternInstructionType.SetLifetimeGradient)
			{
				if (IsRerollNecessary(ip, 0))
					ip.gradient = bullet.dynamicSolver.SolveDynamicGradient(rawInst.gradient, 2456547 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleRenderer.colorEvolution = ip.gradient;
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 9825633 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 5418711 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionGradientSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.gradient));
				}
			}

			#endregion

			#region collision

			// Enable Collision
			else if (ip.instructionType == PatternInstructionType.EnableCollision)
			{
				bullet.moduleCollision.Enable();
			}

			// Disable Collision
			else if (ip.instructionType == PatternInstructionType.DisableCollision)
			{
				bullet.moduleCollision.Disable();
			}

			// Change Collision Tag
			else if (ip.instructionType == PatternInstructionType.ChangeCollisionTag)
			{
				if (IsRerollNecessary(ip, 0))
					ip.collisionTag = bullet.dynamicSolver.SolveDynamicString(rawInst.collisionTag, 6522111 * (numberOfRerolls++), ParameterOwner.Pattern);

				bullet.moduleCollision.collisionTags[ip.collisionTag] = (ip.collisionTagAction == CollisionTagAction.Add);
			}

			#endregion

			#region pattern flow

			// Play Pattern
			else if (ip.instructionType == PatternInstructionType.PlayPattern)
			{
				if (IsRerollNecessary(ip, 0))
					ip.patternTag = bullet.dynamicSolver.SolveDynamicString(rawInst.patternTag, 5689401 * (numberOfRerolls++), ParameterOwner.Pattern);

				bullet.modulePatterns.Play(ip.patternTag);
			}

			// Pause Pattern
			else if (ip.instructionType == PatternInstructionType.PausePattern)
			{
				if (ip.patternControlTarget == PatternControlTarget.ThisPattern)
				{
					bullet.modulePatterns.Pause(patternIndex);
					return InstructionResult.InterruptedItself;
				}
				else
				{
					if (IsRerollNecessary(ip, 0))
						ip.patternTag = bullet.dynamicSolver.SolveDynamicString(rawInst.patternTag, 7456003 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.modulePatterns.Pause(ip.patternTag);

					if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].HasTag(ip.patternTag))
						return InstructionResult.InterruptedItself;
				}
			}

			// Stop Pattern
			else if (ip.instructionType == PatternInstructionType.StopPattern)
			{
				if (ip.patternControlTarget == PatternControlTarget.ThisPattern)
				{
					bullet.modulePatterns.Stop(patternIndex);
					return InstructionResult.RebootedItself;
				}
				else
				{
					if (IsRerollNecessary(ip, 0))
						ip.patternTag = bullet.dynamicSolver.SolveDynamicString(rawInst.patternTag, 4423641 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.modulePatterns.Stop(ip.patternTag);

					if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].HasTag(ip.patternTag))
						return InstructionResult.RebootedItself;
				}
			}

			// Reboot Pattern
			else if (ip.instructionType == PatternInstructionType.RebootPattern)
			{
				if (ip.patternControlTarget == PatternControlTarget.ThisPattern)
				{
					bullet.modulePatterns.Boot(patternIndex);
					return InstructionResult.RebootedItself;
				}
				else
				{
					if (IsRerollNecessary(ip, 0))
						ip.patternTag = bullet.dynamicSolver.SolveDynamicString(rawInst.patternTag, 4101111 * (numberOfRerolls++), ParameterOwner.Pattern);

					bullet.modulePatterns.Boot(ip.patternTag);

					if (bullet.modulePatterns.patternRuntimeInfo[patternIndex].HasTag(ip.patternTag))
						return InstructionResult.RebootedItself;
				}
			}

			// Set Instruction Delay
			else if (ip.instructionType == PatternInstructionType.SetInstructionDelay)
			{
				if (IsRerollNecessary(ip, 0))
					ip.waitTime = bullet.dynamicSolver.SolveDynamicFloat(rawInst.waitTime, 5054711 * (numberOfRerolls++), ParameterOwner.Pattern);

				instructionDelay = ip.waitTime;
			}

			#endregion

			#region random seed

			// Freeze Random Seed
			else if (ip.instructionType == PatternInstructionType.FreezeRandomSeed)
			{
				bullet.dynamicSolver.FreezeRandomSeed();

				// refreshing this bool now, so the next instruction (which is likely to be the very first one) won't be rerolled
				recentlyChangedSeed = false;
			}

			// Unfreeze Random Seed
			else if (ip.instructionType == PatternInstructionType.UnfreezeRandomSeed)
			{
				bullet.dynamicSolver.UnfreezeRandomSeed();
			}

			// Reroll Random Seed
			else if (ip.instructionType == PatternInstructionType.RerollRandomSeed)
			{
				bullet.dynamicSolver.RerollRandomSeed();
				recentlyChangedSeed = true;
			}

			// Set Random Seed
			else if (ip.instructionType == PatternInstructionType.SetRandomSeed)
			{
				if (IsRerollNecessary(ip, 0))
					ip.newRandomSeed = bullet.dynamicSolver.SolveDynamicSlider01(rawInst.newRandomSeed, 4862011 * (numberOfRerolls++), ParameterOwner.Pattern);

				bullet.dynamicSolver.SetRandomSeed(ip.newRandomSeed);
				recentlyChangedSeed = true;
			}

			#endregion
			
			#region custom parameters

			#region numbers

			else if (ip.instructionType == PatternInstructionType.AddCustomInteger)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customInt = bullet.dynamicSolver.SolveDynamicInt(rawInst.customInt, 4587361 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetInt(ip.customParamName, bullet.moduleParameters.GetInt(ip.customParamName) + ip.customInt);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 7458009 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3584711 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomIntAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customInt));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomInteger)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customInt = bullet.dynamicSolver.SolveDynamicInt(rawInst.customInt, 5482011 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetInt(ip.customParamName, bullet.moduleParameters.GetInt(ip.customParamName) * ip.customInt);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5486543 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2314533 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomIntMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customInt));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomInteger)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customInt = bullet.dynamicSolver.SolveDynamicInt(rawInst.customInt, 9004733 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetInt(ip.customParamName, ip.customInt);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 12486901 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2247101 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomIntSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customInt));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.AddCustomFloat)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customFloat = bullet.dynamicSolver.SolveDynamicFloat(rawInst.customFloat, 3215409 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetFloat(ip.customParamName, bullet.moduleParameters.GetFloat(ip.customParamName) + ip.customFloat);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4560547 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6321453 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomFloatAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customFloat));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomFloat)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customFloat = bullet.dynamicSolver.SolveDynamicFloat(rawInst.customFloat, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetFloat(ip.customParamName, bullet.moduleParameters.GetFloat(ip.customParamName) * ip.customFloat);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 5156447 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4015417 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomFloatMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customFloat));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomFloat)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customFloat = bullet.dynamicSolver.SolveDynamicFloat(rawInst.customFloat, 4022633 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetFloat(ip.customParamName, ip.customFloat);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 2007833 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6594801 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomFloatSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customFloat));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.AddCustomSlider01)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customSlider01 = bullet.dynamicSolver.SolveDynamicSlider01(rawInst.customSlider01, 3921017 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetSlider01(ip.customParamName, bullet.moduleParameters.GetSlider01(ip.customParamName) + ip.customSlider01);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1482061 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6365841 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomSlider01Add(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customSlider01));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomSlider01)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 8941011 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetSlider01(ip.customParamName, bullet.moduleParameters.GetSlider01(ip.customParamName) * ip.factor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4882411 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3003007 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomSlider01Multiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.factor));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomSlider01)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customSlider01 = bullet.dynamicSolver.SolveDynamicSlider01(rawInst.customSlider01, 9009014 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetSlider01(ip.customParamName, ip.customSlider01);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 9074011 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3535125 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomSlider01Set(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customSlider01));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.AddCustomLong)
			{
				// Longs are not dynamic
				//if (IsRerollNecessary(ip, 0))
				//	ip.customLong = bullet.dynamicSolver.SolveDynamicLong(rawInst.customLong, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetLong(ip.customParamName, bullet.moduleParameters.GetLong(ip.customParamName) + ip.customLong);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8457455 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 8041357 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomLongAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customLong));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomLong)
			{
				// Longs are not dynamic
				//if (IsRerollNecessary(ip, 0))
				//	ip.customLong = bullet.dynamicSolver.SolveDynamicLong(rawInst.customLong, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetLong(ip.customParamName, bullet.moduleParameters.GetLong(ip.customParamName) * ip.customLong);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 7070517 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1268499 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomLongMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customLong));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomLong)
			{
				// Longs are not dynamic
				//if (IsRerollNecessary(ip, 0))
				//	ip.customLong = bullet.dynamicSolver.SolveDynamicLong(rawInst.customLong, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetLong(ip.customParamName, ip.customLong);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4994999 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3085199 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomLongSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customLong));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.AddCustomDouble)
			{
				// Doubles are not dynamic
				//if (IsRerollNecessary(ip, 0))
				//	ip.customDouble = bullet.dynamicSolver.SolveDynamicDouble(rawInst.customDouble, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetDouble(ip.customParamName, bullet.moduleParameters.GetDouble(ip.customParamName) + ip.customDouble);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 6341707 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 5071507 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomDoubleAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customDouble));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomDouble)
			{
				// Doubles are not dynamic
				//if (IsRerollNecessary(ip, 0))
				//	ip.customDouble = bullet.dynamicSolver.SolveDynamicDouble(rawInst.customDouble, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetDouble(ip.customParamName, bullet.moduleParameters.GetDouble(ip.customParamName) * ip.customDouble);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4064089 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3627481 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomDoubleMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customDouble));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomDouble)
			{
				// Doubles are not dynamic
				//if (IsRerollNecessary(ip, 0))
				//	ip.customDouble = bullet.dynamicSolver.SolveDynamicDouble(rawInst.customDouble, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetDouble(ip.customParamName, ip.customDouble);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8140841 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 9365011 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomDoubleSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customDouble));
				}		
			}

			#endregion

			#region vectors

			else if (ip.instructionType == PatternInstructionType.AddCustomVector2)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customVector2 = bullet.dynamicSolver.SolveDynamicVector2(rawInst.customVector2, 8111521 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector2(ip.customParamName, bullet.moduleParameters.GetVector2(ip.customParamName) + ip.customVector2);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 9237777 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 5412811 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector2Add(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customVector2));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomVector2)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 4850555 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector2(ip.customParamName, bullet.moduleParameters.GetVector2(ip.customParamName) * ip.factor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 9361011 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 9084143 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector2Multiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.factor));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomVector2)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customVector2 = bullet.dynamicSolver.SolveDynamicVector2(rawInst.customVector2, 7889077 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector2(ip.customParamName, ip.customVector2);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8504123 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3781191 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector2Set(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customVector2));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.AddCustomVector3)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customVector3 = bullet.dynamicSolver.SolveDynamicVector3(rawInst.customVector3, 9218235 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector3(ip.customParamName, bullet.moduleParameters.GetVector3(ip.customParamName) + ip.customVector3);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4587101 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 9094811 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector3Add(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customVector3));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomVector3)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 4893033 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector3(ip.customParamName, bullet.moduleParameters.GetVector3(ip.customParamName) * ip.factor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 3541035 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 9501459 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector3Multiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.factor));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomVector3)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customVector3 = bullet.dynamicSolver.SolveDynamicVector3(rawInst.customVector3, 4810211 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector3(ip.customParamName, ip.customVector3);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4825137 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 4829577 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector3Set(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customVector3));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.AddCustomVector4)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customVector4 = bullet.dynamicSolver.SolveDynamicVector4(rawInst.customVector4, 3007307 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector4(ip.customParamName, bullet.moduleParameters.GetVector4(ip.customParamName) + ip.customVector4);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 4852421 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 6841017 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector4Add(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customVector4));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomVector4)
			{
				if (IsRerollNecessary(ip, 0))
					ip.factor = bullet.dynamicSolver.SolveDynamicFloat(rawInst.factor, 1974241 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector4(ip.customParamName, bullet.moduleParameters.GetVector4(ip.customParamName) * ip.factor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 2008801 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3779401 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector4Multiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.factor));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomVector4)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customVector4 = bullet.dynamicSolver.SolveDynamicVector4(rawInst.customVector4, 9040601 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetVector4(ip.customParamName, ip.customVector4);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 3813417 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 3917007 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomVector4Set(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customVector4));
				}		
			}

			#endregion

			#region colors

			else if (ip.instructionType == PatternInstructionType.AddCustomColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customColor = bullet.dynamicSolver.SolveDynamicColor(rawInst.customColor, 4862007 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetColor(ip.customParamName, bullet.moduleParameters.GetColor(ip.customParamName) + ip.customColor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 2691009 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 10194829 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomColorAdd(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customColor));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.MultiplyCustomColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customColor = bullet.dynamicSolver.SolveDynamicColor(rawInst.customColor, 4826019 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetColor(ip.customParamName, bullet.moduleParameters.GetColor(ip.customParamName) * ip.customColor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 8462981 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2854603 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomColorMultiply(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customColor));
				}		
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customColor = bullet.dynamicSolver.SolveDynamicColor(rawInst.customColor, 3086189 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
					bullet.moduleParameters.SetColor(ip.customParamName, ip.customColor);
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 1892563 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 2654701 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomColorSet(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customColor));
				}
			}

			else if (ip.instructionType == PatternInstructionType.OverlayCustomColor)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customColor = bullet.dynamicSolver.SolveDynamicColor(rawInst.customColor, 8080703 * (numberOfRerolls++), ParameterOwner.Pattern);

				if (ip.instructionTiming == InstructionTiming.Instantly)
				{
					Color oldCol = bullet.moduleParameters.GetColor(ip.customParamName);
					Color newCol = oldCol * (1-ip.customColor.a) + ip.customColor * ip.customColor.a;
					newCol.a = oldCol.a + ip.customColor.a*(1-oldCol.a);
					bullet.moduleParameters.SetColor(ip.customParamName, newCol);
				}
				else
				{
					if (IsRerollNecessary(ip, 1))
						ip.instructionDuration = bullet.dynamicSolver.SolveDynamicFloat(rawInst.instructionDuration, 9008621 * (numberOfRerolls++), ParameterOwner.Pattern);

					if (IsRerollNecessary(ip, 2))
						ip.operationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.operationCurve, 1593087 * (numberOfRerolls++), ParameterOwner.Pattern);
					
					bullet.microActions.Add(new MicroActionCustomColorOverlay(
						bullet, ip.instructionDuration, ip.operationCurve, ip.customParamName, ip.customColor));
				}		
			}

			#endregion

			#region other custom params

			else if (ip.instructionType == PatternInstructionType.SetCustomGradient)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customGradient = bullet.dynamicSolver.SolveDynamicGradient(rawInst.customGradient, 6841921 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetGradient(ip.customParamName, ip.customGradient);
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomBool)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customBool = bullet.dynamicSolver.SolveDynamicBool(rawInst.customBool, 6054811 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetBool(ip.customParamName, ip.customBool);
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomString)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customString = bullet.dynamicSolver.SolveDynamicString(rawInst.customString, 4516511 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetString(ip.customParamName, ip.customString);
			}

			else if (ip.instructionType == PatternInstructionType.AppendToCustomString)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customString = bullet.dynamicSolver.SolveDynamicString(rawInst.customString, 1285903 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetString(ip.customParamName, bullet.moduleParameters.GetString(ip.customParamName) + ip.customString);
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomAnimationCurve)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customAnimationCurve = bullet.dynamicSolver.SolveDynamicAnimationCurve(rawInst.customAnimationCurve, 1045543 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetAnimationCurve(ip.customParamName, ip.customAnimationCurve);
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomObject)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customObjectReference = bullet.dynamicSolver.SolveDynamicObjectReference(rawInst.customObjectReference, 1549499 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetObjectReference(ip.customParamName, ip.customObjectReference);
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomQuaternion)
			{
				bullet.moduleParameters.SetQuaternion(ip.customParamName, ip.customQuaternion);
			}

			/* *

			else if (ip.instructionType == PatternInstructionType.SetCustomRect)
			{
				if (IsRerollNecessary(ip, 0))
					ip.customRect = bullet.dynamicSolver.SolveDynamicRect(rawInst.customRect, 8484235 * (numberOfRerolls++), ParameterOwner.Pattern);
				
				bullet.moduleParameters.SetRect(ip.customParamName, ip.customRect);
			}

			else if (ip.instructionType == PatternInstructionType.SetCustomBounds)
			{
				bullet.moduleParameters.SetBounds(ip.customParamName, ip.customBounds);
			}

			/* */

			#endregion
			
			#endregion
			
			// save values if anything changed
			instructions[index] = ip;
			
			return InstructionResult.None;
		}

		// This is in a separate function so that it can be called at the very end of the stack.
		// Returns whether it had to go back to its corresponding BeginLoop.
		bool EndLoop()
		{
			if (loops.Count == 0) return false;

			// if we're skipping a loop that has another loop within it, that might not be the tag we're looking for
			endLoopTagsWanted--;
			if (endLoopTagsWanted > 0) return false;

			// next iteration
			LoopInfo cur = loops.Pop();
			cur.iterationsFinished++;
			if (cur.endless)
			{
				indexOfNextInstruction = cur.indexOfBeginInstruction;
				loops.Push(cur);

				// endless loop without a wait instruction should yield now and wait for next frame
				if (!hadWaitBlockDuringLoop) shouldBreakLoopNow = true;
				hadWaitBlockDuringLoop = false;

				return true;
			}
			else
			{
				cur.iterationsRemaining--;
				if (cur.iterationsRemaining > 0)
				{
					indexOfNextInstruction = cur.indexOfBeginInstruction;
					loops.Push(cur);
					return true;
				}

				return false;
			}
		}

		// Should the dynamic parameter of this instruction be solved once again ?
		bool IsRerollNecessary(SolvedInstruction ip, int channel)
		{
			bool preResult = MainRerollTest(ip, channel);
			if (!preResult) return false;
			
			// if the main test says we should reroll, check the random seed is not frozen before
			// Checking for frozen seed now is necessary because every iteration of the same instruction has a different solvingHash.
			if (!bullet.dynamicSolver.inheritedData.frozenSeed) return true;

			// if it's frozen, rerolling is only necessary if the current seed is different from the last known one
			if (!recentlyChangedSeed) return false;
			recentlyChangedSeed = false;
			return true;
		}

		bool MainRerollTest(SolvedInstruction ip, int channel)
		{
			if (ip.rerollFrequency[channel] == RerollFrequency.WheneverCalled) return true;
			if (ip.rerollFrequency[channel] == RerollFrequency.OnlyOncePerPattern) return false;
			if (ip.rerollFrequency[channel] == RerollFrequency.AtCertainLoops)
			{
				if (loops.Count == 0) return false;
				int relevantLoopIndex = (loops.Count-1) - ip.rerollLoopDepth[channel];
				if (relevantLoopIndex < 0) return false;

				// from now on we know we can look at loops[relevantLoopIndex] and examine how many times it's been over.

				// but first, let's find out if the sub-loops (if any) have just began, which would mean it really is a new iteration
				bool abort = false;
				int totalCount = loops.Count;
				LoopInfo relevantLoop = loops.Peek();
				if (relevantLoopIndex != totalCount-1) // there can't be subloops if we already look at the most-recent one
				{
					for (int i = relevantLoopIndex+1; i < totalCount; i++)
					{
						LoopInfo li = loops.Pop();
						if (li.iterationsFinished > 0) abort = true;
						tempStack.Push(li);
					}
					// while they're all out, retrieve the relevant loop for later
					relevantLoop = loops.Peek();
					// put them back into the stack now we've looked
					for (int i = relevantLoopIndex+1; i < totalCount; i++)
					{
						LoopInfo li = tempStack.Pop();
						loops.Push(li);
					}
					if (abort) return false;
				}
				// else the relevantLoop is effectively loops.Peek so it has the proper value.
				
				// We can now search for a sequence, or simply return true for reroll if no sequence is used.
				if (!ip.useComplexRerollSequence[channel])
				{
					return true;
				}

				int indexInSequence = relevantLoop.iterationsFinished % ip.checkEveryNLoops[channel];
				return (ip.loopSequence[channel] & (1 << indexInSequence)) != 0;
			}
			return false;
		}

		// Gets if we're in the first iteration of loops until the n-th parent loop. 0 is the childmost loop (the current one) and thus would always return true.
		bool IsFirstLoopIteration(int loopDepth)
		{
			if (loops.Count == 0) return false;

			return true; // dummy
			// TODO : think the design a bit deeper. I know I could just go upwards through the loop stack and check for .iterationsFinished but...
		}
	}
}