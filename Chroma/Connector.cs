using System;
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
		private int ani_keybinds = -1;
		private int ani_portal = -1;
		private int ani_lifestone = -1;
		private int ani_emote = -1;
		private int ani_death = -1;
		private int ani_spellcast = -1;
		private int ani_jump = -1;
		private int ani_raiseSkill = -1;
		private int ani_testAnim = -1;
		private int ani_levelUp = -1;
		private int ani_healself = -1;
		private int ani_healthupyellow = -1;
		private int ani_yellowtoblue = -1;
		private int ani_yellowtored = -1;
		private int ani_maxlevel = -1;
		private int ani_aethlevelup = -1;
	


		private int currentCharacterID = -1;

		// probably need some configs
		//const float Health_Low = 0.3f;
		//const float Health_Critical = 0.15f;


		//int obj = e.Message.Value<int>("object");
		//if (obj == Core.CharacterFilter.Id)


		protected override void Startup()
		{
			InitChromaApp();
			InitAnims();
			ServerDispatch += FilterCore_ServerDispatch;
			ClientDispatch += FilterCore_ClientDispatch;
			PlayAnimation(ani_portal);
			CoreManager.Current.CharacterFilter.LoginComplete += CharacterFilter_LoginComplete;
		}

		private void InitAnims()
		{
			ani_keybinds = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Keybinds));
			ani_lifestone = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Lifestone));
			ani_emote = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Emote));
			ani_portal = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Portal));
			ani_death = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Death));
			ani_spellcast = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Spellcast));
			ani_jump = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Jump));
			ani_raiseSkill = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.RaiseSkill));
			ani_testAnim = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.TestAnim));
			ani_levelUp = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.LevelUp));
			ani_healself = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.HealSelf));
			ani_healthupyellow = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.HealthUpYellow));
			ani_yellowtoblue = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Yellow_To_Blue));
			ani_yellowtored = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Yellow_To_Red));
			ani_maxlevel = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.MaxLevel));
			ani_aethlevelup = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.AethLevelUp));
		}

		// Parses server messages
		private void FilterCore_ServerDispatch(object sender, NetworkMessageEventArgs e)
		{
			int obj = e.Message.Value<int>("object");

			switch (e.Message.Type)
			{
				case 0xF7B0: // Network_WOrderHdr
					int gameEvent = e.Message.Value<int>("event");

					switch (gameEvent)
					{
						case 0x028A: //weenie errors. See: https://github.com/ACEmulator/ACE/blob/master/Source/ACE.Entity/Enum/WeenieError.cs
							{
								int weenieError = e.Message.Value<int>("type");
								switch (weenieError)
								{
									case 0x0402: // YourSpellFizzled = 0x0402
										PlayAnimation(ani_testAnim);
										break;

									case 0x003D: // YouChargedTooFar = 0x003D
										PlayAnimation(ani_testAnim);
										break;
								}
							}
							break;

						case 0x01C7: // action complete
							PlayAnimation(ani_keybinds);
							break;

						case 0x01B2: // Receive Melee Damage
							PlayAnimation(ani_testAnim);
							break;

						case 0x01B4: // Evade a Melee Attack
							PlayAnimation(ani_testAnim);
							break;

						case 0x01AC: // Your Death
							PlayAnimation(ani_testAnim);
							break;
					}
					break;

				case 0xF751: // Effects_PlayerTeleport
					PlayAnimation(ani_portal);
					break;

				//case 0xF74A: // Inventory_PickupEvent.  -- only affects pickup from ground.
				//	ToChat("About to pick up from ground.");
				//	if (CheckIfMe(obj)) //check if the effect is on the current player
				//	{
				//		ToChat("Picked up from ground.");
				//		PlayAnimation(ani_testAnim);
				//	}
				//	break;

				// Playscripts
				// See: https://github.com/ACEmulator/ACE/blob/master/Source/ACE.Entity/Enum/PlayScript.cs
				// TODO:  get our ID and ensure the effect is coming from us?
				case 0xF755: // Effects_PlayScriptType
					if (CheckIfMe(obj)) //check if the effect is on the current player
					{
						int scriptType = e.Message.Value<int>("effect");
						switch (scriptType)
						{

							case 0x8A: // LevelUp
								PlayAnimation(ani_levelUp);
								break;

							case 0x8D: // WeddingBliss (MaxLevel)
								PlayAnimation(ani_maxlevel);
								break;

							case 0x1F: // HealthUpRed (heal self)
								PlayAnimation(ani_healself);
								break;

							case 0x4B: // SwapHealth_Yellow_To_Red (stamina to health)
								PlayAnimation(ani_yellowtored);
								break;

							case 0x23: // HealthUpYellow (restam) (restam)
								PlayAnimation(ani_healthupyellow);
								break;

							case 0x4C: // SwapHealth_Yellow_To_Blue (stamina to mana)
								PlayAnimation(ani_yellowtoblue);
								break;



							// havent tested below this line

							case 0xA1: //AetheriaLevelUp
								PlayAnimation(ani_aethlevelup);
								break;

							case 0xA2: // AetheriaSurgeDestruction
								PlayAnimation(ani_testAnim);
								break;

							case 0xA3: // AetheriaSurgeProtection
								PlayAnimation(ani_testAnim);
								break;

							case 0xA4: // AetheriaSurgeRegeneration
								PlayAnimation(ani_testAnim);
								break;

							case 0xA5: // AetheriaSurgeAffliction
								PlayAnimation(ani_testAnim);
								break;

							case 0xA6: // AetheriaSurgeFestering
								PlayAnimation(ani_testAnim);
								break;

							case 0xA7: // HealthDownVoid
								PlayAnimation(ani_testAnim);
								break;

							case 0xA8: // RegenDownVoid
								PlayAnimation(ani_testAnim);
								break;

							case 0xA9: // SkillDownVoid
								PlayAnimation(ani_testAnim);
								break;

							case 0xAA: // DirtyFightingHealDebuff
								PlayAnimation(ani_testAnim);
								break;

							case 0xAB: // DirtyFightingAttackDebuff
								PlayAnimation(ani_testAnim);
								break;

							case 0xAC: // DirtyFightingDefenseDebuff
								PlayAnimation(ani_testAnim);
								break;

							case 0xAD: // DirtyFightingDamageOverTime
								PlayAnimation(ani_testAnim);
								break;


						}
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

				case 0xF7B1: // Network_OrderHdr
					int action = e.Message.Value<int>("action");
					switch (action)
					{
						case 0x00A1: // Materialize character (including any portal taken)
							PlayAnimation(ani_keybinds);
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

						case 0x0063: // Character_TeleToLifestone
							PlayAnimation(ani_portal);
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

		private bool CheckIfMe(int objID)
		{
			ToChat("ObjectID is: " + objID + " || currentCharacterID is: " + currentCharacterID);
			if (objID == currentCharacterID)
			{
				return true;
			}
			return false;
		}
		internal static void ToChat(string text)
		{
			CoreManager.Current.Actions.AddChatText(text, 5, 1);
		}

		private void CharacterFilter_LoginComplete(object sender, EventArgs e)
		{

			currentCharacterID = Core.CharacterFilter.Id;
			ToChat("AC Chroma Connector Initialized");
			ToChat("My character ID is: " + currentCharacterID.ToString());
		}
		private void InitChromaApp()
		{
			ChromaSDK.APPINFOTYPE appInfo = new APPINFOTYPE();
			appInfo.Title = "AC Chroma Connect";
			appInfo.Description = "AC Chroma Effects";
			appInfo.Author_Name = "Paradox, Thwargle, Hells";
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
