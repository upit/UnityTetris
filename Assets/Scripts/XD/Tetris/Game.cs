// Author: Upit      Date: 05.09.2019
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XD.TETRIS.BLOCKS;

namespace XD.TETRIS
{
    /// <summary>
    /// Класс отвечает за игровую логику, подсчёт очков, интерпретирует действия, происходящие на поле. Содержит в себе сериализуемые настройки игрового поля и используемых блоков. </summary>
    public class Game : MonoBehaviour
    {
        [Header("Настройки игры")] 
        [Tooltip("Размеры поля в блоках.")] [SerializeField]
        private Vector2Int fieldSize;

        [Tooltip("Ограничение движения горизонтальными границами.")] [SerializeField]
        private bool horizontalBounds = true;

        [Tooltip("Минимальное кол-во заполненных линий, необходимых для удаления.")] [SerializeField]
        private int minLinesToDelete = 1;

        [Tooltip("Спрайт границы поля (стакана).")] [SerializeField]
        private SpriteRenderer borderSprite;

        [Tooltip("Интервал падения фигуры. Чем меньше - тем быстрее падает.")][Range(0.01f, 3.0f)] [SerializeField]
        private float timeStep;


        [Header("Настройки блоков")] 
        [Tooltip("Ссылки на блоки и вероятность их выпадения.")] [SerializeField]
        private BlockSpawn[] blocks;


        [Header("Отображение")] [Tooltip("Ссылка на UI.Text отображения счета.")] [SerializeField]
        private Text scoreText;

        [Tooltip("Анимация при окончании игры")] [SerializeField]
        private Animation gameOverScreen;

        private int score = 0; // Переменная подсчёта очков.
        private IField field; // Игровое поле.
        private Randomizer randomizer;
        private bool isGameOver = false;

        /// <summary> Стандартное событие. Вызывается один раз, когда скрипт активируется. </summary>
        private void Start()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            // Создаем игровое поле с настройками из Inspector, назначаем действия.
            field = new Field(fieldSize, horizontalBounds, minLinesToDelete, borderSprite, this);
            field.OnBlockFixed = SpawnNewBlock;
            field.OnDeleteLine = AddScore;
            field.OnGameOver = GameOver;

            // Инициализируем вероятности выпадения фигур.
            randomizer = new Randomizer();
            for (int i = 0; i < blocks.Length; i++)
            {
                randomizer.AddProbability(blocks[i].dropChance);
            }
            
            SpawnNewBlock(); // Создаем первую фигуру.
            StartCoroutine(UpdateField()); // Начинаем игровой цикл.
        }

        /// <summary> Стандартное событие Unity. Вызывается каждый кадр, если MonoBehaviour.enabled == true </summary>
        private void Update()
        {
            // События ввода используют виртуальные оси и кнопки для удобной настройки через inspector и
            // кроссплатформенного ввода. Ввод можно осуществлять с любого устройства ввода.

            // Выход.
            if (Input.GetButton("Cancel"))
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Если в редакторе - останавливаем выполнение.
                #else
                Application.Quit ();
                #endif
            }
            
            if (isGameOver) // Если игра закончена - не обрабатывать события ввода.
            {
                return;
            }
            
            // Перемещение.
            int horizontal = (int) Input.GetAxis("Horizontal"); // Берем горизонтальную ось, округляя до целого значения
            if (horizontal != 0) // Если горизонтальная ось задействованна...
            {
                field.MoveBlock(new Vector3(horizontal, 0)); // Создаем из оси вектор направления.
                Input.ResetInputAxes(); // Сбрасываем, чтобы не задействовать ось каждый кадр.
            }

            // Вращение.
            if (Input.GetButton("Rotate"))
            {
                field.RotateBlock(); // Поворачиваем блок.
                Input.ResetInputAxes();
            }

            // Ускоренное падение фигуры.
            if (Input.GetButton("Drop"))
            {
                field.MoveBlock(Vector3.down); // Двигаем фигуру вниз на один шаг.
            }
        }

        /// <summary> Функция обеспечивает падение фигур на поле с заданным интервалом времени. </summary>
        private IEnumerator UpdateField()
        {
            do
            {
                field.MoveBlock(Vector3.down);
                yield return new WaitForSeconds(timeStep);
            } while (!isGameOver);
        }

        /// <summary> Создает на поле новую фигуру. </summary>
        private void SpawnNewBlock()
        {
            field.AddBlock(blocks[randomizer.GetRandom()].block);
        }

        /// <summary> Проигрывает анимацию окончания игры. </summary>
        private void GameOver()
        {
            isGameOver = true;
            gameOverScreen.Play();
            #if DEBUG_MODE
            Debug.Log("Game Over");
            #endif
        }

        /// <summary> Добавляет очки и выводит их на экран. </summary>
        private void AddScore()
        {
            score ++;
            scoreText.text = "Score: " + score;
        }
    }
}