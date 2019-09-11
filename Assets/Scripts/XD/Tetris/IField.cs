// Author: Upit      Date: 05.09.2019
using UnityEngine;
using XD.TETRIS.BLOCKS;

namespace XD.TETRIS
{
    /// <summary> Интерфейс игрового поля. </summary>
    public interface IField
    {
        System.Action OnBlockFixed { set; }
        System.Action OnDeleteLine { set; }
        System.Action OnGameOver { set; }
        void AddBlock(Block block);
        void MoveBlock(Vector3 direction);
        void RotateBlock(int step = 1);
    }
}