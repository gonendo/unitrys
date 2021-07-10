/*
Based on https://tetris.wiki/Arika_Rotation_System
*/
using System.Collections.Generic;
using UnityEngine;
namespace unitrys{
    public class ARS : IRotationSystem{
        private static int[] I_state = {0,0,0,0,
                                        1,1,1,1,
                                        0,0,0,0,
                                        0,0,0,0};
        private static int[] Z_state = {0,0,0,0,
                                        1,1,0,0,
                                        0,1,1,0,
                                        0,0,0,0};
        private static int[] S_state = {0,0,0,0,
                                        0,1,1,0,
                                        1,1,0,0,
                                        0,0,0,0};
        private static int[] J_state = {0,0,0,0,
                                        1,1,1,0,
                                        0,0,1,0,
                                        0,0,0,0};
        private static int[] L_state = {0,0,0,0,
                                        1,1,1,0,
                                        1,0,0,0,
                                        0,0,0,0};
        private static int[] O_state = {0,0,0,0,
                                        0,1,1,0,
                                        0,1,1,0,
                                        0,0,0,0};
        private static int[] T_state = {0,0,0,0,
                                        1,1,1,0,
                                        0,1,0,0,
                                        0,0,0,0};

        private static int[,] _kicks_table ={{1,0},{-1,0}};

        int [] IRotationSystem.GetInitialState(string pieceName){
            switch(pieceName){
                case Piece.I:
                    return I_state;
                case Piece.Z:
                    return Z_state;
                case Piece.S:
                    return S_state;
                case Piece.J:
                    return J_state;
                case Piece.L:
                    return L_state;
                case Piece.O:
                    return O_state;
                case Piece.T:
                    return T_state;
            }
            return null;
        }

        int IRotationSystem.GetNumberOfRotationStates(string pieceName){
            switch(pieceName){
                case Piece.I:
                case Piece.S:
                case Piece.Z:
                    return 2;
            }
            return 4;
        }

