using System.Runtime.CompilerServices;

namespace MathCore.Tags;

/// <summary>Пул меток</summary>
internal static class TagPool
{
    /// <summary>Объект межпотоковой синхронизации</summary>
    private static readonly ReaderWriterLockSlim __Lock = new();

    /// <summary>Словарь меток</summary>
    private static Dictionary<WeakReference, Dictionary<Type, object?>>? __Tags;

    /// <summary>Установить метку указанному объекту</summary>
    /// <param name="Obj">Целевой объект</param>
    /// <param name="Tag">Добавляемая метка</param>
    public static void SetTag(object Obj, object? Tag)
    {
        if (Obj is null) throw new ArgumentNullException(nameof(Obj));

        __Lock.EnterWriteLock();

        try
        {
            var tags = __Tags ??= new();

            foreach (var w in tags.Keys.Where(w => !w.IsAlive).ToArray())
                tags.Remove(w);

            //var reference = .Find(Selector);

            WeakReference? reference = null;
            foreach (var key in tags.Keys)
                if (ReferenceEquals(Obj, key.Target))
                {
                    reference = key;
                    break;
                }

            Dictionary<Type, object?> dictionary;
            if (reference is { })
                dictionary = tags[reference];
            else
                tags.Add(new(Obj), dictionary = new());

            var type = Tag?.GetType() ?? typeof(object);

            if (dictionary.Keys.Contains(type))
                dictionary[type] = Tag;
            else
                dictionary.Add(type, Tag);
        }
        finally
        {
            __Lock.ExitWriteLock();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAlive(WeakReference w) => !w.IsAlive;

    /// <summary>Получить метку указанного типа для указанного объекта</summary>
    /// <typeparam name="TTagType">Тип объекта-метки</typeparam>
    /// <param name="o">Целевой объект</param>
    /// <returns>Объект-метка</returns>
    public static TTagType? Tag<TTagType>(object o)
    {
        if (o is null) throw new ArgumentNullException(nameof(o));

        __Lock.EnterReadLock();

        try
        {
            var tags = __Tags;

            if (tags is null) return default;

            foreach (var w in tags.Keys)
                if (w.IsAlive)
                    tags.Remove(w);

            WeakReference? reference = null;
            foreach (var key in tags.Keys)
                if (o.Equals(key.Target))
                {
                    reference = key;
                    break;
                }

            return reference is null
                ? default
                : tags[reference].TryGetValue(typeof(TTagType), out var result)
                    ? (TTagType?)result
                    : default;
        }
        finally
        {
            __Lock.ExitReadLock();
        }
    }
}