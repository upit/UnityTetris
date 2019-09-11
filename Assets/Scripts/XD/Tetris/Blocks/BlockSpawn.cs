// Author: Upit      Date: 08.09.2019
namespace XD.TETRIS.BLOCKS
{
    /// <summary> Структура отвечает за появление блоков на поле. Содержит ссылку на блок и шанс выпадения блока в процентах.</summary>
    [System.Serializable]
    public struct BlockSpawn
    {
        public Block block;
        public int dropChance;
    }
}