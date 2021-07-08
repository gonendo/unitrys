using System.Collections.Generic;
using UnityEngine;
namespace unitrys{
    public class DebugData{
        public int level;
        public string[] history;
        public int[] blocks;

        public void Load(List<Piece> history, List<Block> blocks){
            Mode mode = Game.GetMode();
            foreach(string pieceName in this.history){
                switch(pieceName){
                    case Piece.I:
                        history.Add(new Piece(Piece.I));
                        break;
                    case Piece.Z:
                        history.Add(new Piece(Piece.Z));
                        break;
                    case Piece.S:
                        history.Add(new Piece(Piece.S));
                        break;
                    case Piece.J:
                        history.Add(new Piece(Piece.J));
                        break;
                    case Piece.L:
                        history.Add(new Piece(Piece.L));
                        break;
                    case Piece.O:
                        history.Add(new Piece(Piece.O));
                        break;
                    case Piece.T:
                        history.Add(new Piece(Piece.T));
                        break;
                }
            }
            int x=0;
            int y=0;
            for(int i=0; i < this.blocks.Length; i++){
                int value = this.blocks[i];
                if(value!=0){
                    Block block = blocks.Find(block => block.x == x && block.y == mode.GRID_HEIGHT-1 - y);
                    Color color = new Color();
                    switch(value){
                        case 1:
                            color = mode.theme.GetColor(Piece.I);
                            break;
                        case 2:
                            color = mode.theme.GetColor(Piece.Z);
                            break;
                        case 3:
                            color = mode.theme.GetColor(Piece.S);
                            break;
                        case 4:
                            color = mode.theme.GetColor(Piece.J);
                            break;
                        case 5:
                            color = mode.theme.GetColor(Piece.L);
                            break;
                        case 6:
                            color = mode.theme.GetColor(Piece.O);
                            break;
                        case 7:
                            color = mode.theme.GetColor(Piece.T);
                            break;
                    }
                    block.empty = false;
                    block.locked = true;
                    block.color = color;
                }
                if((i+1) % mode.GRID_WIDTH == 0){
                    y++;
                }
                x++;
                if(x==mode.GRID_WIDTH){
                    x=0;
                }
            }
        }
    }
}