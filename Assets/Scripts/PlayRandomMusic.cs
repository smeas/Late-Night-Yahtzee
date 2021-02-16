using System;
using System.IO;
using System.Net.Http;
using NLayer;
using UnityEngine;

public class PlayRandomMusic : MonoBehaviour {
	private readonly HttpClient httpClient = new HttpClient();

	private MpegFile mp3File;
	private AudioClip audioClip;

	private AudioSource audioSource;

	private async void Start() {
		audioSource = GetComponent<AudioSource>();

		// Get random track metadata.
		Debug.Log("Fetching some SICC tunes...");
		string metaData = await httpClient.GetStringAsync("https://www.demolatar.se/api/getRandomTrack.php");
		Root meta = JsonUtility.FromJson<Root>(metaData);

		Debug.Assert(meta.tracklink.EndsWith(".mp3"));

		// Download the track.
		Debug.Log("Downloading...");
		byte[] rawMp3Data = await httpClient.GetByteArrayAsync(meta.tracklink);

		// Setup decoder.
		mp3File = new MpegFile(new MemoryStream(rawMp3Data));
		int sampleCount = (int)(mp3File.Length / mp3File.Channels / sizeof(float));

		audioClip = AudioClip.Create($"{meta.track} - {meta.band}", sampleCount, mp3File.Channels, mp3File.SampleRate, true, ReadSamples);

		Debug.Log($"Playing '{meta.track}' by '{meta.band}' ({meta.genre})...");
		audioSource.clip = audioClip;
		audioSource.Play();
	}

	private void ReadSamples(float[] data) {
		mp3File.ReadSamples(data, 0, data.Length);
	}


	private void OnDestroy() {
		Destroy(audioClip);
	}

	[Serializable]
	public class Root {
		public string id;
		public string date;
		public string band;
		public string track;
		public string tracklink;
		public string genre;
		public string plays;
		public int rating;
		public string downloads;
		public string web;
	}
}