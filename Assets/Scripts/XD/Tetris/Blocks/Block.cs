// Author: Upit      Date: 08.09.2019

using UnityEngine;


namespace XD.TETRIS.BLOCKS
{
    /// <summary> Класс отвечает за создание на поле блоков из массивов данных о блоках. </summary>
    [CreateAssetMenu(fileName = "Block", menuName = "Tetris Block", order = 51)]
    public class Block : ScriptableObject
    {
        public BlockData blockData; // Данные о блоке.

        [SerializeField]
        private SpriteRenderer pieceSprite; // Ссылка на спрайт, из которых будут состоять куски блоков.

        public Transform BlockTransform { get; private set; } // Transform блока, в который помещаются "куски".

        /// <summary> Создает GameObject фигуры с "кусками" внутри и возвращает его Transform </summary>
        public Transform Create()
        {
            BlockTransform = new GameObject("Block container").transform;
            bool[,] data = blockData.Get();
            Vector2Int size = blockData.GetSize();
            
            // Проходим по двумерному массиву данных блока. Если элемент == true, то создаем новый GameObject
            // с координатыми, равными индексам элемента в массиве.
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (data[x, y])
                    {
                        Instantiate(pieceSprite, new Vector3(x, y), Quaternion.identity, BlockTransform);
                    }
                }
            }

            return BlockTransform;
        }

        /// <summary> Перестраивает GameObject блока в зависимости от угла поворота. </summary>
        /// <param name="rotation">Угол поворота 0 - 0, 1 - 90, 2 - 180, 3 - 270.</param>
        public void Rebuild(int rotation)
        {
            Vector2Int size = blockData.GetSize(rotation);
            bool[,] pieces = blockData.Get(rotation);
            int counter = 0;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    if (pieces[x, y])
                    {
                        BlockTransform.GetChild(counter).localPosition = new Vector3(x, y);
                        counter++;
                    }
                }
            }
        }
    }
}