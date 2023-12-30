using Sludge.Utility;
using System;
using System.Collections.Generic;

namespace Sludge.Shared
{
    public class LevelTilemapData
    {
        public int TilesX;
        public int TilesY;
        public int TilesW;
        public int TilesH;
        public List<int> TileIndices = new List<int>();
    }

    public class LevelData
    {
        public string LevelName;
        public string ColorSchemeName;
        public PlayerProgress.LevelNamespace Namespace;
        public int LevelId = 0;

        public LevelDataTransform PlayerTransform = new LevelDataTransform();
        public LevelTilemapData WallTilemap = new LevelTilemapData();
        public LevelTilemapData PillTilemap = new LevelTilemapData();
        public List<LevelDataObject> Objects = new List<LevelDataObject>();

        public string FileNameFromNamespaceAndId() => $"{Namespace}-{LevelId:000}";

        public PlayerProgress.LevelNamespace NameSpaceFromFilename(string filename)
        {
            string nsPart = filename[..filename.IndexOf('-')];
            return Enum.Parse<PlayerProgress.LevelNamespace>(nsPart);
        }

        public int IdFromFilename(string filename)
        {
            string idPart = filename[(filename.IndexOf('-') + 1)..];
            return int.Parse(idPart);
        }

        public void SetNamespaceAndIdFromFilename(string filename)
        {
            Namespace = NameSpaceFromFilename(filename);
            LevelId = IdFromFilename(filename);
        }
    }
}
