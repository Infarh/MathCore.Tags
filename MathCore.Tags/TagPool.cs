using System;
using System.Collections.Generic;
using System.Threading;

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

            var to_remove = new List<WeakReference>(tags.Keys.Count);
            foreach (var w in tags.Keys)
                if (!w.IsAlive)
                    to_remove.Add(w);

            foreach (var dead_ref in to_remove)
                tags.Remove(dead_ref);

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

            if (dictionary.ContainsKey(type))
                dictionary[type] = Tag;
            else
                dictionary.Add(type, Tag);
        }
        finally
        {
            __Lock.ExitWriteLock();
        }
    }

    /// <summary>Получить метку указанного типа для указанного объекта</summary>
    /// <typeparam name="T">Тип объекта-метки</typeparam>
    /// <param name="o">Целевой объект</param>
    /// <returns>Объект-метка</returns>
    public static T? Tag<T>(object o)
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
                : tags[reference].TryGetValue(typeof(T), out var result)
                    ? (T?)result
                    : default;
        }
        finally
        {
            __Lock.ExitReadLock();
        }
    }
}