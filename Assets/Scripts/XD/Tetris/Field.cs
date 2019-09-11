// Author: Upit      Date: 05.09.2019

using System.Collections;
using UnityEngine;
using XD.TETRIS.BLOCKS;

namespace XD.TETRIS
{
    /// <summary>Класс игрового поля. Представляет его размеры, свойства. Отвечает за передвижение, вращение блоков и т.д. </summary>
    public class Field : IField
    {
        private readonly int  width; // Ширина поля в блоках.
        private readonly int  height; // Высота поля.
        private readonly bool horizontalBounds; // Горизонтальные границы. Если false - блок выходит с другой стороны. 
        private readonly int  minLinesToDelete; // Минимальное кол-во линий, которые нужно собрать для удаления.
        
        // Двумерный массив ссылок на GameObjects, представляющий зафиксированные блоки на игровом поле.
        private readonly GameObject[,] grid;    

        private Block currentBlock;    // Текущая, управляемая фируга.
        private int   currentBlockRotation; // Вращение текущей фигуры: 0 - 0, 1 - 90, 2 - 180, 3 - 270 градусов. 
        private bool  needToFixBlock;  // Нужно ли фиксировать фигуру (достижение дна или фигуры снизу).

        private readonly Vector3Int spawnPos; // Позиция появления новых фигур.

        private System.Action onBlockFixed;    // Действие вызывается при фиксировании фигуры.
        private System.Action onDeleteLine;    // Действие, при удалении линии
        private System.Action onGameOver;      // Действие, когда весь колодец заполнен.

        // Свойства, для вышеописанных полей.
        public System.Action OnBlockFixed
        {
            set { onBlockFixed = value; }
        }
        public System.Action OnDeleteLine
        {
            set { onDeleteLine = value; }
        }
        public System.Action OnGameOver
        {
            set { onGameOver = value; }
        }

        private readonly MonoBehaviour coroutineStarter;    // Ссылка на MonoBehavior для запуска корутин.

        /// <summary> Конструктор игрового поля. </summary>
        /// <param name="size">Размер поля в блоках.</param>
        /// <param name="horizontalBounds">Ограничивает ли поле горизонтальное движение. В противном случае фигуры выходят с другой стороны.</param>
        /// <param name="minLinesToDelete">Минимальное кол-во линий, которые нужно заполнить для удаления. </param>
        /// <param name="borderSprite">Спрайт границ колодца.</param>
        /// <param name="context">Ссылка на MonoBehavior.</param>
        public Field(Vector2Int size, bool horizontalBounds, int minLinesToDelete, SpriteRenderer borderSprite,
            MonoBehaviour context)
        {
            width = size.x;
            height = size.y;
            this.horizontalBounds = horizontalBounds;
            this.minLinesToDelete = minLinesToDelete;
            grid = new GameObject[width, height];
            coroutineStarter = context;
            Transform parent = coroutineStarter.transform;    // Блоки будут появляться внутри Transform.
            
            // Создаем "стакан" игрвого поля из спрайтов.
            for (int y = -1; y < height; y++)
            {
                for (int x = -1; x < width + 1; x++)
                {
                    if (x > -1 && x < width && y > -1)
                    {
                        continue;    // Не создаем спрайты "внутри".
                    }

                    Object.Instantiate(borderSprite, new Vector3(x, y), Quaternion.identity, parent).name = "Border";
                }
            }

            spawnPos = new Vector3Int(width / 2, height, 0); // Позиция новых блоков (середина поля). 
        }

        /// <summary> Создает новую фигуру на поле в месте появления новых фигур. </summary>
        /// <param name="block">Ссылка на ScriptableObject фигуры</param>
        public void AddBlock(Block block)
        {
            currentBlock = block;
            Vector3Int centeredSpawnPos = spawnPos;
            centeredSpawnPos.x -= block.blockData.GetSize().x / 2; // Смещаем позицию, в зависимости от ширины фигуры.
            block.Create().position = centeredSpawnPos;    // Создаем фигуру и перемещаем на начальную позицию
            currentBlockRotation = 0;    // Сбрасываем текущее вращение.
        }

        /// <summary>Перемещает фигуру в заданом направлении.</summary>
        /// <param name="direction">Единичный вектор направления.</param>
        public void MoveBlock(Vector3 direction)
        {
            // Запоминаем текущую позицию блока, затем перемещаем его и проверяем, возможно ли такое перемещение.
            Transform container = currentBlock.BlockTransform;
            Vector3 oldPosition = container.position;
            container.Translate(direction);

            // Если перемещение по каким-то причинам невозможно - откатываем положение назад.
            if (!CanMove(container))
            {
                container.position = oldPosition;
                
                // Если при этом хоть один блок фигуры оказался на дне или в другом блоке - фиксируем фигуру
                if (needToFixBlock)
                {
                    FixBlock(container);
                }
            }
        }

        /// <summary> Поворот фигуры. </summary>
        /// <param name="step">Шаг поворота. По-умолчанию 90 градусов по часовой стрелке.</param>
        public void RotateBlock(int step = 1)
        {
            // Алгоритм схож с перемещением. Сначала вращаем блок, затем проверяем возможность такого вращения.
            // Если по какой-то причине вращение невозможно - перестраиваем блок назад.
            
            int oldRotation = currentBlockRotation;
            currentBlockRotation = (int) Mathf.Repeat(currentBlockRotation + step, 4);
            currentBlock.Rebuild(currentBlockRotation);

            if (!CanMove(currentBlock.BlockTransform))
            {
                currentBlockRotation = oldRotation;
                currentBlock.Rebuild(currentBlockRotation);
            }
        }

