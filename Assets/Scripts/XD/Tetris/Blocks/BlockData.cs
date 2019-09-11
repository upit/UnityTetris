// Author: Upit      Date: 08.09.2019
using UnityEngine;

namespace XD.TETRIS.BLOCKS
{
    /// <summary> Структура хранит в себе сериализованные данные о фигуре в виде массива блоков. </summary>
    [System.Serializable]
    public struct BlockData : ISerializationCallbackReceiver
    {
        [SerializeField] private Vector2Int size;    // Размер фигуры в блоках.
        [SerializeField] private bool[]     data;    // Массив данных о фигуре. true - блок есть, false - нет.
        
        // Заранее рассчитанные массивы блоков в разных положениях (повороты). Unity не сериализует многомерные массивы. 
        private bool[][,]                states;

        /// <summary> Конструктор создает пустую фигуру заданного размера. </summary>
        /// <param name="width">Ширина фигуры в блоках.</param>
        /// <param name="height">Высота фигуры.</param>
        public BlockData(int width, int height)
        {
            size = new Vector2Int(width, height);
            data = new bool[width * height]; // Данные сериализуются в одномерный массив. 
            states = new bool[4][,]; // 4 поворота фигуры. Двумерный массив.
            InitStates(); // Предварительный расчёт положений блоков фигуры при поворотах.
        }

        /// <summary> Рассчитывает положения фигуры при 4 фиксированных углах поворота: 0, 90, 180, 270 градусов. </summary>
        private void InitStates()
        {
            int width = size.x;
            int height = size.y;
            states = new bool[4][,];

            states[0] = new bool[width, height];
            states[1] = new bool[height, width];
            states[2] = new bool[width, height];
            states[3] = new bool[height, width];
            
            // Преобразуем одномерный массив в 4 двумерных массива.
            int counter = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    states[0][x, y] = data[counter];                             // 0
                    states[1][y, width - x - 1] = data[counter];                 // 90
                    states[2][width - x - 1, height - y - 1] = data[counter];    // 180
                    states[3][height - y - 1, x] = data[counter];                // 270
                    counter++;
                }
            }
        }

        /// <summary>Возвращает размер фигуры в зависимости от угла поворота. </summary>
        /// <param name="state">Фиксированный угол поворота 0 - 0, 1 - 90, 2 - 180, 3 - 270</param>
        public Vector2Int GetSize(int state = 0)
        {
            if (state == 1 || state == 3)    // Если 90 или 270 - меняем ширину и высоту местами.
            {
                return new Vector2Int(size.y, size.x);
            }

            return size;
        }

        /// <summary>Создает новый одномерный массив данных о фигуре из двумерного массива. </summary>
        /// <param name="newData">Двумерный массив данных о фигуре.</param>
        public void Set(bool[,] newData)
        {
            int counter = 0;
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    data[counter] = newData[x, y];
                    counter++;
                }
            }
        }
        
        /// <summary>Возвращает двумерный массив блоков в одном из положений (угла поворота) фигуры.</summary>
        /// <param name="state">Фиксированный угол поворота 0 - 0, 1 - 90, 2 - 180, 3 - 270</param>
        public bool[,] Get(int state = 0)
        {
            return states[state];
        }

#region ISerializationCallbackReceiver

        /// <summary> Реализация члена интерфейса "ISerializationCallbackReceiver". </summary>
        public void OnBeforeSerialize()
        {
        }

        /// <summary> Массивы блоков при разных поворотах. Рассчитывается при десериализации. </summary>
        public void OnAfterDeserialize()
        {
            InitStates();    //  Пересчитываем двумерные массивы фигур, так как они не сериализуются  
        }

#endregion
    }
}