using System.Collections.Generic;
using UnityEngine;

namespace unitrys{
    public class Piece
    {
        public const string EMPTY = "EMPTY";
        public const string I = "I";
        public const string Z = "Z";
        public const string S = "S";
        public const string J = "J";
        public const string L = "L";
        public const string O = "O";
        public const string T = "T";

        private string _name;
        private string _soundId;
        private int _rotationState;
        private bool _locked;
        private bool _visible;
        private bool _rendered=false;
        private Color _color;
        private List<Block> _blocks;
        private Dictionary<Block,int[]> _boundingBoxCoordinates;

        public Piece(string name){
            _name = name;
            _soundId = Sounds.GetPieceSoundId(_name);
            _blocks = new List<Block>();
            _boundingBoxCoordinates = new Dictionary<Block,int[]>();
        }

        public string name{
            get{
                return _name;
            }
        }

        public string soundId{
            get{
                return _soundId;
            }
        }

        public int rotationState{
            get{
                return _rotationState;
            }
            set{
                _rotationState = value;
            }
        }

        public bool locked{
            get{
                return _locked;
            }
            set{
                _locked = value;
                foreach(Block block in _blocks){
                    block.locked = _locked;
                }
            }
        }

        public Color color{
            get{
                return _color;
            }
            set{
                _color = value;
            }
        }

        public bool visible{
            get{
                return _visible;
            }
            set{
                _visible = value;
            }
        }

        public bool rendered{
            get{
                return _rendered;
            }
            set{
                _rendered = value;
            }
        }

        public List<Block> blocks{
            get{
                return _blocks;
            }
            set{
                _blocks = value;
            }
        }

        public Dictionary<Block,int[]> boundingBoxCoordinates{
            get{
                return _boundingBoxCoordinates;
            }
            set{
                _boundingBoxCoordinates = value;
            }
        }

        private bool Move(int xOffset, int yOffset){
            foreach(Block block in _blocks){
                Block nb = Game.GetMode().blocks.Find(b => b.x == block.x+xOffset && b.y == block.y+yOffset);
                if(_blocks.IndexOf(nb)==-1){
                    if (nb!=null){
                        if(!nb.empty){
                            return false;
                        }
                    }
                    else{
                        return false;
                    }
                }
            }

            List<Block> newBlocks = new List<Block>();
            Dictionary<Block,int[]> newBoundingBoxCoordinates = new Dictionary<Block, int[]>();

            foreach(Block block in _blocks){
                if(newBlocks.IndexOf(block)==-1){
                    block.empty = true;
                }
                Block nb = Game.GetMode().blocks.Find(b => b.x == block.x+xOffset && b.y == block.y+yOffset);
                if (nb!=null){
                    if(_visible){
                        nb.color = _color;
                        nb.empty = false;
                        nb.locked = locked;
                    }
                    newBlocks.Add(nb);

                    int[] coordinates;
                    _boundingBoxCoordinates.TryGetValue(block, out coordinates);
                    newBoundingBoxCoordinates.Add(nb, new int[]{coordinates[0], coordinates[1]});
                }
            }

            _blocks = newBlocks;
            _boundingBoxCoordinates = newBoundingBoxCoordinates;
            return true;
        }

        public bool MoveDown(){
            return Move(0, -1);
        }
        public bool MoveLeft(){
            return Move(-1, 0);
        }
        public bool MoveRight(){
            return Move(1, 0);
        }
    }
}