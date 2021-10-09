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
		private int ani_blank = -1;
		private int ani_portal = -1;
		private int ani_lifestone = -1;
		private int ani_emote = -1;
		private int ani_death = -1;
		private int ani_spellcast = -1;
		private int ani_jump = -1;

		// probably need some configs
		const float Health_Low = 0.3f;
		const float Health_Critical = 0.15f;

		protected override void Startup()
		{
			InitChromaApp();

			ani_blank = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Blank));
			ani_lifestone = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Lifestone));
			ani_emote = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Emote));
			ani_portal = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Portal));
			ani_death = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Death));
			ani_spellcast = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Spellcast));
			ani_jump = ChromaAnimationAPI.GetAnimation(System.IO.Path.Combine(this.Path, Properties.Settings.Default.Jump));

			this.MessageProcessed += OnMessageProcessed;

			PlayAnimation(ani_portal);
		}

		protected override void Shutdown()
		{
			this.MessageProcessed -= OnMessageProcessed;

			ChromaAnimationAPI.Uninit();
		}

		[BaseEvent("MessageProcessed")]
		private void OnMessageProcessed(object sender, MessageProcessedEventArgs e)
		{
			switch (e.Message.Type)
			{
				case 0xf74b: // exit portal
					{
						int obj = e.Message.Value<int>("object");
						ushort flags = e.Message.Value<ushort>("portalType");

						Debug.Print("Exit Portal: ID {0}, Type {1}", obj, flags);
						if (obj == Core.CharacterFilter.Id && (flags & 0x4000) == 0)
							PlayAnimation(ani_blank);
					}
					break;

				case 0xf751: // enter portal
					PlayAnimation(ani_portal);
					break;

				case 0x019E: // player killed
					PlayAnimation(ani_death);
					break;

				case 0x01E2: // player emote
					PlayAnimation(ani_emote);
					break;

				case 0xF7B1:
					// player cast spell
					if (Convert.ToInt32(e.Message["action"]) == 0x0048)
					{
						PlayAnimation(ani_spellcast);
					}
					break;

				case 0x074E: 
					// player jumping
					int jumper = e.Message.Value<int>("object");

					if (jumper == Core.CharacterFilter.Id)
					{
						PlayAnimation(ani_jump);
						return;
					}
					break;
			}
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
			ChromaAnimationAPI.PlayAnimationLoop(animationId, true);
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
