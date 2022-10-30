using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioEmitter : MonoBehaviour
{

	public AudioMixerGroup mixerGroup;
	public Sound[] sounds;
	public float distanceMin;
	public float distanceMax;
	public AnimationCurve distanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });

	void Awake()
	{

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;
			s.source.spatialBlend = 1;
			s.source.rolloffMode = AudioRolloffMode.Custom;
			s.source.minDistance = distanceMin;
			s.source.maxDistance = distanceMax;

			s.source.outputAudioMixerGroup = mixerGroup;
			if (s.playOnAwake)
			{
				Play(s.name);
			}
		}
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
}
