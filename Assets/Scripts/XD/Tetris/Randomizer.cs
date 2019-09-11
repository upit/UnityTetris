// Author: Upit      Date: 08.09.2019

using System.Collections.Generic;
using UnityEngine;

namespace XD.TETRIS
{
    /// <summary> Класс выдает индексы элементов в зависимости от вероятности их выпадения. По-одному добавляется элемент и шанс его выпадения.</summary>
    public class Randomizer
    {
        private readonly List<int> probabilities = new List<int>(); // List, содержащий в себе вероятности выпадения.
        private int totalProbability = 0; // Сумма всех вероятностей выпадения. 

        /// <summary> Добавляет одну вероятность в общий пул вероятностей. </summary>
        /// <param name="value">Вероятность в процентах.</param>
        public void AddProbability(int value)
        {
            probabilities.Add(value);
            totalProbability += value;
        }

        /// <summary> Выбирает псевдослучайным образом элемент, в зависимости от вероятности его выпадения и возвращает его индекс из списка. </summary>
        public int GetRandom()
        {
            int chance = Random.Range(0, totalProbability); // Ключевое число для выбора вероятности.
            for (int i = 0; i < probabilities.Count; i++) // Проходим по всем вероятностям.
            {
                if (probabilities[i] < chance)    // Если вероятности недостаточно
                {
                    chance -= probabilities[i];    // Отнимаем  от ключевого числа эту вероятность
                }
                else
                {
                    return i;     // Если вероятности достаточно - возвращаем её индекс.
                }
            }

            return -1;
        }
    }
}