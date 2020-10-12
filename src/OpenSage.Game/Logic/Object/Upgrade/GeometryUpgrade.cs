﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class GeometryUpgrade : UpgradeModule
    {
        private readonly GeometryUpgradeModuleData _moduleData;

        internal GeometryUpgrade(GameObject gameObject, GeometryUpgradeModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            // TODO:
            //foreach (var showGeometry in _moduleData.ShowGeometry)
            //{

            //}
            //foreach (var hideGeometry in _moduleData.HideGeometry)
            //{

            //}
        }
    }


    public sealed class GeometryUpgradeModuleData : UpgradeModuleData
    {
        internal static GeometryUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GeometryUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<GeometryUpgradeModuleData>
            {
                { "ShowGeometry", (parser, x) => x.ShowGeometry = parser.ParseAssetReferenceArray() },
                { "HideGeometry", (parser, x) => x.HideGeometry = parser.ParseAssetReferenceArray() },
                { "WallBoundsMesh", (parser,x) => x.WallBoundsMesh = parser.ParseAssetReference() },
                { "RampMesh1", (parser, x) => x.RampMesh1 = parser.ParseAssetReference() },
                { "RampMesh2", (parser, x) => x.RampMesh2 = parser.ParseAssetReference() },
            });

        public string[] ShowGeometry { get; private set; }
        public string[] HideGeometry { get; private set; }
        public string WallBoundsMesh { get; private set; }
        public string RampMesh1 { get; private set; }
        public string RampMesh2 { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GeometryUpgrade(gameObject, this);
        }
    }
}
