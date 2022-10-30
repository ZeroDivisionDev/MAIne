using UnityEngine.Audio;
using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;

	public AudioMixerGroup mixerGroup;

	public AudioMixerGroup musicMixerGroup;

	public Sound[] sounds;
	public Sound[] musics;

	public string currentMusic;

	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			if(s.source.outputAudioMixerGroup == null)
				s.source.outputAudioMixerGroup = mixerGroup;
            if (s.playOnAwake)
            {
				Play(s.name);
            }
		}

		foreach (Sound s in musics)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			if (s.source.outputAudioMixerGroup == null)
				s.source.outputAudioMixerGroup = musicMixerGroup;
			if (s.playOnAwake)
			{
				Play(s.name);
			}
		}

		currentMusic = "Music1";
		StartCoroutine(PlayMusic());
	}

	public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
		//Debug.Log("Playing " + Time.realtimeSinceStartup);
		s.source.Play();
	}

	public void Stop(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

        if (s.source.isPlaying)
        {
			s.source.Stop();
        }
	}

	IEnumerator PlayMusic()
    {
		
		while(true)
        {
			Sound s = Array.Find(musics, item => item.name == currentMusic);
			s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
			s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
			s.source.Play();
			while (s.source.isPlaying)
            {
				float waitTime = UnityEngine.Random.Range(40f, 160f);
				yield return new WaitForSecondsRealtime(waitTime);
            }
			string newMusic;
			do
			{
				newMusic = "Music" + UnityEngine.Random.Range(1, 7);

			} while (newMusic == currentMusic);
			currentMusic = newMusic;
		}
	}

	public void StopMusic()
    {
		Sound s = Array.Find(musics, item => item.name == currentMusic);
		if (s.source.isPlaying)
		{
			s.source.Stop();
		}
	}

}
