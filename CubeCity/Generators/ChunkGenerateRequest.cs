using CubeCity.Tools;

namespace CubeCity.Generators
{
    /*
     * Оптимизация:
     * Done: 1. Не рисовать вертексы снизу
     * 2. Генерировать чанки вокруг отображаемых чанков
     *    (для этого надо разделить очередь генерации и очередь привязки данных)
     * 3. Ну и обрезать невидимые блоки
     */

    public readonly struct ChunkGenerateRequest
    {
        public Vector2Int Position { get; }

        public ChunkGenerateRequest(Vector2Int position)
        {
            Position = position;
        }
    }
}
