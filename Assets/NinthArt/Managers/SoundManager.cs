using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace NinthArt
{
	internal class SoundManager : Singleton<SoundManager>
	{
		private AudioSource _sfxPlayer;
		private AudioSource _musicPlayer;

		[SerializeField] private List<AudioClip> clips;

		private readonly LinkedList<AudioSource> _pendingPitchedSfxPlayers = new();
		private readonly LinkedList<AudioSource> _playingPlayers = new();
		private readonly Dictionary<string, AudioClip> _dict = new();

		private AudioClip GetAudioClip(string key)
		{
			return _dict.TryGetValue(key, out var clip) ? clip : null;
		}
        internal static void PlaySfx(string key, float vol = 0.5f)
		{
			if (!Preference.SfxOn)
			{
				return;
			}

			var clip = Instance.GetAudioClip(key);
			if (!clip) return;

			Instance._sfxPlayer.volume = vol;
			Instance._sfxPlayer.PlayOneShot(clip);
			DOVirtual.DelayedCall(clip.length, () =>
			{
				Instance._sfxPlayer.clip = null;
			});
		}

		/*
		internal static void PlaySfx(string key, float pitch)
		{
			if (!Preference.SfxOn)
			{
				return;
			}

			var clip = Instance.GetAudioClip(key);
			if (clip == null)
			{
				return;
			}

			AudioSource player;
			if (Instance._pendingPitchedSfxPlayers.First == null)
			{
				player = Instance.gameObject.AddComponent<AudioSource>();
			}
			else
			{
				player = Instance._pendingPitchedSfxPlayers.First.Value;
				player.volume = 1.0f;
				Instance._pendingPitchedSfxPlayers.RemoveFirst();
				Instance._playingPlayers.AddLast(player);
			}

			player.pitch = pitch;
			player.PlayOneShot(clip);
			DOVirtual.DelayedCall(clip.length, () =>
			{
				Instance._playingPlayers.AddLast(player);
				Instance._pendingPitchedSfxPlayers.AddLast(player);
				player.clip = null;
			});
		}*/

		internal static void PlayMusic(string key, bool loop = false, float vol = 0.3f)
		{
			if (!Preference.MusicOn)
			{
				return;
			}

			if (IsMusicPlaying(key))
			{
				Instance._musicPlayer.loop = loop;
				return;
			}

			var clip = Instance.GetAudioClip(key);
			if (!clip)
			{
				return;
			}

			// Play music logic here
			Instance._musicPlayer.Stop();
			Instance._musicPlayer.clip = clip;
			Instance._musicPlayer.Play();
			Instance._musicPlayer.loop = loop;
			Instance._musicPlayer.volume = vol;
		}
		internal static void PlayRandomMusics()
		{
			if (Instance._musicPlayer.isPlaying || !Preference.MusicOn) return;

			PlayMusic("game_bgm_0" + UnityEngine.Random.Range(0, 10).ToString());
		}
		private static bool IsMusicPlaying(string music)
		{
			return
				Instance._musicPlayer.clip != null &&
				Instance._musicPlayer.clip.name.Equals(music) &&
				Instance._musicPlayer.isPlaying;
		}

		internal static void StopMusic()
		{
			Instance._musicPlayer.Stop();
		}

		internal static void ResumeMusic()
		{
			if (!Preference.MusicOn || Instance._musicPlayer.clip == null || Instance._musicPlayer.isPlaying)
			{
				return;
			}

			Instance._musicPlayer.Play();
		}

		internal static void Pause()
		{
			foreach (var t in Instance._playingPlayers)
			{
				t.volume = 0.0f;
			}
		}

		internal static void Resume()
		{
			foreach (var t in Instance._playingPlayers)
			{
				t.volume = 1.0f;
			}
		}

		private void Awake()
		{
			var audioSources = GetComponents<AudioSource>();
			if (audioSources.Length < 2)
			{
				Array.Resize(ref audioSources, 2);
				for (var i = 0; i < 2; i++)
				{
					audioSources[i] = gameObject.AddComponent<AudioSource>();
				}
			}
			_sfxPlayer = audioSources[0];
			_musicPlayer = audioSources[1];
			_playingPlayers.AddLast(_sfxPlayer);
			_playingPlayers.AddLast(_musicPlayer);

			if (clips == null) return;
			foreach (var clip in clips)
			{
				_dict.Add(clip.name, clip);
			}
		}
		[SerializeField] bool playLoopMusic;
        private void Update()
        {
			if (!playLoopMusic || !Config.levelLoaded)
				return;
			PlayRandomMusics();
        }
    }
}