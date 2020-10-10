using NWaves.Audio;
using NWaves.Signals;
using NWaves.VoiceEffects.Interfaces;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace NWaves.VoiceEffects.Services
{
    /// <summary>
    /// Simple audio service wrapped around System.Media.SoundPlayer
    /// </summary>
    public class AudioService : IAudioService
    {
        private SoundPlayer _player;

        public float Volume { get; set; }

        public async Task PlayAsync(string location, int startPos = 0, int endPos = -1)
        {
            _player?.Dispose();
            _player = new SoundPlayer(location);
            _player.Play();
        }

        public async Task PlayAsync(DiscreteSignal signal, int startPos = 0, int endPos = -1, short bitDepth = 16)
        {
            var stream = new MemoryStream();
            var wave = new WaveFile(signal, bitDepth);
            wave.SaveTo(stream);

            stream = new MemoryStream(stream.ToArray());

            _player?.Dispose();
            _player = new SoundPlayer(stream);
            _player.Stream.Seek(0, SeekOrigin.Begin);
            _player.Play();
        }

        public void Pause() => _player.Stop();

        public void Resume() => _player.Play();

        public void Stop() => _player.Stop();
    }
}
