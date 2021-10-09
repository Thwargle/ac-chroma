﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using ChromaSDK;

using Decal.Adapter;

namespace Chroma
{
	// TODO: Guid
	// TODO: WireUpBaseEvents?
	[FriendlyName("AC Chroma Connect")]
	public class Connector : PluginBase
	{
		private int ani_blank = -1;
		private int ani_portal = -1;
		private int ani_lifestone = -1;
		private int ani_emote = -1;
		private int ani_death = -1;
		private int ani_spellcast = -1;
		private int ani_jump = -1;
		private int ani_raiseSkill = -1;
		private int ani_testAnim = -1;

		// probably need some configs
		//const float Health_Low = 0.3f;
		//const float Health_Critical = 0.15f;

		protected override void Startup()
		{
			InitChromaApp();
			InitAnims();
			ServerDispatch += FilterCore_ServerDispatch;
			ClientDispatch += FilterCore_ClientDispatch;
			PlayAnimation(ani_portal);
		}

		private void InitAnims()
		{
			ani_blank = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Blank));
			ani_lifestone = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Lifestone));
			ani_emote = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Emote));
			ani_portal = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Portal));
			ani_death = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Death));
			ani_spellcast = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Spellcast));
			ani_jump = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Jump));
			ani_raiseSkill = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.RaiseSkill));
			ani_testAnim = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.TestAnim));
		}

		// Parses server messages
		private void FilterCore_ServerDispatch(object sender, NetworkMessageEventArgs e)
		{
			switch (e.Message.Type)
			{
				case 0xF751:
					PlayAnimation(ani_portal);
					break;

				case 0xF7B0:
					int gameEvent = e.Message.Value<int>("event");

					switch (gameEvent)
					{
						case 0x028A: //weenie errors. See: https://github.com/ACEmulator/ACE/blob/master/Source/ACE.Entity/Enum/WeenieError.cs
							{
								int weenieError = Convert.ToInt32(e.Message[3]);

								if (weenieError.Equals(0x0402)) // YourSpellFizzled = 0x0402
								{
									PlayAnimation(ani_testAnim);
								}

								if (weenieError.Equals(0x003D)) // YouChargedTooFar = 0x003D
								{
									PlayAnimation(ani_testAnim);
								}
							}
							break;

						case 0x01C7: // action complete
							PlayAnimation(ani_blank);
							break;

						case 0x01B2: // Receive Melee Damage
							PlayAnimation(ani_testAnim);
							break;

						case 0x01B4: // Evade a Melee Attack
							PlayAnimation(ani_testAnim);
							break;

						case 0x01AC:
							PlayAnimation(ani_testAnim);
							break;
					}
					break;

			}
		}

		// Parses client messages
		private void FilterCore_ClientDispatch(object sender, NetworkMessageEventArgs e)
		{

			// GameAction
			switch (e.Message.Type)
			{

				case 0xF7B1:
					int action = e.Message.Value<int>("action");
					switch(action)
					{
						case 0x00A1: // Materialize character (including any portal taken)
							PlayAnimation(ani_blank);
							break;

						case 0x004A: // Start casting a targeted spell
							PlayAnimation(ani_spellcast);
							break;

						case 0x0048: // Start casting an untargeted spell
							PlayAnimation(ani_spellcast);
							break;

						case 0x0046: // Raise a skill
							PlayAnimation(ani_raiseSkill);
							break;

						case 0x0045: // Raise an attribute
							PlayAnimation(ani_testAnim);
							break;

						case 0x0044: // Raise a vital
							PlayAnimation(ani_testAnim);
							break;

						case 0x001A: // Equip an item
							PlayAnimation(ani_testAnim);
							break;

						case 0x001B: // Drop an item
							PlayAnimation(ani_testAnim);
							break;

						case 0x00CD: // Give an item
							PlayAnimation(ani_testAnim);
                            break; 
					}
					break;
			}
		
		}

		protected override void Shutdown()
		{
			ServerDispatch -= FilterCore_ServerDispatch;
			ClientDispatch -= FilterCore_ClientDispatch;
			ChromaStop();
			ChromaAnimationAPI.Uninit();
		}

		private void ChromaStop()
		{
			int count = ChromaAnimationAPI.GetPlayingAnimationCount();
			for (int i = 0; i < count; i++)
			{
				int ani = ChromaAnimationAPI.GetPlayingAnimationId(i);
				//Debug.Print("Ani Playing: {0}", ChromaAnimationAPI.GetAnimationName(ani));
				ChromaAnimationAPI.StopAnimation(ani);
			}

			ChromaAnimationAPI.ClearAll();
		}

		private void ChromaStop(int animationId)
		{
			ChromaAnimationAPI.StopAnimation(animationId);
			ChromaAnimationAPI.ClearAll();
		}

		// TODO: Look at composite API for multiples

		private void PlayAnimation(int animationId)
		{
			ChromaAnimationAPI.PlayAnimationLoop(animationId, false);
		}

		private void PlayAnimation(int animationId, bool loop)
		{
			ChromaAnimationAPI.PlayAnimationLoop(animationId, loop);
		}

		private void InitChromaApp()
		{
			ChromaSDK.APPINFOTYPE appInfo = new APPINFOTYPE();
			appInfo.Title = "AC Chroma Connect";
			appInfo.Description = "AC Chroma Effects";
			appInfo.Author_Name = "Paradox, Thwargle";
			appInfo.Author_Contact = "https://paradoxlost.com";

			//appInfo.SupportedDevice = 
			//    0x01 | // Keyboards
			//    0x02 | // Mice
			//    0x04 | // Headset
			//    0x08 | // Mousepads
			//    0x10 | // Keypads
			//    0x20   // ChromaLink devices
			appInfo.SupportedDevice = (0x01 | 0x02 | 0x04 | 0x08 | 0x10 | 0x20);
			//    0x01 | // Utility. (To specifiy this is an utility application)
			//    0x02   // Game. (To specifiy this is a game);
			appInfo.Category = 1;
			int result = ChromaAnimationAPI.InitSDK(ref appInfo);
			switch (result)
			{
				case RazerErrors.RZRESULT_DLL_NOT_FOUND:
					Debug.Print("Chroma DLL is not found! {0}", RazerErrors.GetResultString(result));
					break;
				case RazerErrors.RZRESULT_DLL_INVALID_SIGNATURE:
					Debug.Print("Chroma DLL has an invalid signature! {0}", RazerErrors.GetResultString(result));
					break;
				case RazerErrors.RZRESULT_SUCCESS:
					break;
				default:
					Debug.Print("Failed to initialize Chroma! {0}", RazerErrors.GetResultString(result));
					break;
			}
		}
	}
}
