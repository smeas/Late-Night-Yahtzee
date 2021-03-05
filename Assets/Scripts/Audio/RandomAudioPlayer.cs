using UnityEngine;

namespace Audio {
	[RequireComponent(typeof(AudioSource))]
	public class RandomAudioPlayer : MonoBehaviour {
		[SerializeField] private AudioClip[] audioClips;

		private AudioSource audioSource;

		private void Awake() {
			audioSource = GetComponent<AudioSource>();
		}

		public void Play() {
			audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
			audioSource.Play();
		}
	}
}