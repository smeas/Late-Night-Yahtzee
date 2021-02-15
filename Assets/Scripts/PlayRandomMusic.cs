using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NAudio.Wave;
using UnityEngine;

public class PlayRandomMusic : MonoBehaviour {
	private readonly HttpClient httpClient = new HttpClient();

	private Mp3FileReader mp3Reader;
	private WaveStream pcmStream;
	private ISampleProvider sampleProvider;
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

		// Setup the conversion stream.
		mp3Reader = new Mp3FileReader(new MemoryStream(rawMp3Data));
		pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);
		sampleProvider = pcmStream.ToSampleProvider();

		// Create an audio clip from the converted data.
		WaveFormat format = pcmStream.WaveFormat;
		int sampleCount = (int)pcmStream.Length / (pcmStream.WaveFormat.BitsPerSample / 8);

		audioClip = AudioClip.Create($"{meta.track} - {meta.band}", sampleCount, format.Channels, format.SampleRate, true, ReadSamples);

		Debug.Log($"Playing '{meta.track}' by '{meta.band}' ({meta.genre})...");
		audioSource.clip = audioClip;
		audioSource.Play();
	}

	private void ReadSamples(float[] data) {
		sampleProvider.Read(data, 0, data.Length);
	}


	private void OnDestroy() {
		Destroy(audioClip);
		pcmStream?.Dispose();
		mp3Reader?.Dispose();
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