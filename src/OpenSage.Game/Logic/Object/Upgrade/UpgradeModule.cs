﻿using System.IO;
using System.Linq;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public abstract class UpgradeModule : BehaviorModule
    {
        protected readonly GameObject _gameObject;
        private readonly UpgradeModuleData _moduleData;
        protected bool _triggered;
        private bool _initial = true;

        internal bool Triggered => _triggered;

        internal UpgradeModule(GameObject gameObject, UpgradeModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _triggered = _moduleData.StartsActive;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var triggered = false;

            if (_moduleData.TriggeredBy != null)
            {
                foreach (var trigger in _moduleData.TriggeredBy)
                {
                    triggered = _gameObject.UpgradeAvailable(trigger.Value);

                    if (triggered && _moduleData.RequiresAllTriggers == false)
                    {
                        break;
                    }

                    if (!triggered && _moduleData.RequiresAllTriggers)
                    {
                        break;
                    }
                }
            }

            var conflictCount = 0;

            if (_moduleData.ConflictsWith != null)
            {
                foreach (var conflict in _moduleData.ConflictsWith)
                {
                    var hasUpgrade = _gameObject.UpgradeAvailable(conflict.Value);

                    if (hasUpgrade)
                    {
                        if (_moduleData.RequiresAllConflictingTriggers)
                        {
                            conflictCount++;
                            if (conflictCount == _moduleData.ConflictsWith.Count())
                            {
                                triggered = false;
                            }
                        }
                        else
                        {
                            triggered = false;
                        }
                    }
                }
            }

            if (triggered != _triggered || _initial)
            {
                _initial = false;
                _triggered = triggered;
                OnTrigger(context, _triggered);
            }
        }

        internal virtual void OnTrigger(BehaviorUpdateContext context, bool triggered) { }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknownByte1 = reader.ReadByte();
            var unknownByte2 = reader.ReadByte();
        }
    }

    public abstract class UpgradeModuleData : BehaviorModuleData
    {
        internal static readonly IniParseTable<UpgradeModuleData> FieldParseTable = new IniParseTable<UpgradeModuleData>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseUpgradeReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseUpgradeReferenceArray() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() },
            { "RequiresAllConflictingTriggers", (parser, x) => x.RequiresAllConflictingTriggers = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "Description", (parser, x) => x.Description = parser.ParseLocalizedStringKey() },
            { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "ActiveDuringConstruction", (parser, x) => x.ActiveDuringConstruction = parser.ParseBoolean() },
        };

        public LazyAssetReference<UpgradeTemplate>[] TriggeredBy { get; private set; }
        public LazyAssetReference<UpgradeTemplate>[] ConflictsWith { get; private set; }
        public bool RequiresAllTriggers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RequiresAllConflictingTriggers { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool StartsActive { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Description { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public AnimAndDuration CustomAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ActiveDuringConstruction { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class AnimAndDuration
    {
        internal static AnimAndDuration Parse(IniParser parser)
        {
            var result = new AnimAndDuration
            {
                AnimState = parser.ParseAttributeEnum<ModelConditionFlag>("AnimState"),
                AnimTime = parser.ParseAttributeInteger("AnimTime")
            };

            parser.ParseAttributeOptional("TriggerTime", parser.ParseInteger, out var triggerTime);
            result.TriggerTime = triggerTime;
            return result;
        }

        public ModelConditionFlag AnimState { get; private set; }
        public int AnimTime { get; private set; }
        public int TriggerTime { get; private set; }
    }
}
