﻿using System;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using SharpAudio;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class GateOpenAndCloseBehavior : BehaviorModule
    {
        private GameObject _gameObject;
        private GateOpenAndCloseBehaviorModuleData _moduleData;
        private bool _open;
        private bool _playedFinishedSound = false;
        private bool _toggledColliders = false;
        private string _closedGeometry = "Closed";
        private string[] _openGeometries = { "OpenLeft", "OpenRight" };

        private DoorState _state;

        private TimeSpan _toggleFinishedTime;
        private TimeSpan _pathingToggleTime;
        private TimeSpan _finishedSoundTime;

        private AudioSource _openingSoundLoop;
        private AudioSource _closingSoundLoop;

        internal GateOpenAndCloseBehavior(GameObject gameObject, GateOpenAndCloseBehaviorModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;

            _open = false; // _moduleData.OpenByDefault;
            _state = DoorState.Idle;
        }

        public void Toggle()
        {
            switch(_state)
            {
                case DoorState.Idle:
                    _state = _open ? DoorState.StartClosing : DoorState.StartOpening;
                    break;
                case DoorState.StartOpening:
                case DoorState.Opening:
                    _state = DoorState.StartClosing;
                    break;
                case DoorState.StartClosing:
                case DoorState.Closing:
                    _state = DoorState.StartOpening;
                    break;
            }
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var audioSystem = context.GameContext.AudioSystem;

            switch (_state)
            {
                case DoorState.Idle:
                    return;

                case DoorState.StartOpening:
                    audioSystem.DisposeSource(_closingSoundLoop);
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, false);
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, true);
                    _toggleFinishedTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.ResetTimeInMilliseconds);
                    _pathingToggleTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.ResetTimeInMilliseconds * _moduleData.PercentOpenForPathing);

                    _openingSoundLoop = audioSystem.PlayAudioEvent(_gameObject, _moduleData.SoundOpeningGateLoop?.Value, true);
                    _finishedSoundTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.TimeBeforePlayingOpenSound);
                    
                    _state = DoorState.Opening;
                    _toggledColliders = false;
                    _playedFinishedSound = false;
                    break;

                case DoorState.Opening:
                    if (!_toggledColliders && context.Time.TotalTime > _pathingToggleTime)
                    {
                        _gameObject.HideCollider(_closedGeometry);
                        foreach (var openCollider in _openGeometries)
                        {
                            _gameObject.ShowCollider(openCollider);
                        }
                        _toggledColliders = true;
                    }
                    if (!_playedFinishedSound && context.Time.TotalTime > _finishedSoundTime)
                    {
                        audioSystem.DisposeSource(_openingSoundLoop);
                        audioSystem.PlayAudioEvent(_gameObject, _moduleData.SoundFinishedOpeningGate?.Value);
                        _playedFinishedSound = true;
                    }
                    if (context.Time.TotalTime > _toggleFinishedTime)
                    {
                        _open = true;
                        _state = DoorState.Idle;
                    }
                    break;

                case DoorState.StartClosing:
                    audioSystem.DisposeSource(_openingSoundLoop);
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, false);
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, true);
                    _toggleFinishedTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.ResetTimeInMilliseconds);
                    _pathingToggleTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.ResetTimeInMilliseconds * _moduleData.PercentOpenForPathing);

                    _closingSoundLoop = audioSystem.PlayAudioEvent(_gameObject, _moduleData.SoundClosingGateLoop?.Value, true);
                    _finishedSoundTime = context.Time.TotalTime + TimeSpan.FromMilliseconds(_moduleData.TimeBeforePlayingClosedSound);

                    _state = DoorState.Closing;
                    _toggledColliders = false;
                    _playedFinishedSound = false;
                    break;

                case DoorState.Closing:
                    if (!_toggledColliders && context.Time.TotalTime > _pathingToggleTime)
                    {
                        _gameObject.ShowCollider(_closedGeometry);
                        foreach (var openCollider in _openGeometries)
                        {
                            _gameObject.HideCollider(openCollider);
                        }
                        _toggledColliders = true;
                    }
                    if (!_playedFinishedSound && context.Time.TotalTime > _finishedSoundTime)
                    {
                        audioSystem.DisposeSource(_closingSoundLoop);
                        audioSystem.PlayAudioEvent(_gameObject, _moduleData.SoundFinishedClosingGate?.Value);
                        _playedFinishedSound = true;
                    }
                    if (context.Time.TotalTime > _toggleFinishedTime)
                    {
                        _open = false;
                        _state = DoorState.Idle;
                    }
                    break;
            }
        }
    }

    enum DoorState
    {
        Idle,
        StartOpening,
        Opening,
        StartClosing,
        Closing
    }

    public sealed class GateOpenAndCloseBehaviorModuleData : BehaviorModuleData
    {
        internal static GateOpenAndCloseBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GateOpenAndCloseBehaviorModuleData> FieldParseTable = new IniParseTable<GateOpenAndCloseBehaviorModuleData>
        {
            { "ResetTimeInMilliseconds", (parser, x) => x.ResetTimeInMilliseconds = parser.ParseInteger() },
            { "OpenByDefault", (parser, x) => x.OpenByDefault = parser.ParseBoolean() },
            { "PercentOpenForPathing", (parser, x) => x.PercentOpenForPathing = parser.ParsePercentage() },
            { "SoundOpeningGateLoop", (parser, x) => x.SoundOpeningGateLoop = parser.ParseAudioEventReference() },
            { "SoundClosingGateLoop", (parser, x) => x.SoundClosingGateLoop = parser.ParseAudioEventReference() },
            { "SoundFinishedOpeningGate", (parser, x) => x.SoundFinishedOpeningGate = parser.ParseAudioEventReference() },
            { "SoundFinishedClosingGate", (parser, x) => x.SoundFinishedClosingGate = parser.ParseAudioEventReference() },
            { "TimeBeforePlayingOpenSound", (parser, x) => x.TimeBeforePlayingOpenSound = parser.ParseInteger() },
            { "TimeBeforePlayingClosedSound", (parser, x) => x.TimeBeforePlayingClosedSound = parser.ParseInteger() },
            { "Proxy", (parser, x) => x.Proxy = parser.ParseAssetReference() },
            { "RepelCollidingUnits", (parser, x) => x.RepelCollidingUnits = parser.ParseBoolean() }
        };

        public int ResetTimeInMilliseconds { get; private set; }
        public bool OpenByDefault { get; private set; }
        public Percentage PercentOpenForPathing { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SoundOpeningGateLoop { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SoundClosingGateLoop { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SoundFinishedOpeningGate { get; private set; }
        public LazyAssetReference<BaseAudioEventInfo> SoundFinishedClosingGate { get; private set; }
        public int TimeBeforePlayingOpenSound { get; private set; }
        public int TimeBeforePlayingClosedSound { get; private set; }
        public string Proxy { get; private set; } // what is this?

        [AddedIn(SageGame.Bfme2)]
        public bool RepelCollidingUnits { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GateOpenAndCloseBehavior(gameObject, this);
        }
    }
}
