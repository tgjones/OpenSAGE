﻿using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class FoundationAIUpdate : AIUpdate
    {
        private readonly FoundationAIUpdateModuleData _moduleData;
        private TimeSpan _waitUntil;
        private int _updateInterval;

        //TODO: rather notify this when the corresponding order is processed and update again when the object is dead/destroyed
        internal FoundationAIUpdate(GameObject gameObject, FoundationAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            _updateInterval = 500; // we do not have to check every frame
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (context.Time.TotalTime < _waitUntil)
            {
                return;
            }

            _waitUntil = context.Time.TotalTime + TimeSpan.FromMilliseconds(_updateInterval);

            var collidingObjects = context.GameContext.Quadtree.FindNearby(GameObject, GameObject.Transform, GameObject.RoughCollider.WorldBounds.Radius);

            foreach (var collidingObject in collidingObjects)
            {
                if (collidingObject.Definition.KindOf.Get(ObjectKinds.Structure))
                {
                    GameObject.IsSelectable = false;
                    GameObject.Hidden = true;
                    return;
                }
            }

            GameObject.IsSelectable = true;
            GameObject.Hidden = false;
        }
    }


    public sealed class FoundationAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static FoundationAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<FoundationAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<FoundationAIUpdateModuleData>
            {
                { "BuildVariation", (parser, x) => x.BuildVariation = parser.ParseInteger() },
            });

        [AddedIn(SageGame.Bfme2)]
        public int BuildVariation { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new FoundationAIUpdate(gameObject, this);
        }
    }
}
