using UnityEngine;
using UnityEditor;
namespace unitrys{
    public class Theme{
        public Texture GetTexture(){
            string[] guids = AssetDatabase.FindAssets("block", new[] {"Assets/Textures"});
            return AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        public Color GetColor(string pieceName){
            switch(pieceName){
                case Piece.I:
                    return Color.red;
                case Piece.Z:
                    return Color.green;
                case Piece.S:
                    return Color.magenta;
                case Piece.J:
                    return Color.blue;
                case Piece.L:
                    return new Color(1f, 0.6471f, 0f, 1f); //orange
                case Piece.O:
                    return Color.yellow;
                case Piece.T:
                    return Color.cyan;
            }
            return new Color();
        }
    }
}