        /// <summary>Возвращает возможность перемещения (нахождения) фигуры и ее блоков в заданном месте.</summary>
        /// <param name="transform">Контейнер, содержащий блоки фигуры.</param>
        private bool CanMove(Transform transform)
        {
            Vector3 pos = transform.position;
            Vector2Int size = currentBlock.blockData.GetSize(currentBlockRotation);// Размер, в зависимости от поворота.

            // Если поле блокирует перемещение по горизонтали, то false. В противном случае переносим блоки.
            if (horizontalBounds)
            {
                if (pos.x < 0 || pos.x + size.x > width)
                {
                    return false;
                }
            }
            else
            {
                TranslatePieces(transform);
            }

            // Если достигли дна - фиксируем фигуру и возвращаем невозможность перемещения 
            if (pos.y < 0)
            {
                needToFixBlock = true;
                return false;
            }

            // Проверяем, нахождение блоков фигуры в уже зафиксированных блоках
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 piecePos = transform.GetChild(i).position;
                if (piecePos.y > height - 1)
                {
                    continue;
                }

                int fixedX = (int) piecePos.x;
                int fixedY = (int) piecePos.y;
                if (grid[fixedX, fixedY])
                {
                    needToFixBlock = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary> Перемещает отдельные блоки в противоположную сторону экрана. </summary>
        /// <param name="transform">Transform с блоками внутри.</param>
        private void TranslatePieces(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 piecePos = transform.GetChild(i).position;
                if (piecePos.x > width - 1)// Если позиция блока больше ширины поля.
                {
                    piecePos.x -= width; 
                }
                else if (piecePos.x < 0.0f)// Если меньше начала поля.
                {
                    piecePos.x += width;
                }

                transform.GetChild(i).position = piecePos;
            }
        }

        /// <summary> Фиксирует блоки фигуры на поле. </summary>
        /// <param name="transform">Transform фигуры, содержащий блоки </param>
        private void FixBlock(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Vector3 piecePos = child.position;
                int fixedX = (int) piecePos.x;
                int fixedY = (int) piecePos.y;
                if (fixedY >= height)    // Если блок выше верхней точки - вызываем Game Over.
                {
                    onGameOver.Invoke();
                    return;
                }

                grid[fixedX, fixedY] = child.gameObject;
            }

            // Отсоединяем все блоки от Transform и удаляем его.
            transform.DetachChildren();
            Object.Destroy(transform.gameObject);
            needToFixBlock = false;
            onBlockFixed.Invoke(); // Вызываем действие 

            TryToDeleteLines(minLinesToDelete); // Проверяем возможность удаления заполненных линий.
        }

        /// <summary> Пробует удалить заполненные линии, если такие имеются. </summary>
        /// <param name="minLines">Минимальное кол-во линий, которые необходимо заполнить для удаления.</param>
        /// <param name="startHeight">Начальная высота, с которой начинается проверка.</param>
        private void TryToDeleteLines(int minLines, int startHeight = 0)
        {
            int lines = 0;
            for (int y = startHeight; y < height - 1; y++)
            {
                int count = 0;
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y])  // Если на поле есть зафиксированный блок, прибавляем к счетчику.
                    {
                        count++;
                        if (count >= width)    // Если счетчик равен ширине поля, значит у нас есть линия.
                        {
                            lines++;    // Прибавляем к счетчику линий
                        }

                        if (lines >= minLines)    // Если достигли необходимого числа линий - начинаем удалять
                        {
                            int lineY = y - lines + 1; // Высота самой первой (нижней) заполненной линии.
                            DeleteLine(lineY, () =>
                            {
                                ShiftLinesDown(lineY + 1); // Смещаем линию выше вниз.
                                TryToDeleteLines(1, lineY); // Пробуем удалить смещенную линию.
                                onDeleteLine.Invoke(); // Вызываем действие при удалении линии.
                            });
                            return;
                        }
                    }
                }
            }
        }

        /// <summary> Обёртка для корутины удаления линии. </summary>
        private void DeleteLine(int y, System.Action callback)
        {
            coroutineStarter.StartCoroutine(DeleteLineRoutine(y, callback));
        }

        /// <summary> Удаляет линию из блоков по одному блоку. </summary>
        /// <param name="y">Высота линии на поле.</param>
        /// <param name="callback">Обратный вызов при завершении удаления линии.</param>
        private IEnumerator DeleteLineRoutine(int y, System.Action callback)
        {
            for (int x = 0; x < width; x++)
            {
                Object.Destroy(grid[x, y]);
                yield return new WaitForEndOfFrame();
            }

            callback.Invoke();
        }

        /// <summary> Смещает все линии вниз на один блок по вертикали. </summary>
        /// <param name="startY">Положение первой линии.</param>
        private void ShiftLinesDown(int startY)
        {
            for (int y = startY; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y - 1] = grid[x, y];
                    if (grid[x, y])
                    {
                        grid[x, y].transform.Translate(Vector3.down);
                    }
                }
            }
        }
    }
}