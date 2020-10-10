using Caliburn.Micro;
using Microsoft.Win32;
using NWaves.Audio;
using NWaves.Effects;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.VoiceEffects.Interfaces;
using System.IO;

namespace NWaves.VoiceEffects.ViewModels
{
    public class MainWindowViewModel : Screen
    {
        private readonly IAudioService _audioService;

        private DiscreteSignal _background, _voice;

        private int _windowSize = 256;
        public int WindowSize
        {
            get => _windowSize;
            set
            {
                _windowSize = value;
                NotifyOfPropertyChange(() => WindowSize);
            }
        }

        private int _hopSize = 50;
        public int HopSize
        {
            get => _hopSize;
            set
            {
                _hopSize = value;
                NotifyOfPropertyChange(() => HopSize);
            }
        }

        private float _wet = 0.9f;
        public float Wet
        {
            get => _wet;
            set
            {
                _wet = value;
                NotifyOfPropertyChange(() => Wet);
            }
        }

        private float _dry = 0.1f;
        public float Dry
        {
            get => _dry;
            set
            {
                _dry = value;
                NotifyOfPropertyChange(() => Dry);
            }
        }

        private bool _morphChecked = true;
        public bool MorphChecked
        {
            get => _morphChecked;
            set
            {
                _morphChecked = value;
                NotifyOfPropertyChange(() => MorphChecked);
            }
        }

        private bool _robotizeChecked;
        public bool RobotizeChecked
        {
            get => _robotizeChecked;
            set
            {
                _robotizeChecked = value;
                NotifyOfPropertyChange(() => RobotizeChecked);
            }
        }

        private bool _whisperizeChecked;
        public bool WhisperizeChecked
        {
            get => _whisperizeChecked;
            set
            {
                _whisperizeChecked = value;
                NotifyOfPropertyChange(() => WhisperizeChecked);
            }
        }

        private string _voiceFile = "No file";
        public string VoiceFile
        {
            get => _voiceFile;
            set
            {
                _voiceFile = value;
                NotifyOfPropertyChange(() => VoiceFile);
            }
        }

        private string _backgroundFile = "No file";
        public string BackgroundFile
        {
            get => _backgroundFile;
            set
            {
                _backgroundFile = value;
                NotifyOfPropertyChange(() => BackgroundFile);
            }
        }


        public MainWindowViewModel(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public void Voice()
        {
            var ofd = new OpenFileDialog();
            var dialogResult = ofd.ShowDialog();

            if (dialogResult == false)
            {
                return;
            }

            using var stream = new FileStream(ofd.FileName, FileMode.Open);

            var waveFile = new WaveFile(stream);
            _voice = waveFile[Channels.Average];

            VoiceFile = Path.GetFileName(ofd.FileName);
        }

        public void Background()
        {
            var ofd = new OpenFileDialog();
            var dialogResult = ofd.ShowDialog();

            if (dialogResult == false)
            {
                return;
            }

            using var stream = new FileStream(ofd.FileName, FileMode.Open);

            var waveFile = new WaveFile(stream);
            _background = waveFile[Channels.Average];

            BackgroundFile = Path.GetFileName(ofd.FileName);
        }

        public async void ListenWithEffect()
        {
            if (_voice == null) return;

            DiscreteSignal voice = _voice;

            if (RobotizeChecked)
            {
                var robot = new RobotEffect(HopSize, WindowSize) { Wet = Wet, Dry = Dry };

                voice = robot.ApplyTo(voice);
            }

            if (WhisperizeChecked)
            {
                var whisper = new WhisperEffect(HopSize, WindowSize) { Wet = Wet, Dry = Dry };

                voice = whisper.ApplyTo(voice);
            }

            if (MorphChecked)
            {
                if (_background == null) return;

                if (_background.SamplingRate != _voice.SamplingRate)
                {
                    _background = Operation.Resample(_background, _voice.SamplingRate);
                }

                var effect = new MorphEffect(HopSize, WindowSize)
                {
                    Wet = Wet,
                    Dry = Dry
                };

                voice = effect.ApplyTo(voice, _background);
            }

            await _audioService.PlayAsync(voice);
        }

        public async void PlayVoice() => await _audioService.PlayAsync(_voice);

        public async void PlayBackground() => await _audioService.PlayAsync(_background);

        public void StopVoice() => _audioService.Stop();

        public void StopBackground() => _audioService.Stop();
    }
}