        private List<Block> Rotate(List<Block> blocks, Piece piece, string actionId){
            if(piece.name==Piece.O){
                return null;
            }
            int size = GetPieceSize(piece.name);

            List<int[]> newBlocksPositions = new List<int[]>();
            List<int[]> newBlocksBoundingBoxCoordinates = new List<int[]>();

            foreach(Block block in piece.blocks){
                int[] coordinates;
                piece.boundingBoxCoordinates.TryGetValue(block, out coordinates);

                int newCoordX = actionId == Controls.ROTATE_RIGHT_ACTION_ID ? 1 - (coordinates[1] - (size - 2)) : coordinates[1];
                int newCoordY = actionId == Controls.ROTATE_RIGHT_ACTION_ID ? coordinates[0] : 1 - (coordinates[0] - (size - 2));

                int x2 = block.x + (newCoordX - coordinates[0]);
                int y2 = block.y - (newCoordY - coordinates[1]);
                
                newBlocksPositions.Add(new int[]{x2, y2});
                newBlocksBoundingBoxCoordinates.Add(new int[]{newCoordX, newCoordY});
            }

            //align the piece (except I) towards the bottom of its bounding box
            if(piece.name!=Piece.I){
                bool isAligned = false;
                foreach(int[] coords in newBlocksBoundingBoxCoordinates){
                    if(coords[1]>=2){
                        isAligned = true;
                        break;
                    }
                }
                if(!isAligned){
                    for(int i=0; i < newBlocksPositions.Count; i++){
                        newBlocksPositions[i][1]-=1;
                        newBlocksBoundingBoxCoordinates[i][1]+=1;
                    }
                }
            }
            else{
                //shift I Piece
                if(piece.rotationState==-1 || piece.rotationState==1){
                    for(int i=0; i < newBlocksPositions.Count; i++){
                        if(newBlocksBoundingBoxCoordinates[i][1]==2){
                            newBlocksPositions[i][1]+=1;
                            newBlocksBoundingBoxCoordinates[i][1]-=1;
                        }
                    }
                }
                else if(actionId == Controls.ROTATE_LEFT_ACTION_ID && piece.rotationState==0){
                    for(int i=0; i < newBlocksPositions.Count; i++){
                        newBlocksPositions[i][0]+=1;
                        newBlocksBoundingBoxCoordinates[i][0]+=1;
                    }
                }
            }
            //shift Z Piece to the right
            if(actionId == Controls.ROTATE_RIGHT_ACTION_ID && piece.name==Piece.Z && piece.rotationState==0){
                for(int i=0; i < newBlocksPositions.Count; i++){
                    newBlocksPositions[i][0]+=1;
                    newBlocksBoundingBoxCoordinates[i][0]+=1;
                }
            }
            //shift S Piece to the left
            if(actionId == Controls.ROTATE_LEFT_ACTION_ID && piece.name==Piece.S && piece.rotationState==0){
                for(int i=0; i < newBlocksPositions.Count; i++){
                    newBlocksPositions[i][0]-=1;
                    newBlocksBoundingBoxCoordinates[i][0]-=1;
                }
            }
            //shift J,L,T Pieces
            if(piece.name==Piece.J || piece.name==Piece.L || piece.name==Piece.T){
                int state = actionId == Controls.ROTATE_RIGHT_ACTION_ID ? 2 : -2;
                if(piece.rotationState == state){
                    for(int i=0; i < newBlocksPositions.Count; i++){
                        newBlocksPositions[i][0] += actionId == Controls.ROTATE_RIGHT_ACTION_ID ? 1 : -1;
                        newBlocksBoundingBoxCoordinates[i][0] += actionId == Controls.ROTATE_RIGHT_ACTION_ID ? 1 : -1;
                    }
                }
            }

            //try to rotate
            List<Block> newBlocks=new List<Block>();
            Dictionary<Block,int[]> newBoundingBoxCoordinates = new Dictionary<Block, int[]>();

            if(CheckRotation(piece, newBlocksPositions)){
                SetRotation(newBlocksPositions, newBlocksBoundingBoxCoordinates, newBlocks, newBoundingBoxCoordinates);
            }
            else{
                //try wallkicks if basic rotation failed
                if(piece.name==Piece.I){
                    return null;
                }
                else{
                    //center column rule for L,J,T Pieces
                    if(piece.name==Piece.L || piece.name==Piece.J || piece.name==Piece.T){
                        int minX=Game.GetMode().GRID_WIDTH-1;
                        int minY=Game.GetMode().GRID_HEIGHT-1;
                        foreach(Block block in piece.blocks){
                            minX = Mathf.Min(minX, block.x);
                            minY = Mathf.Min(minY, block.y);
                        }
                        //search center blocks in 3x3 bounding box
                        int x = minX;
                        int y = minY+2;
                        bool firstBlockInCenter=true;
                        for(int i=0; i < 3; i++){
                            for(int j=0; j < 3; j++){
                                Block blk = Game.GetMode().blocks.Find(block => block.x == x+j && block.y == y-i);
                                if(blk!=null && !blk.empty && piece.blocks.IndexOf(blk)==-1){
                                    //first block is a center block
                                    if(j==1){
                                        return null;
                                    }
                                    else{
                                        firstBlockInCenter=false;
                                        break;
                                    }
                                }
                            }
                            if(!firstBlockInCenter){
                                break;
                            }
                        }
                    }

                    bool kicked=false;
                    List<int[]> testPositions;
                    int numTests = _kicks_table.GetUpperBound(0)+1;
                    for(int i=0; i < numTests; i++){
                        int xOffset = (int)_kicks_table.GetValue(i, 0);
                        int yOffset = (int)_kicks_table.GetValue(i, 1);

                        testPositions = new List<int[]>();
                        for(int j=0; j < newBlocksPositions.Count; j++){
                            testPositions.Add(new int[]{newBlocksPositions[j][0], newBlocksPositions[j][1]});
                        }

                        for(int j=0; j < testPositions.Count; j++){
                            testPositions[j][0]+=xOffset;
                            testPositions[j][1]+=yOffset;
                        }
                        if(CheckRotation(piece, testPositions)){
                            SetRotation(testPositions, newBlocksBoundingBoxCoordinates, newBlocks, newBoundingBoxCoordinates);
                            kicked = true;
                            break;
                        }
                    }

                    if(!kicked){
                        return null;
                    }
                }
            }

            piece.boundingBoxCoordinates = newBoundingBoxCoordinates;
            return newBlocks;
        }

        List<Block> IRotationSystem.RotateClockWise(List<Block> blocks, Piece piece){
            return Rotate(blocks, piece, Controls.ROTATE_RIGHT_ACTION_ID);
        }

        List<Block> IRotationSystem.RotateCounterClockWise(List<Block> blocks, Piece piece){
            return Rotate(blocks, piece, Controls.ROTATE_LEFT_ACTION_ID);
        }

        bool IRotationSystem.AllowStepReset(){
            return true;
        }

        bool IRotationSystem.HardDropLock(){
            return false;
        }

        bool IRotationSystem.SoftDropLock(){
            return true;
        }

        private int GetPieceSize(string pieceName){
            switch(pieceName){
                case Piece.O:
                    return 2;
                case Piece.I:
                    return 4;
            }
            return 3;
        }

        private bool CheckRotation(Piece piece, List<int[]> positions){
            for(int i=0; i < positions.Count; i++){
                Block newBlock = Game.GetMode().blocks.Find(b => b.x == positions[i][0] && b.y == positions[i][1]);
                if(piece.blocks.IndexOf(newBlock)==-1 && (newBlock== null || !newBlock.empty)){
                    return false;
                }
            }
            return true;
        }

        private void SetRotation(List<int[]> positions, 
            List<int[]> coordinates, 
            List<Block> newBlocks, 
            Dictionary<Block,int[]> newBoundingBoxCoordinates)
        {
            for(int i=0; i < positions.Count; i++){
                Block newBlock = Game.GetMode().blocks.Find(b => b.x == positions[i][0] && b.y == positions[i][1]);
                newBlocks.Add(newBlock);
                newBoundingBoxCoordinates.Add(newBlock, new int[]{coordinates[i][0], coordinates[i][1]});
            }
        }
    }
}